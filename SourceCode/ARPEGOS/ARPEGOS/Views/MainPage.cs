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
            this.Master = new MainMenuPage();
            this.Detail = new NavigationPage(new WelcomePage());
        }
    }
}
