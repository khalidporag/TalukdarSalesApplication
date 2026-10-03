using System.Runtime.CompilerServices;

namespace TalukdarSales.Tests
{
    internal static class TestEnvironment
    {
        // Every test host starts file watchers; many hosts in one run exhaust the inotify limit on small machines.
        [ModuleInitializer]
        internal static void UsePollingWatchers() => Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");
    }
}
