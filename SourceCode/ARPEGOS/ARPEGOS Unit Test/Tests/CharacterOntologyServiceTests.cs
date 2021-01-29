using ARPEGOS;
using ARPEGOS.Helpers;
using ARPEGOS.Services;
using NUnit.Framework;
using RDFSharp.Model;
using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARPEGOS_Unit_Test.Tests
{
    [TestFixture]
    public partial class CharacterOntologyServiceTests
    {
        private GameOntologyService game;
        private CharacterOntologyService character;

        public GameOntologyService Game { get => game; set => game = value; }
        public CharacterOntologyService Character { get => character; set => character = value; }

        [OneTimeSetUp]
        public async Task Init()
        {
            Game = DependencyHelper.CurrentContext.CurrentGame;
            try
            {
                Character = await OntologyService.LoadCharacter(Setup.CharacterName, Game);
            }
            catch (RDFModelException)
            {
                Character = await OntologyService.CreateCharacter(Setup.CharacterName, Game);
            }
            DependencyHelper.CurrentContext.CurrentCharacter = Character;
        }

        /*
         * All tests must be done with existing elements in the database
         */
    }
}
