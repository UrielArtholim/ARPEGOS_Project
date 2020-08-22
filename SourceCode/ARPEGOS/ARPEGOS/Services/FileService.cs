
namespace ARPEGOS.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;

    public static class FileService
    {
        private static TextInfo ti => Thread.CurrentThread.CurrentCulture.TextInfo;

        private static string BaseFolder => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static string GamesPath = "gamefiles";

        public static string CharactersPath = "characters";

        /// <summary>
        /// Gets the formatted name of the games stored in the device
        /// </summary>
        /// <returns> Formatted names </returns>
        public static IEnumerable<string> ListGames()
        {
            var games = Directory.GetDirectories(BaseFolder);
            return games.Select(s => s.Split('/').Last().Split('.').First().Replace("_", " "));
        }

        /// <summary>
        /// Gets the formatted name of the versions stored in the device for the given game
        /// </summary>
        /// <param name="game"> Game from which we want to recover the versions </param>
        /// <returns> Formatted names </returns>
        public static IEnumerable<string> ListVersions(string game)
        {
            var path = Path.Combine(BaseFolder, EscapedName(game), GamesPath);
            var versions = Directory.GetFiles(path);
            return versions.Select(s => s.Split('/').Last().Split('.').First().Replace("_", " "));
        }

        /// <summary>
        /// Gets the formatted name of the characters stored in the device for the given game
        /// </summary>
        /// <param name="game"> Game from which we want to recover the characters </param>
        /// <returns> Formatted names </returns>
        public static IEnumerable<string> ListCharacters(string game)
        {
            var path = Path.Combine(BaseFolder, EscapedName(game), CharactersPath);
            var characters = Directory.GetFiles(path);
            return characters.Select(s => s.Split('/').Last().Split('.').First().Replace("_", " "));
        }

        public static bool CreateGameFolderStructure(string game)
        {
            var escaped = EscapedName(game);
            if (Directory.Exists(Path.Combine(BaseFolder, escaped)))
                return false;
            Directory.CreateDirectory(Path.Combine(BaseFolder, escaped, GamesPath));
            Directory.CreateDirectory(Path.Combine(BaseFolder, escaped, CharactersPath));
            return true;
        }

        public static bool DeleteGame(string name)
        {
            var path = GetGameBasePath(name);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                return true;
            }

            return false;
        }

        public static bool DeleteGameVersion(string game, string version)
        {
            var path = GetGameFilePath(game, version);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }

            return false;
        }

        public static bool DeleteCharacter(string name, string game)
        {
            var path = GetCharacterFilePath(name, game);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }

            return false;
        }

        public static string GetGameBasePath(string name)
        {
            return Path.Combine(BaseFolder, EscapedName(name));
        }

        public static string GetGameBasePath(CharacterOntologyService character)
        {
            return GetGameBasePath(character.Game.Name);
        }

        public static string GetGameBasePath(GameOntologyService game)
        {
            return GetGameBasePath(game.Name);
        }

        public static string GetGameFilePath(string name, string version)
        {
            return Path.Combine(GetGameBasePath(name), GamesPath, FileName(version));
        }

        public static string GetCharacterFilePath(string name, string gameName)
        {
            return Path.Combine(GetGameBasePath(gameName), CharactersPath, FileName(name));
        }

        public static string GetCharacterFilePath(string name, GameOntologyService game)
        {
            return GetCharacterFilePath(name, game.Name);
        }

        public static string GetCharacterFilePath(CharacterOntologyService character)
        {
            return GetCharacterFilePath(character.Name, character.Game.Name);
        }

        /// <summary>
        /// Gets the user friendly name g.e. glenn radars => Glenn Radars
        /// </summary>
        /// <param name="name"> Name to format </param>
        /// <returns> Formatted Name </returns>
        public static string FormatName(string name)
        {
            return ti.ToTitleCase(name);
        }

        /// <summary>
        /// Gets the formatted escaped name g.e. glenn radars => Glenn_Radars
        /// </summary>
        /// <param name="name"> Name to escape </param>
        /// <returns> Escaped Name </returns>
        public static string EscapedName(string name)
        {
            return FormatName(name).Replace(" ", "_");
        }

        /// <summary>
        /// Gets the file name g.e. glenn radars => Glenn_Radars.owl
        /// </summary>
        /// <param name="name"> Given name </param>
        /// <returns> Name of the file </returns>
        public static string FileName(string name)
        {
            return $"{EscapedName(name)}.owl";
        }

        /// <summary>
        /// Returns invariant culture info
        /// </summary>
        /// <returns></returns>
        public static CultureInfo Culture()
        {
            return CultureInfo.InvariantCulture;
        }
    }
}
