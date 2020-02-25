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
        readonly MainPageViewModel viewModel;
        public MainPage()
        {
            Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);

            this.Master = new MasterMenuPage();
            this.Detail = new NavigationPage(new WelcomePage());
            this.MasterBehavior = MasterBehavior.Popover;
            viewModel = new MainPageViewModel();
            this.BindingContext = viewModel;

            //Add Master ViewModel to detect events
            MasterMenuViewModel masterViewModel = Master.BindingContext as MasterMenuViewModel;
            masterViewModel.PageSelected += MasterPageSelected;

            //Add viewModels to detect events
            this.Detail = viewModel.PresentDetailPage(PageType.Welcome);
        }

        void MasterPageSelected(object sender, PageType e)
        {
            this.Detail = viewModel.PresentDetailPage(e);
            this.IsPresented = false;
        }
    }
}
