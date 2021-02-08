using ARPEGOS.Helpers;
using ARPEGOS.Services;
using ARPEGOS_Test.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ARPEGOS_Test
{
    class Program
    {
        /// <summary>
        /// Current game
        /// </summary>

        static void Main()
        {
            Setup.GameName = "Anima Beyond Fantasy";
            Setup.GameVersion = "Core Exxet";
            DependencyHelper.CurrentContext.CurrentGame = OntologyService.LoadGame(Setup.GameName , Setup.GameVersion).GetAwaiter().GetResult();
            Setup.Game = DependencyHelper.CurrentContext.CurrentGame;
            Setup.Characters = new List<Character> 
            { 
                new Character("ctuchik","hechicero_mentalista"),
                new Character("kvothe","asesino"),
                new Character("morgana le fey", "conjurador"),
                new Character("legolas", "explorador"),
                new Character("angus mcfife XIII","guerrero"),
                new Character("seda","guerrero_acróbata"),
                new Character("elizabetta barbados","guerrero_conjurador"),
                new Character("durnik","guerrero_mentalista"),
                new Character("elminster","hechicero"),
                new Character("nathan ford","ilusionista"),
                new Character("parker","ladrón"),
                new Character("mario auditore","maestro_en_armas"),
                new Character("patrick jane","mentalista"),
                new Character("uriel artholim karalden"),
                new Character("sparhawk","paladín"),
                new Character("drizzt","paladín_oscuro"),
                new Character("brill", "sombra"),
                new Character("sun wukong","tao"),
                new Character("zoro roronoa", "tecnicista"),
                new Character("belgarion","warlock")
            };
            Setup.SetBaseFolder();
            Setup.ShowPresentation();
            var currentCharacterIndex = 0;
            foreach (var item in Setup.Characters)
            {
                Console.WriteLine($"| / -------------------- \\ |\n");
                Console.WriteLine($"Next Character = {Setup.Characters.ElementAt(currentCharacterIndex)}\n");
                Console.WriteLine($"| \\ -------------------- / |\n");
                new Console_CreationView();
            }
            Setup.SetDefaultBaseFolder();
        }
    }
}
