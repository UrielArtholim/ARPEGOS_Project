using ARPEGOS;
using ARPEGOS.Services;
using ARPEGOS.ViewModels;
using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ARPEGOS_Test
{
    public static class Setup
    {
        #region Properties
        static GameOntologyService game;
        static List<Character> characters;
        static Character currentCharacter = null;
        static string baseFolder;
        static string gameName, gameVersion, characterName;
        #endregion

        #region Property Methods
        public static GameOntologyService Game { get => game; set => game = value; }
        public static Character CurrentCharacter { get => currentCharacter; set => currentCharacter = value; }
        public static List<Character> Characters { get => characters; set => characters = value; }
        public static string BaseFolder { get => baseFolder; set => baseFolder = value; }
        public static string GameName { get => gameName; set => gameName = value; }
        public static string GameVersion { get => gameVersion; set => gameVersion = value; }
        public static string CharacterName { get => characterName; set => characterName = value; }

        #endregion

        #region Methods
        public static void SetBaseFolder()
        {
            var gamefolder = System.AppDomain.CurrentDomain.BaseDirectory;
            var directoryInfo = new DirectoryInfo(gamefolder);
            while (directoryInfo.Name != "ARPEGOS")
                directoryInfo = directoryInfo.Parent;

            gamefolder = Path.Combine(directoryInfo.FullName);
            var folders = new DirectoryInfo(gamefolder).GetDirectories();
            var nextFolder = folders.Where(folder => folder.Name == "ARPEGOS").Single();
            gamefolder = Path.Combine(gamefolder , nextFolder.Name);
            folders = new DirectoryInfo(gamefolder).GetDirectories();
            nextFolder = folders.Where(folder => folder.Name == "DefaultGames").Single();
            gamefolder = Path.Combine(gamefolder , nextFolder.Name);
            FileService.SetBaseFolder(gamefolder);
        }

        public static void SetDefaultBaseFolder()
        {
            FileService.ResetFolderPath();
            Console.WriteLine("Base folder restored");
        }
        public static void ShowPresentation()
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

        internal static void ShowStage( string stage , int Counter )
        {
            string StageTitle = "| Current stage: " + Counter + "|-> Character " + stage.Replace('_' , ' ').Trim() + " |";
            Console.WriteLine(TitleLenghtBar(StageTitle));
            Console.WriteLine(StageTitle);
            Console.WriteLine(TitleLenghtBar(StageTitle));
            Console.WriteLine("\n\n");
        }

        public static void ShowOptions( dynamic options )
        {
            if (options.GetType().ToString().Contains("Arpegos_Test.Item"))
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

        public static string CheckInput( IEnumerable<dynamic> optionslist , string input )
        {
            bool SingleCoincidence = false;
            string result = null;
            int Counter;

            List<string> SelectedWords = input.Replace(" " , "_").Split("_").ToList();
            bool optionsAreItems = optionslist.GetType().ToString().Contains("Item");
            if (optionsAreItems)
            {
                bool end = false;
                IEnumerable<Item> options = optionslist as IEnumerable<Item>;
                while (SingleCoincidence == false && end == false)
                {
                    SelectedWords = input.Replace(" " , "_").Split("_").ToList();
                    Counter = options.Where(item => SelectedWords.Any(word => item.FullName.Split('#').Last().ToLower().Contains(word.ToLower())) || SelectedWords.Any(word => item.FormattedName.ToLower().Contains(word.ToLower()))).Count();
                    if (Counter == 1)
                    {
                        SingleCoincidence = true;
                        result = options.FirstOrDefault(item => SelectedWords.Any(word => item.FullName.Split('#').Last().ToLower().Contains(word.ToLower())) || SelectedWords.Any(word => item.FormattedName.ToLower().Contains(word.ToLower()))).FullName.Split('#').Last();
                    }
                    else
                    {
                        Counter = options.Where(item => item.FullName.Split('#').Last().ToLower() == input.ToLower() || item.FormattedName.ToLower() == input.ToLower()).Count();
                        if (Counter == 1)
                        {
                            SingleCoincidence = true;
                            result = options.FirstOrDefault(item => item.FullName.Split('#').Last().ToLower() == input.ToLower() || item.FormattedName.ToLower() == input.ToLower()).FullName.Split('#').Last();
                        }
                        else
                        {

                            IEnumerable<Item> coincidences = options.Where(item => SelectedWords.Any(word => item.FullName.Split('#').Last().ToLower().Contains(word.ToLower())) || SelectedWords.Any(word => item.FormattedName.ToLower().Contains(word.ToLower())));
                            foreach (string word in SelectedWords)
                                coincidences = coincidences.Where(item => item.FullName.Split('#').Last().ToLower().Contains(word.ToLower()) || item.FormattedName.ToLower().Contains(word.ToLower()));

                            if (coincidences.Count() != 0)
                            {
                                ShowOptions(coincidences);
                                input = FileService.FormatName(Console.ReadLine());
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
                    if (resultFound == false)
                    {
                        result = CheckInput(option.Elements , input);
                        if (result != null)
                            resultFound = true;
                    }
                }
            }
            return result;
        }

        public static string TitleLenghtBar( string title )
        {
            string bar = "|";
            for (int i = 0; i < title.Length - 2; ++i)
                bar += '-';
            return bar += '|';
        }

        #endregion
    }
}
