using ARPEGOS.Helpers;
using ARPEGOS.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARPEGOS_Unit_Test.Tests
{
    [SetUpFixture]
    
    public class Setup
    {
        #region Attributes

        static string baseFolder, gamesFolder;
        static string gameName, gameVersion, characterName;

        #endregion
        #region Attribute Methods

        public static string BaseFolder { get => baseFolder; set => baseFolder = value; }
        public static string GamesFolder { get => gamesFolder; set => gamesFolder = value; }
        public static string GameName { get => gameName; set => gameName = value; }
        public static string GameVersion { get => gameVersion; set => gameVersion = value; }
        public static string CharacterName { get => characterName; set => characterName = value; }

        #endregion

        #region Attributes Initialization
        [OneTimeSetUp]
        public async Task Init()
        {
            _ = new DependencyHelper();
            Xamarin.Forms.Mocks.MockForms.Init();

            BaseFolder = FileService.GetBaseFolder();
            SetBaseFolder();
            GameName = "Anima Beyond Fantasy";
            GameVersion = "Core Exxet";
            CharacterName = "Guybrush Threepwood";
            DependencyHelper.CurrentContext.CurrentGame = await OntologyService.LoadGame(Setup.GameName, Setup.GameVersion);
        }

        public void SetBaseFolder()
        {
            GamesFolder = System.AppDomain.CurrentDomain.BaseDirectory;
            var directoryInfo = new DirectoryInfo(GamesFolder);
            while (directoryInfo.Name != "ARPEGOS")
                directoryInfo = directoryInfo.Parent;

            GamesFolder = Path.Combine(directoryInfo.FullName);
            var folders = new DirectoryInfo(GamesFolder).GetDirectories();
            var nextFolder = folders.Where(folder => folder.Name == "ARPEGOS").Single();
            GamesFolder = Path.Combine(GamesFolder, nextFolder.Name);
            folders = new DirectoryInfo(GamesFolder).GetDirectories();
            nextFolder = folders.Where(folder => folder.Name == "DefaultGames").Single();
            GamesFolder = Path.Combine(GamesFolder, nextFolder.Name);
            FileService.SetBaseFolder(GamesFolder);
        }

        [OneTimeTearDown]
        public void SetDefaultBaseFolder()
        {
            FileService.ResetFolderPath();
            Console.WriteLine("Base folder restored");
        }
        #endregion
    }
}
