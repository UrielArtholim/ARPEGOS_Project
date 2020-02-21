using ARPEGOS.Models;
using ARPEGOS.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace ARPEGOS.Views
{
    class AppMDPage : MasterDetailPage
    {
        public AppMDPage()
        {
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            this.Master = new AppMasterMenuPage();
            this.Detail = new NavigationPage(new WelcomePage());
            this.MasterBehavior = MasterBehavior.Popover;

            AppMasterMenuViewModel masterViewModel = Master.BindingContext as AppMasterMenuViewModel;

            masterViewModel.PageSelected += MasterPageSelected;
            PresentDetailPage(PageType.Welcome);
        }

        void PresentDetailPage(PageType pagetype)
        {
            Page NextPage;
            switch (pagetype)
            {
                case PageType.Welcome: NextPage = new WelcomePage(); break;
                case PageType.Home: NextPage = new HomePage(); break;
                case PageType.GamesList: NextPage = new GameListPage(); break;
                case PageType.CreateCharacter: NextPage = new SelectCharacterClassPage(); break;
                case PageType.ViewCharacter: NextPage = new WelcomePage(); break;
                case PageType.EditCharacter: NextPage = new WelcomePage(); break;
                case PageType.RemoveCharacter: NextPage = new WelcomePage(); break;
                case PageType.OneSkillCalculator: NextPage = new WelcomePage(); break;
                case PageType.TwoSkillCalculator: NextPage = new WelcomePage(); break;
                default: NextPage = new HomePage(); break;
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
