using System;
using System.Collections.Generic;

namespace DecisionTableImporter
{
    class Row
    {
        public Row()
        {
            Conditions = new List<Condition>();
            Actions = new List<Action>();
        }

        public List<Condition> Conditions { get; internal set; }
        public List<Action> Actions { get; internal set; }
        public int ColumnNumber { get; set; }
    }

    class Condition
    {
        public string Value { get; set; }
        public int ColumnNumber { get; set; }
    }

    class Action
    {
        public string Value { get; set; }
        public int ColumnNumber { get; set; }
    }
}
