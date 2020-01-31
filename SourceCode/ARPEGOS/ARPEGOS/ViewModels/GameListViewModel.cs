using ARPEGOS.Interfaces;
using ARPEGOS.Models;
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
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    class GameListViewModel : DirectoryViewModel
    {
        static readonly string rootDirectoryName = "Games";
        readonly string gamesRootDirectoryPath;
        public IList<ListItem> GameList { get; private set; }
        public GameListViewModel()
        {
            GameList = new ObservableCollection<ListItem>();
            gamesRootDirectoryPath = Path.Combine(directoryHelper.GetBaseDirectory(), rootDirectoryName);

            if (!Directory.Exists(gamesRootDirectoryPath))
                directoryHelper.CreateDirectory(gamesRootDirectoryPath);

            CheckGames();
            GetList();
        }
        void CheckGames()
        {
            DirectoryInfo gamesRootDirectoryInfo = new DirectoryInfo(gamesRootDirectoryPath);
            var subdirectories = gamesRootDirectoryInfo.GetDirectories();
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
            var ResourceNames = assembly.GetManifestResourceNames();
            string[] gamefiles = { };
            gamefiles = ResourceNames.Where(x => x.EndsWith(".owl")).ToArray();
            foreach (var game in gamefiles)
            {
                var filefullPath = game.Split('.');
                var folderName = filefullPath[2].Replace('_',' ');
                var fileName = filefullPath[3] + "." + filefullPath[4];

                if (!Directory.Exists(Path.Combine(gamesRootDirectoryPath, folderName)))
                    directoryHelper.CreateDirectory(Path.Combine(gamesRootDirectoryPath, folderName));

                if (!File.Exists(Path.Combine(gamesRootDirectoryPath, folderName, fileName)))
                    WriteResourceToFile(game, Path.Combine(gamesRootDirectoryPath, folderName, fileName));
            }
        }

        void WriteResourceToFile(string resourcePath, string filePath)
        {
            using(var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
            {
                using(var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }
        public override void GetList()
        {
            DirectoryInfo gamesRootDirectoryInfo = new DirectoryInfo(gamesRootDirectoryPath);
            foreach (DirectoryInfo gameDirectory in gamesRootDirectoryInfo.GetDirectories())
            {
                GameList.Add(new ListItem(gameDirectory.Name));
            }
        }
    }
}
