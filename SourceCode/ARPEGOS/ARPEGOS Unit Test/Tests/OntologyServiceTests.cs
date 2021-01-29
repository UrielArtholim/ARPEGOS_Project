using ARPEGOS.Helpers;
using ARPEGOS.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ARPEGOS_Unit_Test.Tests
{
    [TestFixture]
    public class OntologyServiceTests
    {
        [Test]
        public async Task LoadGame_Test()
        {
            var currentGame = await OntologyService.LoadGame(Setup.GameName, Setup.GameVersion);
            Assert.AreEqual(Setup.GameName, currentGame.FormattedName);
        }

        [Test]
        public async Task CreateCharacter_Test()
        {
            var currentGame = DependencyHelper.CurrentContext.CurrentGame;
            var characterFolder = Path.Combine(Setup.GamesFolder, Setup.GameName, "characters");
            var directoryInfo = new DirectoryInfo(characterFolder);
            var files = directoryInfo.GetFiles();
            if(files.Where(file => file.Name == Setup.CharacterName).Count() > 0)
                await OntologyService.CreateCharacter(Setup.CharacterName, currentGame);
            var currentCharacter = await OntologyService.CreateCharacter(Setup.CharacterName, currentGame);
            Assert.AreEqual(Setup.CharacterName, currentCharacter.FormattedName);
        }

        [Test]
        public async Task LoadCharacter_Test()
        {
            var characterName = "Test";
            var currentGame = DependencyHelper.CurrentContext.CurrentGame;
            var currentCharacter = await OntologyService.LoadCharacter(characterName, currentGame);
            if (DependencyHelper.CurrentContext.CurrentCharacter == null)
                DependencyHelper.CurrentContext.CurrentCharacter = currentCharacter;
            Assert.AreEqual(characterName, currentCharacter.FormattedName);
        }

        [Test]
        public async Task CheckIfCharacterExistsAtCreate_Test()
        {
            var characterName = "Test";
            var message = string.Empty;
            var currentGame = DependencyHelper.CurrentContext.CurrentGame;
            var currentCharacter = DependencyHelper.CurrentContext.CurrentCharacter;
            if (currentCharacter == null || currentCharacter.Name != characterName)
                currentCharacter = await OntologyService.LoadCharacter(characterName, currentGame);
            try
            {
                currentCharacter = await OntologyService.CreateCharacter(characterName, currentGame);
            }
            catch(ArgumentException e)
            {
                message = e.Message;
            }
            Assert.AreEqual($"Character {characterName} already exists", message);
        }

        [Test]
        public async Task DeleteCharacter_Test()
        {
            var currentGame = DependencyHelper.CurrentContext.CurrentGame;
            CharacterOntologyService currentCharacter;
            try
            {
                currentCharacter = await OntologyService.CreateCharacter(Setup.CharacterName, currentGame);
            }
            catch (ArgumentException)
            {
                currentCharacter = await OntologyService.LoadCharacter(Setup.CharacterName, currentGame);
            }
            DependencyHelper.CurrentContext.CurrentCharacter = currentCharacter;

            bool characterFileExistsBefore = File.Exists(FileService.GetCharacterFilePath(currentCharacter));
            await OntologyService.DeleteCharacter(currentCharacter.Name, currentGame);
            bool characterFileExistsAfter = File.Exists(FileService.GetCharacterFilePath(currentCharacter));
            Assert.AreNotEqual(characterFileExistsBefore, characterFileExistsAfter);
        }
    }
}
