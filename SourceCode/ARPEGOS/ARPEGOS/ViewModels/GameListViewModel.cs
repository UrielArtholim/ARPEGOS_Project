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

        public IList<GameFolder> GameList { get; private set; }
        public GameListViewModel()
        {
            GameList = new ObservableCollection<GameFolder>();
            var exampleGamePath = Path.Combine(gamesRootDirectoryPath, exampleGame);
            exampleGamePath = directoryHelper.CreateDirectory(exampleGamePath);
            GetGameList();
        }
        public void GetGameList()
        {
            DirectoryInfo gamesRootDirectoryInfo = new DirectoryInfo(gamesRootDirectoryPath);
            foreach (DirectoryInfo gameDirectory in gamesRootDirectoryInfo.GetDirectories())
            {
                GameList.Add(new GameFolder(gameDirectory.Name));
            }

        }
    }
}
