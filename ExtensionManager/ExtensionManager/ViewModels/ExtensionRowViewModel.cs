using System;
using InRule.Authoring.ComponentModel;
using NuGet;

namespace ExtensionManager.ViewModels
{
    public class ExtensionRowViewModel : ObservableObject
    {
        public Guid ExtensionId { get; set; }
        public IPackage Package { get; set; }
        public IPackageMetadata PackageMetadata => Package;
        public string LatestVersion { get; set; }

        private bool updateAvailable;
        public bool UpdateAvailable
        {
            get { return updateAvailable; }
            set
            {
                if (updateAvailable == value) return;

                updateAvailable = value;

                OnPropertyChanged(nameof(UpdateAvailable));
            }
        }

        private bool isInstalled;
        public bool IsInstalled
        {
            get { return isInstalled; }
            set
            {
                if (isInstalled == value) return;

                isInstalled = value;

                OnPropertyChanged(nameof(IsInstalled));
            }
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                var shouldRaise = isEnabled == value;
                isEnabled = value;

                if (shouldRaise)
                {
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        private string installedVersion;
        public string InstalledVersion
        {
            get { return installedVersion; }
            set
            {
                if (value == installedVersion) return;

                installedVersion = value;

                OnPropertyChanged(nameof(InstalledVersion));
            }
        }
    }
}