using ARPEGOS.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS_Test
{
    public class Character
    {
        string name, category;
        CharacterOntologyService characterService;

        public string Name { get => name; set => name = value; }
        public string Category { get => category; set => category = value; }
        public CharacterOntologyService CharacterService { get => characterService; set => characterService = value; }

        public Character( string name, string category = "Novel")
        {
            this.Name = name;
            this.Category = category;
            this.CharacterService = OntologyService.CreateCharacter(this.Name , Setup.Game).GetAwaiter().GetResult();
        }
    }
}
