using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using NuGet;

namespace ExtensionManager.ViewModels
{
    public class ExtensionRowViewModel : INotifyPropertyChanged
    {
        public Guid ExtensionId { get; set; }

        public IPackage Package { get; set; }

        public IPackageMetadata PackageMetadata => Package as IPackageMetadata;

        private bool updateAvailable;
        public bool UpdateAvailable
        {
            get { return updateAvailable; }
            set
            {
                if (updateAvailable == value) return;

                updateAvailable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UpdateAvailable"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsInstalled"));
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
                if (PropertyChanged != null && shouldRaise)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InstalledVersion"));
            }
        }

        public string LatestVersion { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}