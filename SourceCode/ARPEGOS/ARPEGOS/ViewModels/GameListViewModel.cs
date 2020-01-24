using ARPEGOS.Interfaces;
using ARPEGOS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    class GameListViewModel
    {
        static readonly string exampleGame = "Anima Beyond Fantasy";
        static readonly string gamesRootDirectoryName = "Games";
        static readonly IDirectory directoryHelper = DependencyService.Get<IDirectory>();
        static readonly string gamesRootDirectoryPath = directoryHelper.CreateDirectory(gamesRootDirectoryName);

        public ObservableCollection<SimpleListItem> GameList { get; private set; }
        public GameListViewModel()
        {
            GameList = new ObservableCollection<SimpleListItem>();
            

        }
        public ObservableCollection<SimpleListItem> GetGameList()
        {
            DirectoryInfo gamesRootDirectoryInfo = new DirectoryInfo(gamesRootDirectoryPath);
            foreach (DirectoryInfo gameDirectory in gamesRootDirectoryInfo.GetDirectories())
            {
                GameList.Add(new SimpleListItem(gameDirectory.Name));
            }
            return GameList;
        }

        void CheckGames()
    }
}
