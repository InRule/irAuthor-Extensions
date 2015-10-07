using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using InRule.Authoring.Commanding;
using InRule.Authoring.ComponentModel;
using InRule.Authoring.Media;
using InRule.Authoring.Services;
using InRule.Authoring.Windows;
using InRule.Authoring.Windows.Controls;
using InRule.Repository;
using InRule.Repository.RuleElements;

namespace UndoExtension
{
    public class UndoExtension : ExtensionBase
    {
        private VisualDelegateCommand undoCommand;
        private const int BUFFER_SIZE = 10;

        protected IObservable<RuleRepositoryDefBase> DefActions;
        private IObservable<Unit> UndoClicked => undoSubject.AsObservable();

        private readonly Subject<Unit> undoSubject = new Subject<Unit>();
        protected IObserver<Unit> UndoButtonSubject => undoSubject.AsObserver();

        private readonly CompositeDisposable subscriptionsDisposable = new CompositeDisposable();

        private readonly Stack<UndoHistoryItem> bufferCollection = new Stack<UndoHistoryItem>(BUFFER_SIZE);

        public UndoExtension()
            : base("UndoExtension", "Provides undo functionality on defs", new Guid("{CA41B187-3B1F-48A2-94A2-AAAAF047D453}"))
        {

        }

        public override void Enable()
        {
            undoCommand = new VisualDelegateCommand(Undo, "Undo",
                ImageFactory.GetImageThisAssembly("Images/arrow-undo-16.png"),
                ImageFactory.GetImageThisAssembly("Images/arrow-undo-32.png"));


            var group = IrAuthorShell.HomeTab.GetGroup("Clipboard");
            group.AddButton(undoCommand);

            var defChangedObservable = Observable.FromEventPattern<CancelEventArgs<RuleRepositoryDefBase>>(
                x => RuleApplicationService.Controller.RemovingDef += x,
                x => RuleApplicationService.Controller.RemovingDef -= x);

            DefActions = defChangedObservable.Select(x => x.EventArgs.Item);

            var undoStream = DefActions.Select(x =>
            new UndoHistoryItem()
            {
                DefToUndo = x.CopyWithSameGuids(),
                ParentGuid = x.Parent.Guid,
                OriginalIndex = ((IContainsRuleElements)x.Parent).RuleElements.IndexOf(x)
            });
            undoStream.Do(LogEvent);
            subscriptionsDisposable.Add(
                undoStream
                    .Subscribe(x =>
                    {
                        if (bufferCollection.Count >= BUFFER_SIZE)
                        {
                            var p = bufferCollection.Pop();
                            LogEvent("Buffer at max capacity. Popped {0} from stack", p.DefToUndo.Name);
                        }
                        bufferCollection.Push(x);
                        LogEvent("Added item to the undo buffer. Current count is {0}", bufferCollection.Count);
                    })
                );

            subscriptionsDisposable.Add(
                UndoClicked.Do(x => LogEvent("Undo clicked. Current buffer size: {0}", bufferCollection.Count))
                .Where(x => bufferCollection.Any())
                .Subscribe(x => UndoDefRemoved(bufferCollection.Pop()))
            );
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
            Debug.WriteLine("UNDOSTREAM - {0}", string.Format(message, values));
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

            LogEvent(item);

            RuleApplicationService.Controller.InsertDef(def, parent, item.OriginalIndex);
            SelectionManager.SelectedItem = def;

            LogEvent("Added Def {0} to parent {1} at position {2}", def.Name, parent.Name, item.OriginalIndex);

        }

        public void Undo(object obj)
        {
            UndoButtonSubject.OnNext(Unit.Default);
        }
    }

    public class UndoHistoryItem
    {
        public Guid ParentGuid { get; set; }
        public int OriginalIndex { get; set; }
        public RuleRepositoryDefBase DefToUndo { get; set; }
    }
}
