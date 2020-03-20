using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Semantics;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ARPEGOS.Models
{
    /// <summary>
    /// Game represents the information container of any game within the app
    /// </summary>
    public class Game
    {
        #region Properties
        /// <summary>
        /// Format standard for saving and reading game files
        /// </summary>
        static readonly RDFModelEnums.RDFFormats RdfFormat = RDFModelEnums.RDFFormats.RdfXml;
        
        /// <summary>
        /// Name of the current game selected
        /// </summary>
        public string CurrentGameName { get; set; }

        /// <summary>
        /// Path of the current game file
        /// </summary>
        public string GameDBFile { get; internal set; }

        
        public string CurrentGameContext { get; internal set; }

        /// <summary>
        /// Name of the current character
        /// </summary>
        public string CurrentCharacterName { get; set; }

        /// <summary>
        /// Path of the current character file
        /// </summary>
        public string CurrentCharacterFile { get; internal set; }

        /// <summary>
        /// Base URI of the current character selected
        /// </summary>
        public string CurrentCharacterContext { get; internal set; }

        /// <summary>
        /// Semantic representation of the current game
        /// </summary>
        public RDFOntology GameOntology { get; internal set; }

        /// <summary>
        /// Semantic representation of the current character
        /// </summary>
        public RDFOntology CharacterOntology { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Ctor to build a game given the name of the game and its version
        /// </summary>
        /// <param name="GameName">Name of the selected game </param>
        /// <param name="GameVersion">Version of the selected game </param>
        public Game(string GameName, string GameVersion)
        {
            CurrentGameName = GameName.Substring(GameName.LastIndexOf('/') + 1);
            CurrentCharacterName = "TestCharacter";
            CurrentCharacterFile = Path.Combine("F:/Alejandro/Xamarin/OWL Project/characters", CurrentCharacterName + ".owl");
            CurrentCharacterContext = "http://ARPEGOS_Project/Games/" + CurrentGameName + "/characters/" + CurrentCharacterName + "#";
            string CurrentGamePrefix = string.Concat(Regex.Matches(CurrentGameName, "[A-Z]").OfType<Match>().Select(match =>match.Value)).ToLower();

            GameDBFile = GameName + GameVersion;
            RDFGraph GameGraph = RDFGraph.FromFile(RdfFormat, GameDBFile);
            CurrentGameContext = GameGraph.Context.ToString() + '#';
            GameGraph.SetContext(new Uri(CurrentGameContext));
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(CurrentGamePrefix, GameGraph.Context.ToString()));
            GameOntology = RDFOntology.FromRDFGraph(GameGraph);

            RDFGraph CharacterGraph = new RDFGraph();
            CharacterGraph.SetContext(new Uri(CurrentCharacterContext));
            CharacterOntology = RDFOntology.FromRDFGraph(CharacterGraph);
        }

        /// <summary>
        /// Default-ctor to build an example game for function testing
        /// </summary>
        public Game()
        {
            // Override gamepath variable to the path of the game you want to set as default
            string gamepath = "F:/Alejandro/ARPEGOS_Project/Ontologies/Core Exxet.owl";
            CurrentGameName = "Anima_Beyond_Fantasy";
            CurrentCharacterName = "TestCharacter";
            CurrentCharacterFile = Path.Combine("F:/Alejandro/Xamarin/OWL Project/characters", CurrentCharacterName + ".owl");
            CurrentCharacterContext = "http://ARPEGOS_Project/Games/" + CurrentGameName + "/characters/" + CurrentCharacterName + "#";
            string CurrentGamePrefix = string.Concat(Regex.Matches(CurrentGameName, "[A-Z]").OfType<Match>().Select(match =>match.Value)).ToLower();

            GameDBFile = gamepath;
            RDFGraph GameGraph = RDFGraph.FromFile(RdfFormat, gamepath);
            CurrentGameContext = GameGraph.Context.ToString() + '#';
            GameGraph.SetContext(new Uri(CurrentGameContext));
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(CurrentGamePrefix, GameGraph.Context.ToString()));
            GameOntology = RDFOntology.FromRDFGraph(GameGraph);

            RDFGraph CharacterGraph = new RDFGraph();
            CharacterGraph.SetContext(new Uri(CurrentCharacterContext));
            CharacterOntology = RDFOntology.FromRDFGraph(CharacterGraph);
        }
        #endregion

        #region Methods

        #region Save
        /// <summary>
        /// Saves current character info
        /// </summary>
        public void SaveCharacter()
        {
            RDFGraph CharacterGraph = CharacterOntology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData);
            CharacterGraph.ToFile(RdfFormat, CurrentCharacterFile);
        }
        #endregion



        #endregion
    }
}

