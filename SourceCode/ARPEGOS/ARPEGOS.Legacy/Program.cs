using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ARPEGOS
{
    using ARPEGOS.Models;

    /// <summary>
    /// Console Application to test ARPEGOS PROJECT functionality
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Current game
        /// </summary>
        public static Game Game { get; set; } = new Game();
        public static List<string> CreationScheme { get; internal set; } = null;
        public static int Counter { get; set; } = 2;
        public static Dictionary<string, string> Characters = new Dictionary<string, string>()
        {
            {"hechicero_mentalista" ,"ctuchik"},
            {"asesino" ,"kvothe"},
            {"conjurador" ,"morgana le fey"},
            {"explorador" ,"legolas"},
            {"guerrero" ,"angus mcfife XIII"},
            {"guerrero_acróbata" ,"seda"},
            {"guerrero_conjurador" ,"elizabetta barbados"},
            {"guerrero_mentalista" ,"durnik"},
            {"hechicero" ,"elminster"},
            {"ilusionista" ,"nathan ford"},
            {"ladrón" ,"parker"},
            {"maestro_en_armas" ,"mario auditore"},
            {"mentalista" ,"patrick jane"},
            {"novel" ,"uriel artholim karalden"},
            {"paladín" ,"sparhawk"},
            {"paladín_oscuro" ,"drizzt"},
            {"sombra" ,"brill"},
            {"tao" ,"sun wukong"},
            {"tecnicista" ,"zoro roronoa"},
            {"warlock" ,"belgarion"}
        };

        static void Main()
        {
            ShowPresentation();
            Game.Text.ToTitleCase(Console.ReadLine());

            foreach(KeyValuePair<string, string> character in Characters)
            {
                // Create Character
                Game.CreateCharacter(Game.Text.ToTitleCase(character.Value));

                // Get Creation Scheme Root Class
                string rootName = Game.GetCreationSchemeRootClass();

                // Get Character Class Value
                new SingleChoiceViewModel(rootName, out string choice);
                RDFOntologyFact RootFact = Game.GameOntology.Data.SelectFact(Game.CurrentGameContext + choice);

                // Get CreationScheme
                RDFOntologyTaxonomyEntry CreationSchemeAnnotation = Game.GameOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(RootFact).SingleOrDefault();
                List<string> CreationScheme = CreationSchemeAnnotation.TaxonomyObject.ToString().Replace("\n", "").Replace(" ", "").Replace("^^http://www.w3.org/2001/XMLSchema#string", "").Split(",").ToList();
                
                // Iterate over every stage of creation scheme
                foreach (string stage in CreationScheme)
                {
                    new SelectViewType(CreationScheme, stage, Counter);
                    ++Counter;
                }
            }
        }

       

        #region Console_Functions

        internal static void ShowPresentation()
        {
            Console.WriteLine("|------------------------------------------|");
            Console.WriteLine("|              ARPEGOS PROJECT             |");
            Console.WriteLine("|------------------------------------------|");
            Console.WriteLine("|Copyright 2020 - Alejandro Muñoz Del Álamo|");
            Console.WriteLine("|------------------------------------------|\n\n\n");

            Console.WriteLine("|------------------------------------------|");
            Console.WriteLine("|        ARPEGOS PROJECT TEST SUITE        |");
            Console.WriteLine("|------------------------------------------|");
            Console.WriteLine("|Copyright 2020 - Alejandro Muñoz Del Álamo|");
            Console.WriteLine("|------------------------------------------|\n\n\n");

            Console.WriteLine("Pulse Intro para empezar las pruebas.");
        }

        internal static void ShowStage(string stage, int Counter)
        {
            string StageTitle = "| Current stage: " + Counter + "|-> Character " + stage.Replace('_', ' ').Trim() + " |";
            Console.WriteLine(TitleLenghtBar(StageTitle));
            Console.WriteLine(StageTitle);
            Console.WriteLine(TitleLenghtBar(StageTitle));
            Console.WriteLine("\n\n");
        }

        internal static void ShowOptions(dynamic options)
        {
            if (options.GetType().ToString().Contains("ARPEGOS.Item"))
            {
                foreach (var item in options)
                {
                    Console.WriteLine("##################################\n");
                    Console.WriteLine("Item Name: " + item.FormattedName);
                    Console.WriteLine("Item Class: " + item.Class);
                    Console.WriteLine("Item Description: " + item.Description + "\n\n");
                }
                Console.WriteLine("##################################\n");
                Console.WriteLine("Seleccione una opción");
            }
            else
            {
                foreach (var group in options)
                {
                    Console.WriteLine("##################################\n");
                    Console.WriteLine("Item Name: " + group.FormattedTitle);
                    Console.WriteLine("Item Description: " + group.Description + "\n\n");
                }
                Console.WriteLine("##################################\n");
                Console.WriteLine("Seleccione una opción");
            }
        }

        internal static string CheckInput(IEnumerable<dynamic> optionslist, string input)
        {
            bool SingleCoincidence = false;
            string result = null;
            int Counter;

            List<string> SelectedWords = input.Replace(" ", "_").Split("_").ToList();
            bool optionsAreItems = optionslist.GetType().ToString().Contains("Item");
            if (optionsAreItems)
            {
                bool end = false;
                IEnumerable<Item> options = optionslist as IEnumerable<Item>;
                while (SingleCoincidence == false && end == false)
                {
                    SelectedWords = input.Replace(" ", "_").Split("_").ToList();
                    Counter = options.Where(item => SelectedWords.Any(word => item.Name.ToLower().Contains(word.ToLower())) || SelectedWords.Any(word => item.FormattedName.ToLower().Contains(word.ToLower()))).Count();
                    if (Counter == 1)
                    {
                        SingleCoincidence = true;
                        result = options.FirstOrDefault(item => SelectedWords.Any(word => item.Name.ToLower().Contains(word.ToLower())) || SelectedWords.Any(word => item.FormattedName.ToLower().Contains(word.ToLower()))).Name;
                    }
                    else
                    {
                        Counter = options.Where(item => item.Name.ToLower() == input.ToLower() || item.FormattedName.ToLower() == input.ToLower()).Count();
                        if (Counter == 1)
                        {
                            SingleCoincidence = true;
                            result = options.FirstOrDefault(item => item.Name.ToLower() == input.ToLower() || item.FormattedName.ToLower() == input.ToLower()).Name;
                        }
                        else
                        {

                            IEnumerable<Item> coincidences = options.Where(item => SelectedWords.Any(word => item.Name.ToLower().Contains(word.ToLower())) || SelectedWords.Any(word => item.FormattedName.ToLower().Contains(word.ToLower())));
                            foreach(string word in SelectedWords)
                                coincidences = coincidences.Where(item =>item.Name.ToLower().Contains(word.ToLower()) || item.FormattedName.ToLower().Contains(word.ToLower()));

                            if (coincidences.Count() != 0)
                            {
                                ShowOptions(coincidences);
                                input = Game.Text.ToTitleCase(Console.ReadLine());
                                Console.Clear();
                            }
                            else
                                end = true;
                        }
                    }
                }
            }
            else
            {
                bool resultFound = false;
                foreach (Group option in optionslist)
                {
                    if(resultFound == false)
                    {
                        result = CheckInput(option.GroupList, input);
                        if (result != null)
                            resultFound = true;
                    }
                }
            }
            return result;
        }

        internal static string TitleLenghtBar(string title)
        {
            string bar = "|";
            for (int i = 0; i < title.Length -2; ++i)
                bar += '-';
            return bar += '|';
        }

        #endregion
    }
}
