using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.Resources;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;

using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Authoring.Windows;
using InRule.Authoring.Threading;

using InRule.Repository;
using InRule.Repository.RuleElements;
using InRule.Repository.EndPoints;
using InRule.Repository.DecisionTables;
using InRule.Repository.ValueLists;
using InRule.Repository.Client;
using InRule.Repository.Service.Data;
using InRule.Repository.Expressions;
using InRule.Repository.Vocabulary;
using InRule.Repository.Templates;

using InRule.Runtime;
using InRule.RuleApplicationFramework;

namespace InRule.Authoring.Extensions.RuleAppFlowAnalyzer
{
    public class Extension : ExtensionBase
    {
        private VisualDelegateCommand _submitCommand;
        //private VisualDelegateCommand _reportCommand;

        public Extension()
            : base("RuleFlowVisualization", "Rule Flow Visualization", new Guid("{6C5DD414-4FAE-4012-95E5-9EA9DC7EEC93}"))
        {
            
        }

        public override void Enable()
        {
            var group = IrAuthorShell.HomeTab.GetGroup("Reports");
            //var group = IrAuthorShell.HomeTab.AddGroup("Flow Visualizer", ImageFactory.GetImageThisAssembly("/Images/flow32.png"));
            var button = group.GetControl("Execution Flow");
            if (button != null)
                group.RemoveItem(button);
            _submitCommand = new VisualDelegateCommand(Submit, "Execution Flow", "/Images/flow32.png", "/Images/flow32.png", false);
            group.AddButton(_submitCommand);
            _submitCommand.IsEnabled = RuleApplicationService.RuleApplicationDef != null;
            //_submitCommand.IsEnabled = false;

            RuleApplicationService.Opened += WhenRuleApplicationChanged;
            RuleApplicationService.Closed += WhenRuleApplicationChanged;
        }

        private void WhenRuleApplicationChanged(object sender, EventArgs e)
        {
            var enabled = RuleApplicationService.RuleApplicationDef != null;
            _submitCommand.IsEnabled = enabled;
        }

        private void Submit(object obj)
        {
            try
            {
                //var ruleApp = new FileSystemRuleApplicationReference(Properties.Settings.Default.RuleappFilePath);
                RuleApplicationDef ruleApp = RuleApplicationService.RuleApplicationDef;
                //using (var session = new RuleSession(ruleApp))
                //{
                    var entityName = string.Empty;
                    var rulesetName = string.Empty;

                    if (SelectionManager.SelectedItem as RuleSetDef != null)
                    {
                        RuleSetDef rulesetDef = (RuleSetDef)SelectionManager.SelectedItem;
                        //System.Windows.Forms.MessageBox.Show(rulesetDef.Name);

                        //var rootEntity = session.CreateEntity(rulesetDef.ThisEntity.Name);
                        string executionFlow = RuleEngineUtil.GetRuleExecutionFlowXml(rulesetDef);
                        
                        XmlDocument xmlExecutionFlow = new XmlDocument();
                        xmlExecutionFlow.LoadXml(executionFlow);

                        StringWriter sw = new StringWriter();
                        XmlToJson(xmlExecutionFlow, sw);

                        string curDir = System.IO.Path.GetDirectoryName(RuleApplicationService.EditorAssembly.Location);
                        //File.WriteAllText(curDir + @"\FlowVisualizer_files\ruleapp_data.js", "var ruleapp_data = " + sw.ToString());

                        //byte[] bytes = ReadBytesFromStream(InRule.Authoring.Extensions.RuleAppFlowAnalyzer.Properties.Resources.FlowVisualizer1);
                        //string s = GetResourceStream("FlowVisualizer1.htm");
                        //string s = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                        //Uri uri = new Uri(@"pack://application:,,,/StreamPage.htm");
                        //Stream source = Application.GetContentStream(uri).Stream;

                        string s = InRule.Authoring.Extensions.RuleAppFlowAnalyzer.Properties.Resources.FlowVisualizer1;
                        s = s.Replace("//\"ruleapp_data.js\"", "var ruleapp_data = " + sw.ToString()).Replace("###Root###", rulesetDef.Name);

                        var userControl2 = new UserControl2();
                        //userControl2.webReport.Navigate(new Uri(String.Format("file:///{0}/FlowVisualizer.htm", curDir)));
                        //userControl2.webReport.Navigate(new Uri(@"C:\Temp\InRuleRuleAppFlowVisualizer.htm"));
                        userControl2.webReport.Loaded += delegate
                        {
                            //userControl2.webReport.NavigateToStream(source);
                            userControl2.webReport.NavigateToString(s);
                        };

                        userControl2.SelectionChanged += delegate
                        {
                            //userControl2.webReport.NavigateToStream(source);
                            RuleSetDef selectedRuleset = null;
                            EntityDef entity = null;
                            if (userControl2.SelectedRuleset.Contains("_ForEach_"))
                            {
                                entity = ruleApp.Entities[userControl2.SelectedRuleset.Substring(userControl2.SelectedRuleset.IndexOf("_ForEach_") + 9)];
                                selectedRuleset = entity.GetRuleSet(userControl2.SelectedRuleset.Substring(0, userControl2.SelectedRuleset.IndexOf("_ForEach_")));
                            }
                            else
                            {
                                entity = rulesetDef.ThisEntity;
                                selectedRuleset = entity.GetRuleSet(userControl2.SelectedRuleset);
                                //System.Windows.Forms.MessageBox.Show(userControl2.SelectedRuleset);
                            }

                            if (selectedRuleset != null)
                                SelectionManager.SelectedItem = selectedRuleset;
                        };

                        //userControl2.webReport.Document.
                        //userControl2.webReport.NavigateToString(s);
                        //userControl2.webReport.Navigate(new Uri(@".\FlowVisualizer.htm"));

                        userControl2.Title = "Rule Execution Flow: " + rulesetDef.Name;
                        //SelectionManager.SelectedItem = ruleApp.GetRuleSet("GetFinalQuote");

                        userControl2.Show();

                        //var window = WindowFactory.CreateWindow("Execution Flow", userControl2, true);
                        //window.Show();



                        //mshtml.HTMLDocument doc;
                        //doc = (mshtml.HTMLDocument)userControl2.webReport.Document;

                        //mshtml.HTMLDocumentEvents2_Event iEvent;
                        
                        
                    }
                    else
                        System.Windows.Forms.MessageBox.Show("You must select a ruleset for this report!");
                //}
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private byte[] ReadBytesFromStream(string streamName)
        {
            using (System.IO.Stream stream = this.GetType().Assembly.GetManifestResourceStream(streamName))
            {
                byte[] result = new byte[stream.Length];
                stream.Read(result, 0, (int)stream.Length);
                return result;
            }
        }

        private string GetResourceStream(string resName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var strResources = assembly.GetName().Name + ".g.resources";
            var rStream = assembly.GetManifestResourceStream(strResources);
            var resourceReader = new ResourceReader(rStream);
            var items = resourceReader.OfType<DictionaryEntry>();
            var stream = items.First(x => (x.Key as string) == resName.ToLower()).Value;

            var file = (UnmanagedMemoryStream)stream;
            var reader = new StreamReader(file);

            return reader.ReadToEnd();
        }

        private void XmlToJson(XmlNode root, StringWriter sw)
        {
            //Hashtable obj = new Hashtable();
            var isRoot = root.ToString() == "System.Xml.XmlDocument";

            if (!isRoot)
                sw.Write("{\"name\": \"" + root.Name + "\"");

            if (root.ChildNodes.Count > 0)
            {
                if (!isRoot)
                    sw.Write(",\"children\": [");
                for (int i = root.ChildNodes.Count - 1, n = 0; i >= n; i--)
                {
                    //object result = null;
                    XmlNode current = root.ChildNodes.Item(i);

                    XmlToJson(current, sw);
                    if (i != 0)
                        sw.Write(",");
                }
                if (!isRoot)
                    sw.Write("]");
            }
            if (!isRoot)
                sw.Write("}");
        }
    }
}
