using System.Diagnostics;
using NuGet;

namespace CatalogSearch.ViewModels
{
    public class DebugLogger : ILogger
    {
        public void Log(MessageLevel level, string message, params object[] parameters)
        {
            Debug.WriteLine($"{level}: {string.Format(message, parameters)}");
        }

        public FileConflictResolution ResolveFileConflict(string conflict)
        {
            Log(MessageLevel.Error, conflict);
            return FileConflictResolution.Ignore;
        }
    }
}