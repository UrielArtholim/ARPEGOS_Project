using ARPEGOS_Test.Models;
using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arpegos_Test.Views
{
    public class SingleChoiceViewModel
    {
        /// <summary>
        /// Subroutine that simulates a Xamarin View List for Single Choice
        /// </summary>
        /// <param name="stage">Etapa del proceso de creación que tiene que mostrar</param>
        public SingleChoiceViewModel(string stage, out string choice)
        {
            choice = null;
            Group StageGroup = new Group(stage);
            bool elementSelected = false;
            if (stage != "Categoría")
            {
                while (elementSelected == false)
                {
                    StageGroup.ShowGroup();
                    Console.WriteLine("Seleccione una única opción");
                    string input = Game.Text.ToTitleCase(Console.ReadLine());
                    choice = Program.CheckInput(StageGroup.GroupList, input);
                    if (choice != null)
                        elementSelected = true;
                }
            }
            else
            {
                KeyValuePair<string, string> character = Program.Characters.Where(item => Game.Text.ToTitleCase(item.Value).Replace(' ','_') == Program.Game.CurrentCharacterName).SingleOrDefault();
                choice = Game.Text.ToTitleCase(stage + "_" + character.Key);
            }

            string stageName = stage.Replace("_", "");
            IEnumerable<RDFOntologyProperty> StageProperties = Program.Game.GameOntology.Model.PropertyModel.Where(item => item.ToString().Contains(stageName));
            RDFOntologyProperty StageObjectProperty = StageProperties.Where(item => item.Range != null && item.Range.ToString().Contains(stage)).SingleOrDefault();
            string predicate = StageObjectProperty.ToString().Split('#').LastOrDefault();
            Program.Game.AddObjectProperty(Program.Game.CurrentCharacterContext + Program.Game.CurrentCharacterName, Program.Game.CurrentCharacterContext + predicate, Program.Game.CurrentCharacterContext + choice);
            Program.Game.AddClassification(predicate);
            Console.Clear();

        }
    }
}
