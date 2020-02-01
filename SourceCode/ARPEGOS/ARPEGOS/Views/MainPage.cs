using ARPEGOS.Models;
using ARPEGOS.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace ARPEGOS.Views
{
    class MainPage : MasterDetailPage
    {
        public MainPage()
        {
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            this.Master = new MasterMenuPage();
            this.Detail = new NavigationPage(new WelcomePage());
            this.MasterBehavior = MasterBehavior.Popover;

            MasterMenuViewModel masterViewModel = Master.BindingContext as MasterMenuViewModel;
            masterViewModel.PageSelected += MasterPageSelected;
            PresentDetailPage(PageType.Home);
        }

        void PresentDetailPage(PageType pagetype)
        {
            Page NextPage;
            switch (pagetype)
            {
                case PageType.GamesList: NextPage = new GameListPage(); break;
              //case PageType.CreateCharacter: NextPage = new WelcomePage(); break;
              //case PageType.ViewCharacter: NextPage = new WelcomePage(); break;
              //case PageType.EditCharacter: NextPage = new WelcomePage(); break;
              //case PageType.RemoveCharacter: NextPage = new WelcomePage(); break;
              //case PageType.OneSkillCalculator: NextPage = new WelcomePage(); break;
              //case PageType.TwoSkillCalculator: NextPage = new WelcomePage(); break;
              default: NextPage = new WelcomePage(); break;
            }
            Detail = new NavigationPage(NextPage);
            IsPresented = false;
        }
        void MasterPageSelected(object sender, PageType e)
        {
            PresentDetailPage(e);
        }
    }
}
