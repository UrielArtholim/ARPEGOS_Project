using ARPEGOS.Models;
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
        GameListViewModel gameListViewModel;
        public GameListPage()
        {
            InitializeComponent();
            gameListViewModel = new GameListViewModel();
            BindingContext = gameListViewModel;
        }
        void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            SimpleListItem tappedItem = e.Item as SimpleListItem;
            //DisplayAlert("OnTapped",tappedItem.DisplayName, "OK");

            ((ListView)sender).SelectedItem = null;

        }

    }
}