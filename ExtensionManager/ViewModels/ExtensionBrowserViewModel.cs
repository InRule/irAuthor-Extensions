using System.Collections.ObjectModel;
using System.Linq;

namespace ExtensionManager.ViewModels
{
    using NuGet;

    public class ExtensionBrowserViewModel
    {
        public ObservableCollection<IPackage> Extensions { get; private set; }
        
        private const string RoadGetFeedUrl = "http://roadget.azurewebsites.net/nuget"; // TODO: move this into a runtime-configurable setting

        public ExtensionBrowserViewModel()
        {
            var repo = PackageRepositoryFactory.Default.CreateRepository(RoadGetFeedUrl);
            var packages = repo.GetPackages().Where(x => x.Tags.Contains("extension")).ToList();
            Extensions = new ObservableCollection<IPackage>(packages);

        }
    }
}