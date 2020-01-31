using ARPEGOS.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ARPEGOS.Views
{
    class MainPage : MasterDetailPage
    {
        public MainPage()
        {
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            this.Master = new MainMenuPage();
            this.Detail = new NavigationPage(new WelcomePage());
            this.MasterBehavior = MasterBehavior.Popover;
        }
    }
}
