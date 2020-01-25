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
    class GameListViewModel : ListViewModel
    {
        static readonly string rootDirectoryName = "Games";
        readonly string gamesRootDirectoryPath;
        public IList<SimpleListItem> GameList { get; private set; }
        public GameListViewModel()
        {
            GameList = new ObservableCollection<SimpleListItem>();
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
                Debug.WriteLine("");
                var folderName = filefullPath[2].Replace('_',' ');
                var fileName = filefullPath[3] + "." + filefullPath[4];
                var folderPath = Path.Combine(gamesRootDirectoryPath, folderName);
                var filePath = Path.Combine(gamesRootDirectoryPath, folderName, fileName);

                if (!Directory.Exists(folderPath))
                    directoryHelper.CreateDirectory(folderPath);

                if (!File.Exists(filePath))
                    WriteResourceToFile(game, filePath);
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
                GameList.Add(new SimpleListItem(gameDirectory.Name));
            }
        }
    }
}
