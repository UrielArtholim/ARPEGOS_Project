namespace ARPEGOS
{
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
            this.MainPage = new NavigationPage(new CreationView());
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
    }
}
