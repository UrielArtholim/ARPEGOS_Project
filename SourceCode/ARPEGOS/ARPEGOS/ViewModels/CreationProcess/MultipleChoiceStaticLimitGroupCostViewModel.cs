using RDFSharp.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arpegos_Test.ViewModels
{
    public class MultipleChoiceStaticLimitGroupCostViewModel
    {
        /// <summary>
        /// Subroutine that simulates a Xamarin View List for Multiple Choice
        /// </summary>
        /// <param name="stage">Etapa del proceso de creación que tiene que mostrar</param>
        public MultipleChoiceStaticLimitGroupCostViewModel(string stage, float? LimitValue, out float? ReturnLimitValue)
        {
            ReturnLimitValue = LimitValue;

            string finish = "Siguiente";
            string choice = null;
            List<string> choices = new List<string>();
            string LimitName = Program.Game.GetLimit(stage, out float? stageLimitValue);
            bool hasLimit = LimitName != null;

            Group StageGroup = new Group(stage);
            while (choice == null || choice.ToLower() != finish.ToLower())
            {
                StageGroup.ShowGroup();
                Console.WriteLine("\n\n");

                if (choices.Count > 0)
                {
                    Console.WriteLine("\n\nElementos seleccionados");
                    Console.WriteLine("|------------------------------------------|");
                    foreach (string item in choices)
                        Console.WriteLine(item);
                    Console.WriteLine("|------------------------------------------|");
                    Console.WriteLine("\n\n");
                }

                Console.WriteLine("Seleccione una opción. Si ha terminado, escriba \"" + finish + "\"");
                string input = Console.ReadLine().ToLower();
                if (input == finish.ToLower())
                    choice = finish;
                else
                    choice = Program.CheckInput(StageGroup.GroupList, input);
                if (choice != null)
                {
                    if (choice != finish && choices.Contains(choice) == false)
                        choices.Add(choice);
                }
                Console.Clear();
            }
            choice = stage;
            foreach (string element in choices)
            {
                RDFOntologyProperty choiceProperty = Program.Game.GameOntology.Model.PropertyModel.Where(item => item.Range != null && item.Range.ToString().Contains(stage)).SingleOrDefault();
                string predicate = choiceProperty.ToString().Substring(choiceProperty.ToString().LastIndexOf('#') + 1);
                Program.Game.AddObjectProperty(Program.Game.CurrentCharacterContext + Program.Game.CurrentCharacterName, Program.Game.CurrentCharacterContext + predicate, Program.Game.CurrentCharacterContext + element);
                Program.Game.AddClassification(predicate);
            }

            Console.Clear();
        }
    }
}
