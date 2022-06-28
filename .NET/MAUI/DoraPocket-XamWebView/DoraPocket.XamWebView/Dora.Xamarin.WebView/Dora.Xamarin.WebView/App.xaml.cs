using Dora.Xamarin.WebView.Common.DependencyServices;
using Dora.Xamarin.WebView.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Dora.Xamarin.WebView
{
    public partial class App : Application
    {

        public App()
        {
            Initialize();

            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();

            UniversalSpecialFolderPathsShownList();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        /// <summary>
        /// 初始化操作，包括但不限于
        /// 1.服务注册 2.hybrid包解压
        /// </summary>
        private void Initialize()
        {
            var services = new ServiceCollection()
                .RegisterServices("Dora.Xamarin.WebView.Common")
                .RegisterServices("Dora.Xamarin.WebView.ViewModel");

            if (Device.RuntimePlatform == Device.Android)
                services.RegisterServices("Dora.Xamarin.WebView.Android");
            else if (Device.RuntimePlatform == Device.iOS)
                services.RegisterServices("Dora.Xamarin.WebView.iOS");

            var serviceProvider = services.BuildServiceProvider();

            ServiceProviderAccessor.Current = serviceProvider;
        }

        private void UniversalSpecialFolderPathsShownList()
        {
            Console.WriteLine($"---------------------{Device.RuntimePlatform} of FileSystem folder list---------------------");
            Console.WriteLine($"AppDataDirectory: {FileSystem.AppDataDirectory}");
            Console.WriteLine($"CacheDirectory: {FileSystem.CacheDirectory}");

            Console.WriteLine($"---------------------{Device.RuntimePlatform} of Environment SpecialFolder List---------------------");

            Console.WriteLine($"AdminTools: {Environment.SpecialFolder.AdminTools}");
            Console.WriteLine($"ApplicationData: {Environment.SpecialFolder.ApplicationData}");
            Console.WriteLine($"CDBurning: {Environment.SpecialFolder.CDBurning}");
            Console.WriteLine($"CommonAdminTools: {Environment.SpecialFolder.CommonAdminTools}");
            Console.WriteLine($"CommonApplicationData: {Environment.SpecialFolder.CommonApplicationData}");
            Console.WriteLine($"CommonDesktopDirectory: {Environment.SpecialFolder.CommonDesktopDirectory}");
            Console.WriteLine($"CommonDocuments: {Environment.SpecialFolder.CommonDocuments}");
            Console.WriteLine($"CommonMusic: {Environment.SpecialFolder.CommonMusic}");
            Console.WriteLine($"CommonOemLinks: {Environment.SpecialFolder.CommonOemLinks}");
            Console.WriteLine($"CommonPictures: {Environment.SpecialFolder.CommonPictures}");
            Console.WriteLine($"CommonProgramFiles: {Environment.SpecialFolder.CommonProgramFiles}");
            Console.WriteLine($"CommonProgramFilesX86: {Environment.SpecialFolder.CommonProgramFilesX86}");
            Console.WriteLine($"CommonPrograms: {Environment.SpecialFolder.CommonPrograms}");
            Console.WriteLine($"CommonStartMenu: {Environment.SpecialFolder.CommonStartMenu}");
            Console.WriteLine($"CommonStartup: {Environment.SpecialFolder.CommonStartup}");
            Console.WriteLine($"CommonTemplates: {Environment.SpecialFolder.CommonTemplates}");
            Console.WriteLine($"CommonVideos: {Environment.SpecialFolder.CommonVideos}");
            Console.WriteLine($"Cookies: {Environment.SpecialFolder.Cookies}");
            Console.WriteLine($"Desktop: {Environment.SpecialFolder.Desktop}");
            Console.WriteLine($"DesktopDirectory: {Environment.SpecialFolder.DesktopDirectory}");
            Console.WriteLine($"Favorites: {Environment.SpecialFolder.Favorites}");
            Console.WriteLine($"Fonts: {Environment.SpecialFolder.Fonts}");
            Console.WriteLine($"History: {Environment.SpecialFolder.History}");
            Console.WriteLine($"InternetCache: {Environment.SpecialFolder.InternetCache}");
            Console.WriteLine($"LocalApplicationData: {Environment.SpecialFolder.LocalApplicationData}");
            Console.WriteLine($"LocalizedResources: {Environment.SpecialFolder.LocalizedResources}");
            Console.WriteLine($"MyComputer: {Environment.SpecialFolder.MyComputer}");
            Console.WriteLine($"MyDocuments: {Environment.SpecialFolder.MyDocuments}");
            Console.WriteLine($"MyMusic: {Environment.SpecialFolder.MyMusic}");
            Console.WriteLine($"MyPictures: {Environment.SpecialFolder.MyPictures}");
            Console.WriteLine($"MyVideos: {Environment.SpecialFolder.MyVideos}");
            Console.WriteLine($"NetworkShortcuts: {Environment.SpecialFolder.NetworkShortcuts}");
            Console.WriteLine($"PrinterShortcuts: {Environment.SpecialFolder.PrinterShortcuts}");
            Console.WriteLine($"ProgramFiles: {Environment.SpecialFolder.ProgramFiles}");
            Console.WriteLine($"ProgramFilesX86: {Environment.SpecialFolder.ProgramFilesX86}");
            Console.WriteLine($"Programs: {Environment.SpecialFolder.Programs}");
            Console.WriteLine($"Recent: {Environment.SpecialFolder.Recent}");
            Console.WriteLine($"Resources: {Environment.SpecialFolder.Resources}");
            Console.WriteLine($"SendTo: {Environment.SpecialFolder.SendTo}");
            Console.WriteLine($"StartMenu: {Environment.SpecialFolder.StartMenu}");
            Console.WriteLine($"Startup: {Environment.SpecialFolder.Startup}");
            Console.WriteLine($"System: {Environment.SpecialFolder.System}");
            Console.WriteLine($"SystemX86: {Environment.SpecialFolder.SystemX86}");
            Console.WriteLine($"Templates: {Environment.SpecialFolder.Templates}");
            Console.WriteLine($"UserProfile: {Environment.SpecialFolder.UserProfile}");
            Console.WriteLine($"Windows: {Environment.SpecialFolder.Windows}");
        }
    }
}
