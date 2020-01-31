using ARPEGOS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARPEGOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExtendedLoadingPage : ContentPage
    {
        public ExtendedLoadingPage()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await DelayedNavigation();
        }
        private async Task<Boolean> DelayedNavigation()
        {
            await Task.Delay(1000);
            await Navigation.PushAsync(new MainPage()); // Go to the first app page
            return true;
        }
    }
}