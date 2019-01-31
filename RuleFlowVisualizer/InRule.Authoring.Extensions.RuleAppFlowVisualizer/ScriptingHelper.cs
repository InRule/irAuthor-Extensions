using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace InRule.Authoring.Extensions.RuleAppFlowVisualizer
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class ScriptingHelper
    {
        UserControl2 mExternalWPF;

        public ScriptingHelper(UserControl2 w)
        {
            this.mExternalWPF = w;
        }

        public void SelectElementInIrAuthor(string jsscript)
        {
            //System.Windows.Forms.MessageBox.Show(string.Format("Message :{0}", jsscript));
            this.mExternalWPF.SelectedRuleset = string.Format("{0}", jsscript);
            //this.mExternalWPF..tbMessageFromBrowser.Text = string.Format("Message :{0}", jsscript);
        }

        public void ResizeControl(int width, int height)
        {
            this.mExternalWPF.Resize(height, width, true);
            //this.mExternalWPF.webReport.RenderSize.Width = width;
            //this.mExternalWPF.webReport.RenderSize = new System.Windows.Size(height, width + 150);
        }
    }

}
