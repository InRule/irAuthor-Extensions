using System;
using InRule.Repository;

namespace UndoExtension
{
    public class UndoHistoryItem
    {
        public Guid ParentGuid { get; set; }
        public int OriginalIndex { get; set; }
        public RuleRepositoryDefBase DefToUndo { get; set; }
        public Action<UndoHistoryItem> UndoAction { get; set; }
    }
}