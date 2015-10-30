using System.ComponentModel;
using NuGet;

namespace ExtensionManager.ViewModels
{
    public class ExtensionRowViewModel : INotifyPropertyChanged
    {
        public IPackage Package { get; set; }

        public IPackageMetadata PackageMetadata { get { return Package as IPackageMetadata; } }

        public bool IsInstalled
        {
            get { return isInstalled; }
            set
            {
                var shouldRaise = isInstalled == value;
                isInstalled = value;
                if (PropertyChanged != null && shouldRaise)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsInstalled"));
                }
            }
        }
        private bool isInstalled;
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
        private bool isEnabled;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}