//using PuppeteerSharp;
//namespace TerminalTestIntegration.Tests;

//public static class PuppeteerSetup
//{
//    private static readonly object LockObj = new();
//    private static bool _downloaded = false;

//    public static async Task EnsureBrowserDownloadedAsync()
//    {
//        lock (LockObj)
//        {
//            if (_downloaded)
//                return;
//            _downloaded = true;
//        }

//        await new BrowserFetcher().DownloadAsync();
//    }
//}
