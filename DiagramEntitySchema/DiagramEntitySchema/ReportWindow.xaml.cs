using InRule.Authoring.Services;
using System;
using System.Windows;

namespace InRule.Authoring.Extensions.DiagramEntitySchema
{
    public partial class ReportWindow : Window
    {
        public bool loaded;
        private RuleApplicationService _ruleApplicationService;
        private SelectionManager _selectionManager;

        public ReportWindow(RuleApplicationService ras, SelectionManager sm)
        {
            _ruleApplicationService = ras;
            _selectionManager = sm;
            InitializeComponent();

            //https://www.codeproject.com/articles/738504/winrt-how-to-communicate-with-webview-javascript-f
            webControl.ScriptNotify += WebControl_ScriptNotify;
        }

        private void WebControl_ScriptNotify(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlScriptNotifyEventArgs e)
        {
            if (e.Value.StartsWith("SelectEntity:"))
            {
                var entityName = e.Value.Replace("SelectEntity:", "");
                var ruleAppName = _ruleApplicationService.RuleApplicationDef.Name;
                var selectEntity = _ruleApplicationService.RuleApplicationDef.LookupItemByFullName(ruleAppName + "." + entityName);
                if (selectEntity != null)
                    _selectionManager.SelectedItem = selectEntity;
            }
        }
    }
}
