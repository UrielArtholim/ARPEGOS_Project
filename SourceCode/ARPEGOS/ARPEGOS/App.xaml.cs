namespace ARPEGOS
{
    using ARPEGOS.ViewModels;
    using ARPEGOS.Views;
    using System;
    using System.Diagnostics;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
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
