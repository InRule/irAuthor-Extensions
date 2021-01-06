using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using InRule.Authoring.Extensions.DiagramEntitySchema;

namespace InRule.Authoring.Extensions.DiagramEntitySchema
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class ScriptingHelper
    {
        ReportWindow mExternalWPF;

        public ScriptingHelper(ReportWindow w)
        {
            this.mExternalWPF = w;
        }

        public void SelectElementInIrAuthor(string jsscript)
        {
        }

        public void ResizeControl(int width, int height)
        {
            this.mExternalWPF.Resize(height, width, true);
        }
    }

}
