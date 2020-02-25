using ARPEGOS.Interfaces;
using ARPEGOS.Models;
using ARPEGOS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    class GameListViewModel
    {
        public ObservableCollection<ListItem> GameList { get; private set; }
        public GameListViewModel() 
        {
            SelectGameCommand = new Command<ListItem>(item =>
            {
                var selectedItem = this.GameList.FirstOrDefault(currentVar => currentVar.ItemName == item.ItemName);
                SystemControl.UpdateActiveGame(selectedItem.ItemName);
                MainPage main = Xamarin.Forms.Application.Current.MainPage as MainPage;
                NavigationPage navPage = main.Detail as NavigationPage; 
                navPage.PushAsync(new VersionListPage());
            });

            SystemControl.UpdateGames();
            GameList = SystemControl.GetGameList();
        }

        public ICommand SelectGameCommand { get; }
    }
}
