using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using InRule.Authoring.Commanding;
using InRule.Authoring.ComponentModel;
using InRule.Authoring.Media;
using InRule.Authoring.Windows;
using InRule.Common.Utilities;
using InRule.Repository;

namespace UndoExtension
{
    public class UndoExtension : ExtensionBase
    {
        private IObserver<Unit> UndoButtonSubject => undoSubject.AsObserver();
        private VisualDelegateCommand undoCommand;
        private VisualDelegateCommand redoCommand;

        private const int BufferSize = 5;
        private IObservable<Unit> UndoClicked => undoSubject.AsObservable();
        private readonly Subject<Unit> undoSubject = new Subject<Unit>();
        private readonly Subject<Unit> redoSubject = new Subject<Unit>();
        
        private readonly BehaviorSubject<bool> undoInProgress = new BehaviorSubject<bool>(false);
        private readonly CompositeDisposable subscriptionsDisposable = new CompositeDisposable();

        private readonly ObservableDonutStack<UndoHistoryItem> undoBuffer = new ObservableDonutStack<UndoHistoryItem>(BufferSize);
        private readonly ObservableDonutStack<UndoHistoryItem> redoBuffer = new ObservableDonutStack<UndoHistoryItem>(BufferSize);

        public UndoExtension()
            : base("UndoExtension", "Provides undo functionality on defs", new Guid("{CA41B187-3B1F-48A2-94A2-AAAAF047D453}"))
        {

        }

        public override void Disable()
        {
            if (!subscriptionsDisposable.IsDisposed)
            {
                subscriptionsDisposable.Dispose();
            }
            base.Disable();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Enable()
        {
            undoCommand = new VisualDelegateCommand(Undo, "Undo",
                ImageFactory.GetImageThisAssembly("Images/arrow-undo-16.png"),
                ImageFactory.GetImageThisAssembly("Images/arrow-undo-32.png"),
                false);

            redoCommand = new VisualDelegateCommand(Redo, "Redo",
                ImageFactory.GetImageThisAssembly("Images/arrow-redo-16.png"),
                ImageFactory.GetImageThisAssembly("Images/arrow-redo-32.png"),
                false);

            var group = IrAuthorShell.HomeTab.GetGroup("Clipboard");
            group.AddButton(undoCommand);
            group.AddButton(redoCommand);

            var deleteStream = Observable.FromEventPattern<CancelEventArgs<RuleRepositoryDefBase>>(
                x => RuleApplicationService.Controller.RemovingDef += x,
                x => RuleApplicationService.Controller.RemovingDef -= x)
                .Select(x => x.EventArgs.Item)
                .Select(x => PopulateUndoHistoryItem(x, UndoDefRemoved));

            var insertStream = Observable.FromEventPattern<EventArgs<RuleRepositoryDefBase>>(
                x => RuleApplicationService.Controller.DefAdded += x,
                x => RuleApplicationService.Controller.DefAdded -= x)
                .Select(x => x.EventArgs.Item)
                .Select(x => PopulateUndoHistoryItem(x, UndoDefInserted));


            // add operations to the undo stack, as long as there isn't an undo currently in progress
            var operationStream = deleteStream.Merge(insertStream);
            var undoStream = operationStream.Where(x => !undoInProgress.Value).Do(item => LogEvent("UndoStream: {0}", item.DefToUndo.Name));
            var redoStream = operationStream.Where(x => undoInProgress.Value).Do(item => LogEvent("RedoStream: {0}", item.DefToUndo.Name));

            subscriptionsDisposable.Add(undoStream.Subscribe(x => undoBuffer.Push(x)));
            subscriptionsDisposable.Add(redoStream.Subscribe(x => redoBuffer.Push(x)));

            subscriptionsDisposable.Add(redoBuffer.ItemCount.Subscribe(x => redoCommand.IsEnabled = x > 0));

            var undoActionStream = UndoClicked.Where(x => undoBuffer.Any());
            subscriptionsDisposable.Add(undoActionStream.Subscribe(x =>
            {
                var item = undoBuffer.Pop();
                PerformUndo(item);
            }, exception => LogEvent(exception.ToString())));

            var redoActionStream = redoSubject.Where(x => redoBuffer.Any());
            subscriptionsDisposable.Add(redoActionStream.Subscribe(x =>
            {
                var item = redoBuffer.Pop();
                item.UndoAction(item);
            }));
            subscriptionsDisposable.Add(undoBuffer.ItemCount.Subscribe(x => undoCommand.IsEnabled = x > 0));
            subscriptionsDisposable.Add(undoInProgress.Subscribe(x => LogEvent("UndoInProgress: {0}", x)));
        }

        private void Redo(object obj)
        {
            redoSubject.OnNext(Unit.Default);
        }

        private void PerformUndo(UndoHistoryItem item)
        {
            undoInProgress.OnNext(true);
            var undoAction = item.UndoAction;
            undoAction(item);
            undoInProgress.OnNext(false);
        }

        private void UndoDefRemoved(UndoHistoryItem item)
        {
            var def = item.DefToUndo;

            var ruleApp = RuleApplicationService.RuleApplicationDef;
            if (ruleApp.LookupItem(def.Guid) != null)
            {
                LogEvent("{0} already exists in rule application def. Cannot undo a deletion when the target element is already present", def.Name);
                return;
            }

            var parent = ruleApp.LookupItem(item.ParentGuid) ?? RuleApplicationService.RuleApplicationDef;

            RuleApplicationService.Controller.InsertDef(def, parent, item.OriginalIndex);
            SelectionManager.SelectedItem = def;

            LogEvent("Added Def {0} to parent {1} at position {2}", def.Name, parent.Name, item.OriginalIndex);
        }

        private UndoHistoryItem PopulateUndoHistoryItem(RuleRepositoryDefBase x, Action<UndoHistoryItem> undoAction)
        {
            var idx = -1;
            if (x.ParentCollection != null)
            {
                idx = x.ParentCollection.IndexOf(x.Guid);
            }

            return new UndoHistoryItem
            {
                DefToUndo = x.CopyWithSameGuids(),
                ParentGuid = x.Parent.Guid,
                OriginalIndex = idx,
                UndoAction = undoAction
            };
        }

        private void UndoDefInserted(UndoHistoryItem item)
        {
            var def = item.DefToUndo;
            var ruleApp = RuleApplicationService.RuleApplicationDef;
            var lookedUpDef = ruleApp.LookupItem(def.Guid);
            if (lookedUpDef == null)
            {
                LogEvent("{0} does not exist in this RuleAppDef. Nothing to undo.", def.Name);
                return;
            }
            SelectionManager.SelectedItem = lookedUpDef.Parent;
            RuleApplicationService.Controller.RemoveDef(lookedUpDef);
            LogEvent("(UndoDefInserted) - Removed Def {0}", def.Name);
        }
        
        private static void LogEvent(string message, params object[] values)
        {
            var formattedMessage = string.Format(message, values);
            Debug.WriteLine("UndoExtension - {0}", new object[] { formattedMessage });
        }

        private void Undo(object obj)
        {
            UndoButtonSubject.OnNext(Unit.Default);
        }
    }
}
