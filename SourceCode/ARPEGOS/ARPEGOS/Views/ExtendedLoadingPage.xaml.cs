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
            await Task.Delay(5000);
            await Navigation.PushAsync(new GameListPage()); // Go to the first app page
            return true;
        }
    }
}