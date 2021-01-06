using System;
using System.Windows;

namespace InRule.Authoring.Extensions.DiagramEntitySchema
{
    public partial class ReportWindow : Window
    {
        public bool loaded;

        public ReportWindow()
        {
            InitializeComponent();
            ScriptingHelper helper = new ScriptingHelper(this);
        }
    }
}
