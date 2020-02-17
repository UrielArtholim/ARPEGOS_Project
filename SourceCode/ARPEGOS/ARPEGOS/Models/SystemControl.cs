namespace ARPEGOS.Models
{
    using ARPEGOS.Interfaces;
    using ARPEGOS.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Xamarin.Forms;

    public static class SystemControl
    {
        public static readonly IDirectory directoryHelper = DependencyService.Get<IDirectory>();
        static readonly Dictionary<Guid, bool> ActiveGames = new Dictionary<Guid, bool>();
        static readonly Dictionary<Guid, string> Games = new Dictionary<Guid, string>();
        static readonly string gamesRootDirectoryPath = directoryHelper.GetBaseDirectory();
        static readonly ObservableCollection<ListItem> GamesList = new ObservableCollection<ListItem>();
        static string GameVersion { get; set; }

        public static void UpdateGames()
        {
            if (!Directory.Exists(gamesRootDirectoryPath))
                directoryHelper.CreateDirectory(gamesRootDirectoryPath);

            DirectoryInfo gamesRootDirectoryInfo = new DirectoryInfo(gamesRootDirectoryPath);
            var subdirectories = gamesRootDirectoryInfo.GetDirectories();
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
            var ResourceNames = assembly.GetManifestResourceNames();
            string[] gamefiles = { };
            gamefiles = ResourceNames.Where(x => x.EndsWith(".owl")).ToArray();
            foreach (var game in gamefiles)
            {
                var filefullPath = game.Split('.');
                var folderName = filefullPath[2].Replace('_', ' ');
                var fileName = filefullPath[3] + "." + filefullPath[4];

                if (!Directory.Exists(Path.Combine(gamesRootDirectoryPath, folderName)))
                {
                    directoryHelper.CreateDirectory(Path.Combine(gamesRootDirectoryPath, folderName));
                    directoryHelper.CreateDirectory(Path.Combine(gamesRootDirectoryPath, folderName, "characters"));
                    directoryHelper.CreateDirectory(Path.Combine(gamesRootDirectoryPath, folderName, "gamefiles"));
                }

                Guid currentGameID = Guid.NewGuid();
                bool usedID = Games.Keys.Any(key => key.Equals(currentGameID));
                while(usedID)
                {
                    currentGameID = Guid.NewGuid();
                    usedID = Games.Keys.Any(key => key.Equals(currentGameID));
                }
                SystemControl.Games.Add(currentGameID, folderName);
                SystemControl.ActiveGames.Add(currentGameID, false);

                if (!System.IO.File.Exists(Path.Combine(gamesRootDirectoryPath, folderName, fileName)))
                    WriteResourceToFile(game, Path.Combine(gamesRootDirectoryPath, folderName, "GameFiles", fileName));
            }
        }

        private static void WriteResourceToFile(string resourcePath, string filePath)
        {
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
            {
                using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    resource.CopyTo(file);
                }
            }
        }

        public static string GetActiveGame()
        {
            var activeGameID = ActiveGames.FirstOrDefault(key => key.Value == true).Key;
            return Games.FirstOrDefault(name => name.Key == activeGameID).Value;
        }

        public static string GetActiveVersion()
        {
            return GameVersion;
        }

        public static void UpdateActiveVersion(string selectedVersion)
        {
            GameVersion = selectedVersion;
        }

        public static void UpdateActiveGame(string selectedGame)
        {
            var activeGameID = ActiveGames.FirstOrDefault(game => game.Value == true).Key;
            ActiveGames[activeGameID] = false;
            activeGameID = Games.FirstOrDefault(game => game.Value == selectedGame).Key;
            ActiveGames[activeGameID] = true;
        }

        public static ObservableCollection<ListItem> GetGameList()
        {
            GamesList.Clear();
            List<string> GamesValues = Games.Values.ToList();
            foreach (var game in GamesValues)
            {
                ListItem gameItem = GamesList.FirstOrDefault(item => item.ItemName == game);
                if (gameItem == null)
                    GamesList.Add(new ListItem(game));
            }
            return GamesList;
        }

        
    }
}
