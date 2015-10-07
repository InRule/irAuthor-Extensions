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

        protected readonly ISubject<UndoHistoryItem> UndoStack = new ReplaySubject<UndoHistoryItem>(BUFFER_SIZE);
        private readonly CompositeDisposable subscriptionsDisposable = new CompositeDisposable();

        private Stack<UndoHistoryItem> bufferCollection = new Stack<UndoHistoryItem>();
         
        public UndoExtension()
            : base("UndoExtension", "Provides undo functionality on defs", new Guid("{CA41B187-3B1F-48A2-94A2-AAAAF047D453}"))
        {

        }

        public override void Enable()
        {
            undoCommand = new VisualDelegateCommand(Undo, "Undo",
                ImageFactory.GetImageThisAssembly("Images/arrows-undo-icon.png"),
                ImageFactory.GetImageThisAssembly("Images/arrows-undo-icon.png"));

            var group = IrAuthorShell.HomeTab.GetGroup("Clipboard");
            group.AddButton(undoCommand);
            var defChangedObservable = Observable.FromEventPattern<CancelEventArgs<RuleRepositoryDefBase>>(
                x => RuleApplicationService.Controller.RemovingDef += x,
                x => RuleApplicationService.Controller.RemovingDef -= x);

            DefActions = defChangedObservable.Select(x => x.EventArgs.Item);
            subscriptionsDisposable.Add(DefActions.Select(x => new UndoHistoryItem()
            {
                DefToUndo = x.CopyWithSameGuids(),
                ParentGuid = x.Parent.Guid,
                OriginalIndex = ((IContainsRuleElements)x.Parent).RuleElements.IndexOf(x)
            }).Subscribe(x =>
            {
                if (bufferCollection.Count >= BUFFER_SIZE)
                {
                    bufferCollection.Pop();
                }
                bufferCollection.Push(x);
            }));
            
            UndoStack.Do(
                x =>
                    Debug.WriteLine("UNDOSTREAM - Name {0} - Order {1} - Parent {2}", x.DefToUndo.Name, x.OriginalIndex,
                        x.ParentGuid));
            subscriptionsDisposable.Add(UndoClicked.Where(x => bufferCollection.Any()).Subscribe(x =>
            {
                UndoDefRemoved(bufferCollection.Pop());

            }));

        }

        private void AddItemToUndoHistory(RuleRepositoryDefBase def)
        {
            // old way
            //var parentId = def.Parent.Guid;
            //var elementIndex = ((IContainsRuleElements) def.Parent).RuleElements.IndexOf(def);
            //UndoStack.Push(new UndoHistoryItem { DefToUndo = def.CopyWithSameGuids(), ParentGuid = parentId, OriginalIndex = elementIndex});
            //Debug.WriteLine("added {0} to the undo stack", new object[] { def.Name });
            //undoCommand.IsEnabled = UndoStack.Count > 0;
        }

        private void UndoDefRemoved(UndoHistoryItem item)
        {
            var def = item.DefToUndo;

            Debug.WriteLine("Parent GUID: {0}", item.ParentGuid);
            Debug.WriteLine("undo delete of def: {0}", new object[] { def.Name });

            var ruleApp = RuleApplicationService.RuleApplicationDef;
            var parent = ruleApp.LookupItem(item.ParentGuid);

            if (parent == null) return;

            Debug.WriteLine("lookup of parent: {0}", new object[] { parent.Name });

            //parent.RuleElements.Insert(item.OriginalIndex, def);
            IrAuthorShell.ContentControl.InvalidateVisual();
            RuleApplicationService.Controller.InsertDef(def, parent, item.OriginalIndex);
            SelectionManager.SelectedItem = def;
            

            Debug.WriteLine("Added Def {0} to parent {1} elements collection", def.Name, parent.Name);

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
