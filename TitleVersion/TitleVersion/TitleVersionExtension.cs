using System;
using System.Windows;
using InRule.Authoring.Windows;
using InRule.Repository;

namespace CatalogSearch
{
    public sealed class TitleVersionExtension : ExtensionBase
    {
        public TitleVersionExtension() 
            : base(name: "Title Version for IrAuthor", 
                  description: "Add the current version of irAuthor to the application title bar.", 
                  guid: new Guid("{16550F9C-3096-4BA0-9B30-6391E49C810E}"))
        {
        }

        public override void Enable()
        {
            RuleApplicationService.RuleApplicationDefChanged += RuleApplicationService_RuleApplicationDefChanged;
            SetApplicationTitle();
        }

        private void RuleApplicationService_RuleApplicationDefChanged(object sender, EventArgs e)
        {
            SetApplicationTitle();
        }

        private void SetApplicationTitle()
        {
            try
            {
                var mainWindow = Application.Current.MainWindow as IIrAuthorShell;
                var version = mainWindow.InRuleVersion;
                
                //Window Title After the Dash
                mainWindow.ApplicationName = $"InRule irAuthor v{version.Major}.{version.Minor}.{version.MajorRevision}.{version.MinorRevision}";

                //Taskbar Item Name
                var selectedDef = SelectionManager.SelectedItem as RuleRepositoryDefBase;
                if (selectedDef != null)
                    Application.Current.MainWindow.Title = $"{((RuleRepositoryDefBase)SelectionManager.SelectedItem).ThisRuleApp.Name} - InRule irAuthor v{version.Major}.{version.Minor}.{version.MajorRevision}.{version.MinorRevision}";
                else
                    Application.Current.MainWindow.Title = $"InRule irAuthor v{version.Major}.{version.Minor}.{version.MajorRevision}.{version.MinorRevision}";
            }
            catch(Exception)
            { }
        }
    }
}