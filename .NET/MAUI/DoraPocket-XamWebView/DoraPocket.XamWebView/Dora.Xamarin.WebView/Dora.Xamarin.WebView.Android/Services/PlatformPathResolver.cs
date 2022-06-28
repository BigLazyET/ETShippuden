using Dora.Xamarin.WebView.Droid.Services;
using Dora.Xamarin.WebView.Services;
using System;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformPathResolver))]
namespace Dora.Xamarin.WebView.Droid.Services
{
    public class PlatformPathResolver : IPlatformPathResolver
    {
        public void PlatformSepcificPathsShownList()
        {
            Console.WriteLine($"---------------------Android of Android.App.Application.Context List---------------------");

            Console.WriteLine($"PackageCodePath: {Android.App.Application.Context.PackageCodePath}");
            Console.WriteLine($"PackageResourcePath: {Android.App.Application.Context.PackageResourcePath}");
            Console.WriteLine($"PackageCodePath: {Android.App.Application.Context.PackageCodePath}");

            Console.WriteLine($"---------------------Android of Android.OS.Environment List---------------------");

            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DirectoryAlarms}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DirectoryAudiobooks}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DirectoryDcim}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DirectoryDocuments}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DirectoryDownloads}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DirectoryMovies}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DirectoryMusic}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DirectoryNotifications}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DirectoryPictures}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DirectoryPodcasts}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DirectoryRingtones}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DirectoryScreenshots}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.DownloadCacheDirectory}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.ExternalStorageDirectory}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.RootDirectory}");
            Console.WriteLine($"DirectoryAlarms: {Android.OS.Environment.StorageDirectory}");
        }
    }
}