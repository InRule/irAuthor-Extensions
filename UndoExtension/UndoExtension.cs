using System;
using System.Collections.Generic;
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
using InRule.Repository.RuleElements;

namespace UndoExtension
{
    public class UndoExtension : ExtensionBase
    {
        private IObserver<Unit> UndoButtonSubject => undoSubject.AsObserver();

        private VisualDelegateCommand undoCommand;

        private const int BUFFER_SIZE = 3;

        private IObservable<Unit> UndoClicked => undoSubject.AsObservable();

        private readonly Subject<Unit> undoSubject = new Subject<Unit>();
        private readonly Subject<int> undoStackChanged = new Subject<int>();
        private readonly BehaviorSubject<bool> undoInProgress = new BehaviorSubject<bool>(false);

        private readonly CompositeDisposable subscriptionsDisposable = new CompositeDisposable();

        private readonly Stack<UndoHistoryItem> bufferCollection = new Stack<UndoHistoryItem>(BUFFER_SIZE);

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

        public override void Enable()
        {
            undoCommand = new VisualDelegateCommand(Undo, "Undo",
                ImageFactory.GetImageThisAssembly("Images/arrow-undo-16.png"),
                ImageFactory.GetImageThisAssembly("Images/arrow-undo-32.png"),
                false);

            var group = IrAuthorShell.HomeTab.GetGroup("Clipboard");
            group.AddButton(undoCommand);

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
            var operationStream = deleteStream.Merge(insertStream).Where(x => !undoInProgress.Value).Do(LogEvent);
            subscriptionsDisposable.Add(operationStream.Subscribe(x => DonutPopStack(x)));

            var undoActionStream = UndoClicked.Where(x => bufferCollection.Any());
            subscriptionsDisposable.Add(undoActionStream.Subscribe(x =>
            {
                var item = bufferCollection.Pop();
                undoStackChanged.OnNext(bufferCollection.Count);
                PerformUndo(item);
            }, exception => LogEvent(exception.ToString())));
            subscriptionsDisposable.Add(undoStackChanged.Subscribe(x => undoCommand.IsEnabled = x > 0));

            subscriptionsDisposable.Add(undoInProgress.Subscribe(x => LogEvent("UndoInProgress: {0}", x)));
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
            return new UndoHistoryItem
            {
                DefToUndo = x.CopyWithSameGuids(),  
                ParentGuid = x.Parent.Guid,
                OriginalIndex = ((IContainsRuleElements)x.Parent).RuleElements.IndexOf(x),
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

        private UndoHistoryItem DonutPopStack(UndoHistoryItem itemToAdd, int bufferSize = BUFFER_SIZE)
        {
            UndoHistoryItem item = null;
            if (bufferCollection.Count >= bufferSize)
            {
                item = bufferCollection.Pop();
            }
            bufferCollection.Push(itemToAdd);
            undoStackChanged.OnNext(bufferCollection.Count);
            return item;
        }

        private void LogEvent(UndoHistoryItem undoItem)
        {
            if (undoItem != null)
            {
                LogEvent("Buffer Count: {3} Name: {0} - DefIndex: {1} Parent: {2}", undoItem.DefToUndo.Name, undoItem.OriginalIndex, undoItem.ParentGuid, bufferCollection.Count);
            }
        }

        private void LogEvent(string message, params object[] values)
        {
            var formattedMessage = string.Format(message, values);
            Debug.WriteLine("UNDOSTREAM - {0}", new object[] { formattedMessage });
        }

        public void Undo(object obj)
        {
            UndoButtonSubject.OnNext(Unit.Default);
        }
    }

    public class UndoHistoryItem
    {
        public enum OperationType
        {
            DefRemoved = 1,
            DefInserted = 2
        }
        public Guid ParentGuid { get; set; }
        public int OriginalIndex { get; set; }
        public RuleRepositoryDefBase DefToUndo { get; set; }

        public OperationType Operation { get; set; }

        public Action<UndoHistoryItem> UndoAction { get; set; }
    }
}
