using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ExtensionManager.Commands;
using ExtensionManager.Views;
using InRule.Authoring.Services;
using InRule.Authoring.Windows;

namespace ExtensionManager.ViewModels
{
    using NuGet;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;

    public class ExtensionBrowserViewModel 
    {
        public event EventHandler<ExtensionManagerSettings> SettingsChanged;

        public ExtensionBrowser ExtensionBrowserView { private get; set; }
        public ObservableCollection<ExtensionRowViewModel> Extensions { get; }
        public ICommand AddExtensionCommand { get; }
        public ICommand RemoveExtensionCommand { get; }
        public IIrAuthorShell IrAuthorShell { get; set; }
        public RuleApplicationService RuleApplicationService { get; set; }
        public ICommand UpdateExtensionCommand { get; }
        public IEnumerable<IExtension> InstalledExtensions { get; set; } 
        public readonly PackageManager PackageManager;

        private readonly string ExtensionsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"InRule\irAuthor\ExtensionExchange");
        private readonly AggregateRepository repository;

        internal readonly ExtensionManagerSettings Settings;

        public ExtensionBrowserViewModel(ExtensionManagerSettings settings)
        {
            Settings = settings;

            Extensions = new ObservableCollection<ExtensionRowViewModel>();
            InstalledExtensions = new List<IExtension>();
            
            repository = new AggregateRepository(PackageRepositoryFactory.Default, new[] {
                Settings.FeedUrl,
                "https://www.nuget.org/api/v2/curated-feeds/microsoftdotnet/",
                "https://api.nuget.org/v3/index.json",
                }, true)
            {
                ResolveDependenciesVertically = true,
                IgnoreFailingRepositories = true,
                
            };
            PackageManager = new PackageManager(repository, ExtensionsDirectory)
            {
                Logger = new DebugLogger()
            };
            PackageManager.FileSystem.Logger = PackageManager.Logger;
            repository.Logger = PackageManager.Logger;

            AddExtensionCommand = new AddExtensionCommand(this);
            RemoveExtensionCommand = new RemoveExtensionCommand(this);
            UpdateExtensionCommand = new UpdateExtensionCommand(this);

            RefreshPackageList();
        }

        internal void RestartApplicationWithConfirm()
        {
            var restart = MessageBoxFactory.ShowYesNo("Requested operation has been completed. irAuthor must be restarted before your changes can take effect. Would you like to restart irAuthor now?", "Restart needed", MessageBoxFactoryImage.Question, ExtensionBrowserView);

            if (!restart) return;

            // If a ruleapp is loaded, give them the opportunity to save it or cancel.
            if (RuleApplicationService.RuleApplicationDef != null)
            {
                restart = RuleApplicationService.Close();
            }

            if (!restart) return;

            Process.Start(Application.ResourceAssembly.Location);

            IrAuthorShell.Exit();
        }

        public void RefreshPackageList(bool showInstalledOnly = false)
        {
            var window = new BackgroundWorkerWaitWindow("Loading Extensions", $"Loading extensions from the InRule Extension Exchange service...");
            window.DoWork += delegate(object sender, DoWorkEventArgs args) 
            {
                Debug.WriteLine("In refresh packages Task");

                var packages = repository.Repositories.First(x => x.Source == Settings.FeedUrl).GetPackages()
                    .Where(x => x.Tags.Contains("extension"))
                    .ToList()
                    .GroupBy(x => x.Id, (id, packs) =>
                    {
                        SemanticVersion latestVersion;
                        latestVersion = packs.Max(v => v.Version);
                        IPackage currentPackage = PackageManager.LocalRepository.FindPackage(id);
                        return new ExtensionRowViewModel
                        {
                            UpdateAvailable = currentPackage != null && !currentPackage.IsLatestVersion,
                            IsInstalled = currentPackage != null,
                            LatestVersion = latestVersion.ToNormalizedString(),
                            InstalledVersion = currentPackage == null ? "--" : currentPackage.Version.ToNormalizedString(),
                            Package = packs.First(p => p.Id == id)
                        };
                    })
                    .ToList()
                    .Where(x => !showInstalledOnly || x.IsInstalled);

                args.Result = packages;
            };
            window.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs args)
            {
                Debug.WriteLine("Got packages.");
                if (args.Error == null)
                {
                    Extensions.Clear();
                    Extensions.AddRange((IEnumerable<ExtensionRowViewModel>)args.Result);
                }
                else
                {
                    Debug.WriteLine(args.Error.ToString());
                    MessageBox.Show(args.Error.ToString());
                    throw args.Error;
                }
            };
            window.ShowDialog();
        }

        internal void InvokeSettingsChanged()
        {
            SettingsChanged?.Invoke(this, Settings);
        }
    }
}