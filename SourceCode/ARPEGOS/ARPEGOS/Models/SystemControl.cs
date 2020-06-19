namespace ARPEGOS.Models
{
    using ARPEGOS.Interfaces;
    using RDFSharp.Model;
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
        #region Properties
        public static readonly IDirectory DirectoryHelper = DependencyService.Get<IDirectory>();
        static readonly Dictionary<Guid, bool> ActiveGames = new Dictionary<Guid, bool>();
        static readonly Dictionary<Guid, string> Games = new Dictionary<Guid, string>();
        static readonly string GamesRootDirectoryPath = DirectoryHelper.GetBaseDirectory();
        static readonly ObservableCollection<SimpleListItem> GamesList = new ObservableCollection<SimpleListItem>();
        static string ActiveGameVersion { get; set; }
        static string ActiveCharacter { get; set; }
        public static Game ActiveGame { get; set; }
        #endregion

        #region Methods
        public static void UpdateGames()
        {
            if (!Directory.Exists(GamesRootDirectoryPath))
                DirectoryHelper.CreateDirectory(GamesRootDirectoryPath);

            DirectoryInfo gamesRootDirectoryInfo = new DirectoryInfo(GamesRootDirectoryPath);
            var subdirectories = gamesRootDirectoryInfo.GetDirectories();
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
            var ResourceNames = assembly.GetManifestResourceNames();
            string[] gamefiles = { };
            gamefiles = ResourceNames.Where(x => x.EndsWith(".owl")).ToArray();
            foreach (var game in gamefiles)
            {
                var filefullPath = game.Split('.');
                var folderName = filefullPath[2].Replace('_', ' ');
                var fileName = filefullPath[4];

                if (!Directory.Exists(Path.Combine(GamesRootDirectoryPath, folderName)))
                {
                    DirectoryHelper.CreateDirectory(Path.Combine(GamesRootDirectoryPath, folderName));
                    DirectoryHelper.CreateDirectory(Path.Combine(GamesRootDirectoryPath, folderName, "characters"));
                    DirectoryHelper.CreateDirectory(Path.Combine(GamesRootDirectoryPath, folderName, "gamefiles"));
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

                if (!System.IO.File.Exists(Path.Combine(GamesRootDirectoryPath, folderName, fileName)))
                    WriteResourceToFile(game, Path.Combine(GamesRootDirectoryPath, folderName, "gamefiles", fileName));
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

        public static ObservableCollection<SimpleListItem> GetGameList()
        {
            GamesList.Clear();
            List<string> GamesValues = Games.Values.ToList();
            foreach (var game in GamesValues)
            {
                SimpleListItem gameItem = GamesList.FirstOrDefault(item => item.ItemName == game);
                if (gameItem == null)
                    GamesList.Add(new SimpleListItem(game));
            }
            return GamesList;
        }

        public static string GetActiveGame()
        {
            var activeGameID = ActiveGames.FirstOrDefault(key => key.Value == true).Key;
            return Games.FirstOrDefault(name => name.Key == activeGameID).Value;
        }

        public static string GetActiveVersion()
        {
            return ActiveGameVersion;
        }

        public static string GetActiveCharacter()
        {
            return ActiveCharacter;
        }

        public static void UpdateActiveCharacter(string selectedCharacter)
        {
            ActiveCharacter = selectedCharacter;
        }

        public static void UpdateActiveVersion(string selectedVersion)
        {
            ActiveGameVersion = selectedVersion;
        }

        public static void UpdateActiveGame(string selectedGame)
        {
            var activeGameID = ActiveGames.FirstOrDefault(game => game.Value == true).Key;
            ActiveGames[activeGameID] = false;
            activeGameID = Games.FirstOrDefault(game => game.Value == selectedGame).Key;
            ActiveGames[activeGameID] = true;
        }
        #endregion

    }
}
