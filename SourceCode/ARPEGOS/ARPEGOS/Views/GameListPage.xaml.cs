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
    public partial class GameListPage : ContentPage
    {
        readonly GameListViewModel gameListViewModel;
        public GameListPage()
        {
            InitializeComponent();
            gameListViewModel = new GameListViewModel();
            BindingContext = gameListViewModel;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            var mainLoadingPage = Navigation.NavigationStack[0];
            Navigation.RemovePage(mainLoadingPage);
        }
    }
}