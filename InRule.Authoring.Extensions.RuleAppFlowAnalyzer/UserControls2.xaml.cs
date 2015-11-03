using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace InRule.Authoring.Extensions.RuleAppFlowAnalyzer
{
    /// <summary>
    /// Interaction logic for UserControl
    /// </summary>
    public partial class UserControl2 : Window
    {
        private string _selectedRuleset = string.Empty;
        public event System.EventHandler SelectionChanged;
        public bool loaded;

        protected virtual void OnSelectionChanged()
        {
            if (SelectionChanged != null) SelectionChanged(this, EventArgs.Empty);
        }

        public UserControl2()
        {
            InitializeComponent();
            ScriptingHelper helper = new ScriptingHelper(this);
            this.webReport.ObjectForScripting = helper;
            ContentRendered += UserControl2_ContentRendered;
            this.webReport.LoadCompleted += WebBrowser_OnLoadCompleted;
        }

        private void UserControl2_ContentRendered(object sender, EventArgs e)
        {
            SizeChanged += UserControl2_RenderSizeChanged;
            loaded = true;
            //Zoom(150);
        }

        private void UserControl2_RenderSizeChanged(object sender, EventArgs e)
        {
            //Zoom((int)(100 * this.Width / (this.webReport.Document as dynamic).body.scrollWidth));
            //Zoom((int)((this.webReport.Document as dynamic).body.scrollWidth * 100 / ((System.Windows.SizeChangedEventArgs)(e)).NewSize.Width));
            Resize((int)((System.Windows.SizeChangedEventArgs)(e)).NewSize.Height - 20, (int)((System.Windows.SizeChangedEventArgs)(e)).NewSize.Width + 10, false);
        }

        private void Zoom(object zoomPercent)
        {
            // grab a handle to the underlying ActiveX object
            IServiceProvider serviceProvider = null;
            if (this.webReport.Document != null)
            {
                serviceProvider = (IServiceProvider)this.webReport.Document;
            }
            Guid serviceGuid = SID_SWebBrowserApp;
            Guid iid = typeof(SHDocVw.IWebBrowser2).GUID;
            SHDocVw.IWebBrowser2 browserInst = (SHDocVw.IWebBrowser2)serviceProvider.QueryService(ref serviceGuid, ref iid);

            while (browserInst.QueryStatusWB(SHDocVw.OLECMDID.OLECMDID_OPTICAL_ZOOM) != (SHDocVw.OLECMDF.OLECMDF_SUPPORTED & SHDocVw.OLECMDF.OLECMDF_ENABLED))
            { }

            // send the zoom command to the ActiveX object
            browserInst.ExecWB(SHDocVw.OLECMDID.OLECMDID_OPTICAL_ZOOM, SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER, ref zoomPercent, IntPtr.Zero);
        }

        private void WebBrowser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            Zoom(150);
            //SizeChanged += UserControl2_RenderSizeChanged;

            //MessageBox.Show("before " + this.Height.ToString());
            //this.Height = (int)(this.webReport.Document as dynamic).body.scrollHeight + 20;
            //this.Width = (int)(this.webReport.Document as dynamic).body.scrollWidth + 20;
            //MessageBox.Show("after " + this.Height.ToString());
            ////Resize((int)(this.Height / 1.5), (int)(this.Width / 1.5));
        }

        public string SelectedRuleset
        {
            get
            {
                return _selectedRuleset;
            }
            set
            {
                _selectedRuleset = value;
                OnSelectionChanged();
            }
        }

        public void Resize(int height, int width, bool fromJS)
        {
            //MessageBox.Show("resize from/to " + this.webReport.Height.ToString() + " " + height.ToString());
            if (fromJS && loaded)
                return;

            if (fromJS)
            {
                this.Height = Math.Max(height, 580);
                this.Width = Math.Max(width, 630);
            }

            this.webReport.Height = Math.Max(height, 600);
            this.webReport.Width = Math.Max(width + 150, 700);
            //this.webReport.RenderSize = new System.Windows.Size(height , width + 150);
        }

        // Needed to expose the WebBrowser's underlying ActiveX control for zoom functionality
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
        internal interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object QueryService(ref Guid guidService, ref Guid riid);
        }
        static readonly Guid SID_SWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
    }
}
