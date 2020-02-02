namespace ARPEGOS.Models
{
    using ARPEGOS.Interfaces;
    using ARPEGOS.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Xamarin.Forms;

    public static class SystemControl
    {
        public static readonly IDirectory directoryHelper = DependencyService.Get<IDirectory>();
        static readonly Dictionary<Guid, bool> ActiveGames = new Dictionary<Guid, bool>();
        static readonly Dictionary<Guid, string> Games = new Dictionary<Guid, string>();
        static readonly string rootDirectoryName = "Games";
        static readonly string gamesRootDirectoryPath = Path.Combine(directoryHelper.GetBaseDirectory(), rootDirectoryName);

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
                    directoryHelper.CreateDirectory(Path.Combine(gamesRootDirectoryPath, folderName, "Characters"));
                    directoryHelper.CreateDirectory(Path.Combine(gamesRootDirectoryPath, folderName, "GameFiles"));
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

        public static ObservableCollection<ListItem> GetGameList()
        {
            ObservableCollection<ListItem> GamesList = new ObservableCollection<ListItem>();
            foreach(var game in Games)
            {
                string currentGameName = "";
                Games.TryGetValue(game.Key, out currentGameName);
                GamesList.Add(new ListItem(currentGameName));
            }
            return GamesList;
        }

        
    }
}
