namespace ARPEGOS
{
    using System;
    using System.IO;
    using System.Reflection;

    using ARPEGOS.Helpers;
    using ARPEGOS.Views;

    using Autofac;

    using Xamarin.Forms;

    public partial class App : Application
    {
        public static INavigation Navigation => (App.Current.MainPage as MainView)?.Detail?.Navigation;

        public static IContainer Container { get; set; }

        public App()
        {
            this.InitializeComponent();
            Container = new AutofacSetup().CreateContainer();
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainView)).Assembly;
            foreach (var res in assembly.GetManifestResourceNames()) {
                System.Diagnostics.Debug.WriteLine("found resource: " + res);
            }
            string path = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
            var files = Directory.GetFiles(path);
            this.MainPage = new MainView();
        }
    }
}
