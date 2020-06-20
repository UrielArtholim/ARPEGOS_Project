using Arpegos_Test;
using RDFSharp.Model;
using RDFSharp.Semantics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Group = Arpegos_Test.Group;

namespace ARPEGOS.Models
{
    /// <summary>
    /// Game represents the information container of any game within the app
    /// </summary>
    public class Game
    {

        public static string ProjectPath { get; internal set; } = "F:/Alejandro/Xamarin/Arpegos Test/";
        public string GameFolder { get; internal set; } = ProjectPath + "Games/";
        public string CharacterFolder { get; internal set; } = ProjectPath +"Characters/";

        #region Properties
        /// <summary>
        /// Reference to the current culture used in runtime
        /// </summary>
        internal static CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;

        /// <summary>
        /// Reference to the text information of the current culture used in runtime
        /// </summary>
        public static TextInfo Text = cultureInfo.TextInfo;

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
        public string CurrentGameFile { get; internal set; }

        /// <summary>
        /// Base URI of the current game selected
        /// </summary>
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
            CurrentCharacterFile = Path.Combine(CharacterFolder, CurrentCharacterName + ".owl");
            CurrentCharacterContext = "http://arpegos_project/Games/" + CurrentGameName + "/characters/" + CurrentCharacterName + "#";
            string CurrentGamePrefix = string.Concat(Regex.Matches(CurrentGameName, "[A-Z]").OfType<Match>().Select(match =>match.Value)).ToLower();

            CurrentGameFile = GameName + GameVersion;
            RDFGraph GameGraph = RDFGraph.FromFile(RdfFormat, CurrentGameFile);
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
            string gamepath = GameFolder + "Anima_Beyond_Fantasy/Core Exxet.owl";
            CurrentGameName = "Anima_Beyond_Fantasy";
            CurrentCharacterName = "TestCharacter";
            CurrentCharacterFile = Path.Combine(CharacterFolder, CurrentCharacterName + ".owl");
            CurrentCharacterContext = "http://arpegos_project/Games/" + CurrentGameName + "/characters/" + CurrentCharacterName + "#";
            string CurrentGamePrefix = string.Concat(Regex.Matches(CurrentGameName, "[A-Z]").OfType<Match>().Select(match =>match.Value)).ToLower();

            CurrentGameFile = gamepath;
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

        #region Add
        /// <summary>
        /// Asserts the object property between subject and object inside the character
        /// </summary>
        /// <param name="subjectFact">Fact that serves as subject of the assertion</param>
        /// <param name="predicate">Object property that serves as the predicate of the assertion</param>
        /// <param name="objectFact">Fact that serves as object of the assertion</param>
        internal void AddObjectProperty(string subjectFullName, string predicateFullName, string objectFullName)
        {
            RDFOntologyData CharacterDataModel = CharacterOntology.Data;
            RDFOntologyFact subjectFact, objectFact;
            RDFOntologyProperty predicate;

            string subjectName = subjectFullName.ToString().Substring(subjectFullName.ToString().LastIndexOf('#') + 1);
            string predicateName = predicateFullName.ToString().Substring(predicateFullName.ToString().LastIndexOf('#') + 1);
            string objectName = objectFullName.ToString().Substring(objectFullName.ToString().LastIndexOf('#') + 1);
            if (!CheckIndividual(subjectName))
                subjectFact = CreateIndividual(subjectName);
            else
                subjectFact = CharacterDataModel.SelectFact(subjectFullName);

            if (!CheckObjectProperty(predicateName))
                predicate = CreateObjectProperty(predicateName);
            else
                predicate = CharacterOntology.Model.PropertyModel.SelectProperty(predicateFullName);

            if (!CheckIndividual(objectName))
                objectFact = CreateIndividual(objectName);
            else
                objectFact = CharacterDataModel.SelectFact(objectFullName);

            CharacterDataModel.AddAssertionRelation(subjectFact, predicate as RDFOntologyObjectProperty, objectFact);
            SaveCharacter();
        }

        /// <summary>
        /// Asserts the datatype property between subject and object inside the character
        /// </summary>
        /// <param name="subjectFact">Fact that serves as subject of the assertion</param>
        /// <param name="predicate">Datatype property that serves as the predicate of the assertion</param>
        /// <param name="objectFact">Literal that serves as object of the assertion</param>
        internal void AddDatatypeProperty(string subjectFullName, string predicateFullName, string value, string valuetype)
        {
            RDFOntologyData CharacterDataModel = CharacterOntology.Data;
            RDFOntologyFact subjectFact;
            RDFOntologyProperty predicate;
            RDFOntologyLiteral objectLiteral;
            string subjectName = subjectFullName.ToString().Substring(subjectFullName.ToString().LastIndexOf('#') + 1);
            string predicateName = predicateFullName.ToString().Substring(predicateFullName.ToString().LastIndexOf('#') + 1);

            if (!CheckIndividual(subjectName))
                subjectFact = CreateIndividual(subjectName);
            else
                subjectFact = CharacterDataModel.SelectFact(subjectFullName);

            if (!CheckDatatypeProperty(predicateName))
                predicate = CreateDatatypeProperty(predicateName);
            else
                predicate = CharacterOntology.Model.PropertyModel.SelectProperty(predicateFullName);

            objectLiteral = CreateLiteral(value, valuetype);

            CharacterDataModel.AddAssertionRelation(subjectFact, predicate as RDFOntologyDatatypeProperty, objectLiteral);
            SaveCharacter();
        }
        #endregion

        #region Create
        /// <summary>
        /// Creates literal for character given its value and type
        /// </summary>
        /// <param name="value">Value of the literal</param>
        /// <param name="type">Semantic datatype of the literal</param>
        /// <returns></returns>
        internal RDFOntologyLiteral CreateLiteral(string value, string type)
        {
            RDFOntologyData CharacterDataModel = CharacterOntology.Data;
            
            RDFOntologyLiteral CharacterLiteral = new RDFOntologyLiteral(new RDFTypedLiteral(value, CheckDatatypeFromString(type)));
            if (!CheckLiteral(value, type))
                CharacterDataModel.AddLiteral(CharacterLiteral);
            //Console.WriteLine("\nLista de literales pertenecientes al personaje");
            IEnumerator<RDFOntologyLiteral> enumerator = CharacterDataModel.LiteralsEnumerator;
            while (enumerator.MoveNext())
            {
                //Console.WriteLine("Literal: " + enumerator.Current.ToString());
            }
            //Console.ReadLine();
            SaveCharacter();
            return CharacterLiteral;
        }

        /// <summary>
        /// Creates fact for character given its name
        /// </summary>
        /// <param name="name">Name of the fact</param>
        /// <returns></returns>
        internal RDFOntologyFact CreateFact(string name)
        {
            RDFOntologyData GameDataModel = GameOntology.Data;
            RDFOntologyData CharacterDataModel = CharacterOntology.Data;
            RDFOntologyFact GameFact = GameDataModel.SelectFact(CurrentGameContext + name);
            RDFOntologyTaxonomy FactCommentEntries = GameDataModel.Annotations.Comment.SelectEntriesBySubject(GameFact);
            RDFOntologyFact CharacterFact = new RDFOntologyFact(new RDFResource(CurrentCharacterContext + name));
            if (!CheckFact(name))
                CharacterDataModel.AddFact(CharacterFact);

            if (FactCommentEntries.EntriesCount > 0)
            {                
                string FactDescription = FactCommentEntries.SingleOrDefault().TaxonomyObject.ToString();
                if (FactDescription.Contains('^'))
                    FactDescription.Substring(0, FactDescription.IndexOf('^'));

                RDFOntologyLiteral DescriptionLiteral = CreateLiteral(FactDescription, "string");
                CharacterDataModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, CharacterFact, DescriptionLiteral);

            }
            SaveCharacter();
            return CharacterFact;
        }

        /// <summary>
        /// Creates class for character given its name
        /// </summary>
        /// <param name="name">Name of the class</param>
        /// <returns></returns>
        internal RDFOntologyClass CreateClass(string name)
        {
            RDFOntologyClassModel GameClassModel = GameOntology.Model.ClassModel;
            RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;

            RDFOntologyClass CharacterClass;
            name = Text.ToTitleCase(name.Replace(" ", "_"));
            RDFOntologyClass GameClass = GameClassModel.SelectClass(CurrentGameContext + name);

            CharacterClass = new RDFOntologyClass(new RDFResource(CurrentCharacterContext + name));
            if (!CheckClass(name))
                CharacterClassModel.AddClass(CharacterClass);

            RDFOntologyClass CharacterPreviousClass = null;
            var GameSuperClasses = GameClassModel.GetSuperClassesOf(GameClass);
            List<RDFOntologyClass> UpperClasses = GameSuperClasses.ToList<RDFOntologyClass>();
            UpperClasses.Reverse();
            foreach (RDFOntologyClass item in UpperClasses)
            {
                string CharacterUpperClassName = item.Value.ToString().Substring(item.Value.ToString().LastIndexOf('#') + 1);
                RDFOntologyClass CharacterUpperClass = new RDFOntologyClass(new RDFResource(CurrentCharacterContext + CharacterUpperClassName));
                if (!CheckClass(CharacterUpperClassName))
                    CharacterClassModel.AddClass(CharacterUpperClass);
                if (CharacterPreviousClass != null)
                    CharacterClassModel.AddSubClassOfRelation(CharacterUpperClass, CharacterPreviousClass);
                CharacterPreviousClass = CharacterUpperClass;
            }

            if (CharacterPreviousClass != null)
                CharacterClassModel.AddSubClassOfRelation(CharacterClass, CharacterPreviousClass);

            SaveCharacter();
            return CharacterClass;
        }

        /// <summary>
        /// Creates object property for character given its name
        /// </summary>
        /// <param name="name">Name of the object property</param>
        /// <returns></returns>
        internal RDFOntologyObjectProperty CreateObjectProperty(string name)
        {
            RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;
            RDFOntologyPropertyModel CharacterPropertyModel = CharacterOntology.Model.PropertyModel;
            RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;

            RDFOntologyObjectProperty GameObjectProperty = GamePropertyModel.SelectProperty(CurrentGameContext + name) as RDFOntologyObjectProperty;
            RDFOntologyObjectProperty CharacterPreviousObjectProperty = null;

            RDFOntologyObjectProperty CharacterObjectProperty = new RDFOntologyObjectProperty(new RDFResource(CurrentCharacterContext + name));
            if (!CheckObjectProperty(name))
            {
                if (GameObjectProperty.Domain != null)
                {
                    string DomainName;
                    RDFOntologyClass DomainClass;

                    DomainName = GameObjectProperty.Domain.ToString().Substring(GameObjectProperty.Domain.ToString().LastIndexOf('#') + 1);
                    if (!CheckClass(DomainName))
                        DomainClass = CreateClass(DomainName);
                    else
                        DomainClass = CharacterClassModel.SelectClass(CurrentCharacterContext + DomainName);
                    CharacterObjectProperty.SetDomain(DomainClass);
                }

                if (GameObjectProperty.Range != null)
                {
                    string RangeName;
                    RDFOntologyClass RangeClass;

                    RangeName = GameObjectProperty.Range.ToString().Substring(GameObjectProperty.Range.ToString().LastIndexOf('#') + 1);
                    if (!CheckClass(RangeName))
                        RangeClass = CreateClass(RangeName);
                    else
                        RangeClass = CharacterClassModel.SelectClass(CurrentCharacterContext + RangeName);
                    CharacterObjectProperty.SetRange(RangeClass);
                }

                CharacterObjectProperty.SetFunctional(GameObjectProperty.Functional);
                CharacterPropertyModel.AddProperty(CharacterObjectProperty);
            }

            List<RDFOntologyProperty> GameSuperProperties = GamePropertyModel.GetSuperPropertiesOf(GameObjectProperty).ToList();
            GameSuperProperties.Reverse();
            foreach (RDFOntologyProperty item in GameSuperProperties)
            {
                RDFOntologyObjectProperty superproperty = item as RDFOntologyObjectProperty;
                string superpropertyName = superproperty.ToString().Substring(superproperty.ToString().LastIndexOf('#') + 1);
                RDFOntologyObjectProperty CharacterUpperProperty = new RDFOntologyObjectProperty(new RDFResource(CurrentCharacterContext + superpropertyName));

                if (!CheckObjectProperty(superpropertyName))
                    CharacterPropertyModel.AddProperty(CharacterUpperProperty);
                if (CharacterPreviousObjectProperty != null)
                    CharacterPropertyModel.AddSubPropertyOfRelation(CharacterUpperProperty, CharacterPreviousObjectProperty);
                CharacterPreviousObjectProperty = CharacterUpperProperty;
            }

            if (CharacterPreviousObjectProperty != null)
                CharacterPropertyModel.AddSubPropertyOfRelation(CharacterObjectProperty, CharacterPreviousObjectProperty);

            SaveCharacter();
            return CharacterObjectProperty;
        }

        /// <summary>
        /// Creates datatype property for character given its name
        /// </summary>
        /// <param name="name">Name of the datatype property</param>
        /// <returns></returns>
        internal RDFOntologyDatatypeProperty CreateDatatypeProperty(string name)
        {
            RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;
            RDFOntologyPropertyModel CharacterPropertyModel = CharacterOntology.Model.PropertyModel;
            RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;

            RDFOntologyDatatypeProperty GameDatatypeProperty = GamePropertyModel.SelectProperty(CurrentGameContext + name) as RDFOntologyDatatypeProperty;
            RDFOntologyDatatypeProperty CharacterPreviousDatatypeProperty = null;

            List<RDFOntologyProperty> GameSuperProperties = GamePropertyModel.GetSuperPropertiesOf(GameDatatypeProperty).ToList();
            GameSuperProperties.Reverse();
            // Vincular superpropiedades a propiedades

            RDFOntologyDatatypeProperty CharacterDatatypeProperty = new RDFOntologyDatatypeProperty(new RDFResource(CurrentCharacterContext + name));
            if (!CheckDatatypeProperty(name))
            {
                if(GameDatatypeProperty.Domain != null)
                {
                    RDFOntologyClass DomainClass;

                    string DomainName = GameDatatypeProperty.Domain.ToString().Substring(GameDatatypeProperty.Domain.ToString().LastIndexOf('#') + 1);
                    if (!CheckClass(DomainName))
                        DomainClass = CreateClass(DomainName);
                    else
                        DomainClass = CharacterClassModel.SelectClass(CurrentCharacterContext + DomainName);
                    CharacterDatatypeProperty.SetDomain(DomainClass);
                }

                if (GameDatatypeProperty.Range != null)
                {
                    string RangeName = GameDatatypeProperty.Range.ToString().Substring(GameDatatypeProperty.Range.ToString().LastIndexOf('#') + 1);
                    CharacterDatatypeProperty.SetRange(CheckClassFromDatatype(CheckDatatypeFromString(RangeName)));
                }

                CharacterDatatypeProperty.SetFunctional(GameDatatypeProperty.Functional);
                CharacterPropertyModel.AddProperty(CharacterDatatypeProperty);
            }

            foreach (RDFOntologyProperty item in GameSuperProperties)
            {
                RDFOntologyDatatypeProperty superproperty = item as RDFOntologyDatatypeProperty;
                string superpropertyName = superproperty.ToString().Substring(superproperty.ToString().LastIndexOf('#') + 1);
                RDFOntologyDatatypeProperty CharacterUpperProperty = new RDFOntologyDatatypeProperty(new RDFResource(CurrentCharacterContext + superpropertyName));

                if (!CheckDatatypeProperty(superpropertyName))
                    CharacterPropertyModel.AddProperty(CharacterUpperProperty);
                if (CharacterPreviousDatatypeProperty != null)
                    CharacterPropertyModel.AddSubPropertyOfRelation(CharacterUpperProperty, CharacterPreviousDatatypeProperty);
                CharacterPreviousDatatypeProperty = CharacterUpperProperty;
            }

            if (CharacterPreviousDatatypeProperty != null)
                CharacterPropertyModel.AddSubPropertyOfRelation(CharacterDatatypeProperty, CharacterPreviousDatatypeProperty);

            SaveCharacter();
            return CharacterDatatypeProperty;
        }

        /// <summary>
        /// Creates individual for character given its name
        /// </summary>
        /// <param name="name">Name of the individual</param>
        /// <returns></returns>
        internal RDFOntologyFact CreateIndividual(string name)
        {
            RDFOntologyFact CharacterSubject = null;
            name = name.Replace(" ", "_");
            if (CurrentCharacterName.Contains(name))
            {
                if (!CheckFact(name))
                    CharacterSubject = CreateFact(name);

                RDFOntologyClass subjectClass;
                string subjectClassName = "Personaje Jugador";
                if (!CheckClass(subjectClassName))
                    subjectClass = CreateClass(subjectClassName);
                else
                    subjectClass = CharacterOntology.Model.ClassModel.SelectClass(CurrentCharacterContext + subjectClassName);

                CharacterOntology.Data.AddClassTypeRelation(CharacterSubject, subjectClass);
            }
            else
            {
                RDFOntologyClassModel GameClassModel = GameOntology.Model.ClassModel;
                RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;
                RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;
                RDFOntologyPropertyModel CharacterPropertyModel = CharacterOntology.Model.PropertyModel;
                RDFOntologyData GameDataModel = GameOntology.Data;
                RDFOntologyData CharacterDataModel = CharacterOntology.Data;

                // Comprobar que existe la clase del sujeto 
                RDFOntologyFact GameNamedFact = GameDataModel.SelectFact(CurrentGameContext + name);
                RDFOntologyClass CharacterSubjectClass;
                RDFOntologyTaxonomyEntry GameNamedFactClasstype = GameDataModel.Relations.ClassType.FirstOrDefault(entry => entry.TaxonomySubject.Value.Equals(GameNamedFact));
                string FactClassName = GameNamedFactClasstype.TaxonomyObject.ToString().Substring(GameNamedFactClasstype.TaxonomyObject.ToString().LastIndexOf('#') + 1);
                if (!CheckClass(FactClassName))
                    CharacterSubjectClass = CreateClass(FactClassName);
                else
                    CharacterSubjectClass = CharacterClassModel.SelectClass(CurrentCharacterContext + FactClassName);

                // Comprobar si existe el sujeto
                RDFOntologyFact CharacterObject;
                if (!CheckFact(name))
                {
                    CharacterSubject = CreateFact(name);
                    CharacterDataModel.AddClassTypeRelation(CharacterSubject, CharacterSubjectClass);
                }
                else
                {
                    CharacterSubject = CharacterDataModel.SelectFact(CurrentGameContext + name);
                    CharacterDataModel.AddClassTypeRelation(CharacterSubject, CharacterSubjectClass);
                }

                // Para cada propiedad del objeto existente en la BD
                var GameNamedFactAssertions = GameDataModel.Relations.Assertions.SelectEntriesBySubject(GameNamedFact);
                foreach (var assertion in GameNamedFactAssertions)
                {
                    // Comprobar el tipo de predicado
                    string PredicateType = assertion.TaxonomyPredicate.GetType().ToString();
                    if (PredicateType.Contains("RDFOntologyObjectProperty"))
                    {
                        // Si el predicado es una propiedad de objeto
                        RDFOntologyObjectProperty CharacterPredicate;
                        RDFOntologyObjectProperty GamePredicate = assertion.TaxonomyPredicate as RDFOntologyObjectProperty;
                        string PredicateName = GamePredicate.ToString().Substring(GamePredicate.ToString().LastIndexOf('#') + 1);
                        if (!CheckObjectProperty(PredicateName))
                            CharacterPredicate = CreateObjectProperty(PredicateName);
                        else
                            CharacterPredicate = CharacterPropertyModel.SelectProperty(CurrentCharacterContext + PredicateName) as RDFOntologyObjectProperty;

                        // Comprobar que el objeto existe
                        string ObjectName = assertion.TaxonomyObject.Value.ToString().Substring(assertion.TaxonomyObject.Value.ToString().LastIndexOf('#') + 1);
                        if (!CheckFact(ObjectName))
                            CharacterObject = CreateIndividual(ObjectName);
                        else
                            CharacterObject = CharacterDataModel.SelectFact(CurrentCharacterContext + ObjectName);

                        CharacterDataModel.AddAssertionRelation(CharacterSubject, CharacterPredicate, CharacterObject);
                    }
                    else
                    {
                        // Si el predicado es una propiedad de datos
                        RDFOntologyDatatypeProperty CharacterPredicate;
                        RDFOntologyDatatypeProperty GamePredicate = assertion.TaxonomyPredicate as RDFOntologyDatatypeProperty;
                        string PredicateName = GamePredicate.ToString().Substring(GamePredicate.ToString().LastIndexOf('#') + 1);
                        if (!CheckDatatypeProperty(PredicateName))
                            CharacterPredicate = CreateDatatypeProperty(PredicateName);
                        else
                            CharacterPredicate = GamePropertyModel.SelectProperty(CurrentCharacterContext + PredicateName) as RDFOntologyDatatypeProperty;

                        string value = assertion.TaxonomyObject.Value.ToString().Substring(0, assertion.TaxonomyObject.Value.ToString().IndexOf('^'));
                        int typeIndex = assertion.TaxonomyObject.Value.ToString().LastIndexOf('#') + 1;
                        int typeLength = assertion.TaxonomyObject.Value.ToString().Length;
                        string valuetype = assertion.TaxonomyObject.Value.ToString()[typeIndex..typeLength];

                        RDFOntologyLiteral Literal;
                        if (!CheckLiteral(value, valuetype))
                            Literal = CreateLiteral(value, valuetype);
                        else
                            Literal = CharacterDataModel.SelectLiteral(new RDFOntologyLiteral(new RDFTypedLiteral(value, CheckDatatypeFromString(valuetype))).ToString());

                        CharacterDataModel.AddAssertionRelation(CharacterSubject, CharacterPredicate, Literal);
                    }
                }
                RDFOntologyTaxonomy IndividualAssertions = CharacterDataModel.Relations.Assertions;



                //Console.WriteLine("\tIndividual Assertions: ");
                int assertionCounter = 1;
                foreach (var assertion in IndividualAssertions)
                {
                    var assertionSubject = assertion.TaxonomySubject.ToString().Substring(assertion.TaxonomySubject.ToString().LastIndexOf('#') + 1);
                    var assertionPredicate = assertion.TaxonomyPredicate.ToString().Substring(assertion.TaxonomyPredicate.ToString().LastIndexOf('#') + 1);
                    var assertionObject = assertion.TaxonomyObject.ToString().Substring(assertion.TaxonomyObject.ToString().LastIndexOf('#') + 1);

                    //Console.WriteLine(assertionCounter + " -\t" + assertionSubject + "\t" +assertionPredicate +"\t" + assertionObject + ".");
                    ++assertionCounter;
                }
                //Console.ReadLine();
            }
            SaveCharacter();
            return CharacterSubject;
        }

        /// <summary>
        /// Creates new character for the current game selected given its name
        /// </summary>
        /// <param name="name">Name of the character </param>
        internal void CreateCharacter(string name)
        {
            CurrentCharacterName = Text.ToTitleCase(name.Replace(" ", "_"));
            CurrentCharacterFile = CharacterFolder + CurrentCharacterName.Replace("_", " ") + ".owl";
            CurrentCharacterContext = "http://arpegos_project/Games/" + CurrentGameName + "/characters/" + CurrentCharacterName + "#";
            RDFGraph CharacterGraph = new RDFGraph();
            CharacterGraph.SetContext(new Uri(CurrentCharacterContext));
            CharacterOntology = RDFOntology.FromRDFGraph(CharacterGraph);
            if (File.Exists(CurrentCharacterFile))
                File.Delete(CurrentCharacterFile);
            string CurrentCharacterPrefix;
            if (CurrentCharacterName.Contains('_'))
                CurrentCharacterPrefix = CurrentCharacterName.Substring(0, CurrentCharacterName.IndexOf("_"));
            else
                CurrentCharacterPrefix = CurrentCharacterName;
            if (RDFNamespaceRegister.GetByPrefix(CurrentCharacterPrefix) == null)
                RDFNamespaceRegister.AddNamespace(new RDFNamespace(CurrentCharacterPrefix, CurrentCharacterContext));

            CreateIndividual(CurrentCharacterName);
            SaveCharacter();
        }
        #endregion

        #region Check 
        /// <summary>
        /// Check if literal exists inside the current game or character file
        /// </summary>
        /// <param name="value">Value of the literal</param>
        /// <param name="type">Semantic datatype of the literal</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        internal bool CheckLiteral(string value, string type, bool applyOnCharacter = true)
        {
            bool LiteralExists = false;
            RDFTypedLiteral Literal;

            if (applyOnCharacter)
            {
                RDFOntologyData CharacterDataModel = CharacterOntology.Data;
                var LiteralType = CheckDatatypeFromString(type);
                Literal = new RDFTypedLiteral(value, LiteralType);
                if (CharacterDataModel.SelectLiteral(Literal.ToString()) != null)
                    LiteralExists = true;
            }
            else
            {
                RDFOntologyData GameDataModel = GameOntology.Data;
                var LiteralType = CheckDatatypeFromString(type);
                Literal = new RDFTypedLiteral(value, LiteralType);
                if (GameDataModel.SelectLiteral(Literal.ToString()) != null)
                    LiteralExists = true;
            }
            return LiteralExists;
        }

        /// <summary>
        /// Check if fact exists inside the current game or character file
        /// </summary>
        /// <param name="name">Name of the fact</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        internal bool CheckFact(string name, bool applyOnCharacter = true)
        {
            name = name.Replace(" ", "_");
            bool FactExists = false;

            if (applyOnCharacter)
            {
                RDFOntologyData CharacterDataModel = CharacterOntology.Data;
                if (CharacterDataModel.SelectFact(CurrentCharacterContext + name) != null)
                    FactExists = true;
            }
            else
            {
                RDFOntologyData GameDataModel = GameOntology.Data;
                if (GameDataModel.SelectFact(CurrentGameContext + name) != null)
                    FactExists = true;
            }
            return FactExists;
        }

        /// <summary>
        /// Check if class exists inside the current game or character file
        /// </summary>
        /// <param name="name">Name of the class</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        internal bool CheckClass(string name, bool applyOnCharacter = true)
        {
            name = name.Replace(" ", "_");
            bool ClassExists = false;
            if (applyOnCharacter)
            {
                RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;
                if (CharacterClassModel.SelectClass(CurrentCharacterContext + name) != null)
                    ClassExists = true;
            }
            else
            {
                RDFOntologyClassModel GameClassModel = GameOntology.Model.ClassModel;
                if (GameClassModel.SelectClass(CurrentGameContext + name) != null)
                    ClassExists = true;
            }
            return ClassExists;
        }

        /// <summary>
        /// Check if object property exists inside the current game or character file
        /// </summary>
        /// <param name="name">Name of the object property</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        internal bool CheckObjectProperty(string name, bool applyOnCharacter = true)
        {
            name = name.Replace(" ", "_");
            bool PropertyExists = false;

            if (applyOnCharacter)
            {
                RDFOntologyPropertyModel CharacterPropertyModel = CharacterOntology.Model.PropertyModel;
                RDFOntologyProperty CharacterProperty = CharacterPropertyModel.SelectProperty(CurrentCharacterContext + name);
                if (CharacterProperty != null)
                {
                    IEnumerator<RDFOntologyObjectProperty> ObjectEnumerator = CharacterPropertyModel.ObjectPropertiesEnumerator;
                    while (PropertyExists == false && ObjectEnumerator.MoveNext())
                    {
                        RDFOntologyObjectProperty property = ObjectEnumerator.Current;
                        string currentProperty = property.ToString().Substring(property.ToString().LastIndexOf('#') + 1);
                        if (currentProperty == name)
                            PropertyExists = true;
                    }
                }
            }
            else
            {
                RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;
                RDFOntologyProperty GameProperty = GamePropertyModel.SelectProperty(CurrentGameContext + name);
                if (GameProperty != null)
                {
                    IEnumerator<RDFOntologyObjectProperty> ObjectEnumerator = GamePropertyModel.ObjectPropertiesEnumerator;
                    while (PropertyExists == false && ObjectEnumerator.MoveNext())
                    {
                        RDFOntologyObjectProperty property = ObjectEnumerator.Current;
                        string currentProperty = property.ToString().Substring(property.ToString().LastIndexOf('#') + 1);
                        if (currentProperty == name)
                            PropertyExists = true;
                    }
                }
            }
            return PropertyExists;
        }

        /// <summary>
        /// Check if datatype property exists inside the current game or character file
        /// </summary>
        /// <param name="name">Name of the datatype property</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        internal bool CheckDatatypeProperty(string name, bool applyOnCharacter = true)
        {
            name = name.Replace(" ", "_");
            bool PropertyExists = false;

            if (applyOnCharacter)
            {
                RDFOntologyPropertyModel CharacterPropertyModel = CharacterOntology.Model.PropertyModel;
                RDFOntologyProperty CharacterProperty = CharacterPropertyModel.SelectProperty(CurrentCharacterContext + name);
                if (CharacterProperty != null)
                {
                    IEnumerator<RDFOntologyDatatypeProperty> DatatypeEnumerator = CharacterPropertyModel.DatatypePropertiesEnumerator;
                    while (PropertyExists == false && DatatypeEnumerator.MoveNext())
                    {
                        RDFOntologyDatatypeProperty property = DatatypeEnumerator.Current;
                        string currentProperty = property.ToString().Substring(property.ToString().LastIndexOf('#') + 1);
                        if (currentProperty == name)
                            PropertyExists = true;
                    }
                    
                }
            }
            else
            {
                RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;
                RDFOntologyProperty GameProperty = GamePropertyModel.SelectProperty(CurrentGameContext + name);
                if (GameProperty != null)
                {
                    IEnumerator<RDFOntologyDatatypeProperty> DatatypeEnumerator = GamePropertyModel.DatatypePropertiesEnumerator;
                    while (PropertyExists == false && DatatypeEnumerator.MoveNext())
                    {
                        RDFOntologyDatatypeProperty property = DatatypeEnumerator.Current;
                        string currentProperty = property.ToString().Substring(property.ToString().LastIndexOf('#') + 1);
                        if (currentProperty == name)
                            PropertyExists = true;
                    }
                }
            }
            return PropertyExists;
        }

        /// <summary>
        /// Check if individual exists inside the current game or character file
        /// </summary>
        /// <param name="name">Name of the individual</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        internal bool CheckIndividual(string name, bool applyOnCharacter = true)
        {
            return CheckFact(name, applyOnCharacter);
        }

        /// <summary>
        /// Returns the general cost associated to the given stage
        /// </summary>
        /// <param name="stage">Name of the stage</param>
        /// <returns></returns>
        internal string CheckGeneralCost(string stage)
        {
            string GeneralCostPredicateName = "";
            List<string> StageWords = stage.Split('_').ToList();
            int FilterCounter = 2;
            int wordCounter = StageWords.Count();
            while (FilterCounter > 1)
            {
                if (wordCounter > 0)
                {
                    string SubjectFactName = "Def_";
                    for (int i = 0; i < wordCounter - 1; ++i)
                        SubjectFactName += StageWords.ElementAtOrDefault(i) + "_";
                    SubjectFactName += StageWords.LastOrDefault();

                    RDFOntologyFact SubjectFact = GameOntology.Data.SelectFact(CurrentGameContext + SubjectFactName);
                    if (SubjectFact != null)
                    {
                        IEnumerable<RDFOntologyTaxonomyEntry> SubjectFactCostAnnotations = GameOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(SubjectFact);
                        SubjectFactCostAnnotations = SubjectFactCostAnnotations.Where(entry => entry.ToString().Contains("GeneralCostDefinedBy"));
                        if (SubjectFactCostAnnotations.Count() == 1)
                        {
                            FilterCounter = SubjectFactCostAnnotations.Count();
                            GeneralCostPredicateName = SubjectFactCostAnnotations.SingleOrDefault().TaxonomyObject.ToString();
                            if(GeneralCostPredicateName.Contains('^'))
                                GeneralCostPredicateName = GeneralCostPredicateName.Substring(0, GeneralCostPredicateName.IndexOf('^'));
                        }
                    }
                    --wordCounter;
                }
                else
                    FilterCounter = 0;
            }

            if (FilterCounter != 1)
            {
                string parents = GetParentClasses(stage);
                if(parents != null)
                {
                    bool GeneralCostFound = false;
                    List<string> parentList = parents.Split(':').ToList();
                    foreach(string parent in parentList)
                    {
                        if(GeneralCostFound == false)
                        {
                            GeneralCostPredicateName = CheckGeneralCost(parent);
                            if (GeneralCostPredicateName != null)
                                GeneralCostFound = true;
                        }
                    }
                }
            }

            return GeneralCostPredicateName;
        }

        /// <summary>
        /// Returns true if an element belongs to Equipment given its class
        /// </summary>
        /// <param name="elementClass"></param>
        /// <returns></returns>
        internal bool CheckEquipmentClass (string elementClassName)
        {
            bool isEquipment = false;
            List<string> EquipmentWords = new List<string> { "Equipamiento", "Equipment", "Équipement" };
            RDFOntologyClass ElementClass = GameOntology.Model.ClassModel.SelectClass(CurrentGameContext + elementClassName);
            RDFOntologyTaxonomyEntry ElementClassTypeEntry = GameOntology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(ElementClass).SingleOrDefault();
            if(ElementClassTypeEntry != null)
            {
                string elementSuperClass = ElementClassTypeEntry.TaxonomyObject.ToString().Substring(ElementClassTypeEntry.TaxonomyObject.ToString().LastIndexOf('#') + 1);
                if (EquipmentWords.Any(word => elementSuperClass.Contains(word)))
                    isEquipment = true;
                else
                    isEquipment = CheckEquipmentClass(elementSuperClass);
            }

            return isEquipment;
        }

        /// <summary>
        /// Returns the description of a valued list view given the stage
        /// </summary>
        /// <param name="stage">Name of the stage stage</param>
        /// <returns></returns>
        internal string CheckValueListInfo(string stage)
        {
            string Info = null;

            string StageDefinitionName = "Def_" + stage;
            RDFOntologyFact StageDefinitionIndividual = GameOntology.Data.SelectFact(CurrentGameContext + StageDefinitionName);
            RDFOntologyTaxonomy StageAnnotations = GameOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(StageDefinitionIndividual);
            RDFOntologyTaxonomyEntry StageDefinitionAnnotation = StageAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("ValuedListInfo")).SingleOrDefault();
            if(StageDefinitionAnnotation != null)
            {
                Info = StageDefinitionAnnotation.TaxonomyObject.ToString();
                if(Info.Contains('^'))
                    Info = Info.Substring(0, Info.IndexOf('^'));
            }
            else
            {
                string parents = GetParentClasses(stage);
                if (parents != null)
                {
                    bool infoFound = false;
                    List<string> parentList = parents.Split(':').ToList();
                    foreach (string parent in parentList)
                    {
                        if (infoFound == false)
                        {
                            Info = CheckValueListInfo(parent);
                            infoFound = Info != null;
                        }
                    }
                }
            }

            return Info;
        }

        /// <summary>
        /// Returns a list of elements available given the current stage, true if the stage has a general limit, the general limit and the partial limit
        /// </summary>
        /// <param name="stage">Name of the stage</param>
        /// <param name="hasGeneralLimitValue">Boolean which tells if the stage has general limit</param>
        /// <param name="GeneralLimitValue">Value of the general limit</param>
        /// <param name="PartialLimitValue">Value of the partial limit</param>
        /// <returns></returns>
        internal List<string> CheckAvailableOptions(string stage, bool hasGeneralLimitValue, float? GeneralLimitValue, float? PartialLimitValue)
        {
            List<string> CostWords = new List<string> { "Coste", "Cost", "Coût" };
            List<string> AvailableOptions = new List<string>();
            Group StageGroup = new Group(stage);
            foreach(Item item in StageGroup.GroupList)
            {
                if(hasGeneralLimitValue == false || GeneralLimitValue > 0)
                {
                    if(PartialLimitValue > 0)
                    {
                        List<string> RequirementWords = new List<string> {"Requisito","Requirement","Requisite","Prérequis" };
                        RDFOntologyFact ItemFact = GameOntology.Data.SelectFact(CurrentGameContext + item.Name);
                        RDFOntologyTaxonomy ItemFactAssertions = GameOntology.Data.Relations.Assertions.SelectEntriesBySubject(ItemFact);

                        //Check it has requisites
                        IEnumerable<RDFOntologyTaxonomyEntry> ItemRequirements = ItemFactAssertions.Where(entry => RequirementWords.Any(word => entry.ToString().Contains(word)));
                        string  DatatypeRequirementName;
                        bool AllRequirementsFulfilled = true;
                        List<string> RequirementsChecked = new List<string>();
                        foreach (RDFOntologyTaxonomyEntry entry in ItemRequirements)
                        {
                            if(AllRequirementsFulfilled == true)
                            {
                                List<string> ObjectRequirementNameList = new List<string>();
                                Dictionary<string, bool> ObjectRequirementNameDictionary = new Dictionary<string, bool>();
                                DatatypeRequirementName = entry.TaxonomyPredicate.ToString().Substring(entry.TaxonomyPredicate.ToString().LastIndexOf('#') + 1);                                
                                if (RequirementsChecked.Any(req => req == DatatypeRequirementName))
                                    continue;
                                RequirementsChecked.Add(DatatypeRequirementName);
                                bool isDatatype;

                                if (isDatatype = CheckDatatypeProperty(DatatypeRequirementName, false))
                                {
                                    string requirementNumber = DatatypeRequirementName.Split('_').ToList().LastOrDefault();
                                    IEnumerable<RDFOntologyTaxonomyEntry> ItemObjectRequirements = ItemFactAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains(DatatypeRequirementName) == false);
                                    ItemObjectRequirements = ItemObjectRequirements.Where(entry => entry.ToString().Contains("_" + requirementNumber));
                                    foreach (RDFOntologyTaxonomyEntry requirementEntry in ItemObjectRequirements)
                                    {
                                        string predicate = requirementEntry.TaxonomyPredicate.ToString().Substring(requirementEntry.TaxonomyPredicate.ToString().LastIndexOf('#') + 1);
                                        string name = requirementEntry.TaxonomyObject.ToString().Substring(requirementEntry.TaxonomyObject.ToString().LastIndexOf('#') + 1);
                                        ObjectRequirementNameList.Add(name);
                                        RequirementsChecked.Add(predicate);
                                    }
                                }
                                else
                                    ObjectRequirementNameList.Add(entry.TaxonomyObject.ToString().Substring(entry.TaxonomyObject.ToString().LastIndexOf('#') + 1));

                                foreach(string name in ObjectRequirementNameList)
                                {
                                    string objectFactName = name;
                                    RDFOntologyFact objectFact = null;
                                    if (!CheckIndividual(objectFactName))
                                    {
                                        AllRequirementsFulfilled = false;
                                        continue;
                                    }
                                    else
                                        objectFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + objectFactName);

                                    RDFOntologyFact CharacterFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);
                                    RDFOntologyTaxonomy CharacterAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                                    RDFOntologyTaxonomy CharacterRequirementAssertions = CharacterAssertions.SelectEntriesByObject(objectFact);
                                    if (CharacterRequirementAssertions.Count() > 0)
                                    {
                                        if (isDatatype)
                                        {
                                            RDFOntologyTaxonomyEntry RequirementAssertion = CharacterAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains(objectFactName)).SingleOrDefault();
                                            float RequirementValue = Convert.ToSingle(entry.TaxonomyObject.ToString().Substring(0, entry.TaxonomyObject.ToString().IndexOf('^')));
                                            float CharacterValue = Convert.ToSingle(RequirementAssertion.TaxonomyObject.ToString().Substring(0, RequirementAssertion.TaxonomyObject.ToString().IndexOf('^')));
                                            dynamic result = ConvertToOperator("<", CharacterValue, RequirementValue);
                                            if (result == true)
                                                ObjectRequirementNameDictionary.Add(name, false);
                                            else
                                                ObjectRequirementNameDictionary.Add(name, true);
                                        }
                                    }
                                    else
                                    {
                                        IEnumerable<RDFOntologyTaxonomyEntry> RequirementAssertions = CharacterAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains(objectFactName));
                                        if (RequirementAssertions.Count() > 0)
                                        {
                                            if (RequirementAssertions.Count() > 1)
                                                RequirementAssertions = RequirementAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains("Total"));

                                            if (isDatatype)
                                            {
                                                RDFOntologyTaxonomyEntry RequirementAssertion = RequirementAssertions.SingleOrDefault();
                                                float RequirementValue = Convert.ToSingle(entry.TaxonomyObject.ToString().Substring(0, entry.TaxonomyObject.ToString().IndexOf('^')));
                                                float CharacterValue = Convert.ToSingle(RequirementAssertion.TaxonomyObject.ToString().Substring(0, RequirementAssertion.TaxonomyObject.ToString().IndexOf('^')));
                                                dynamic result = ConvertToOperator("<", CharacterValue, RequirementValue);
                                                if (result == true)
                                                    ObjectRequirementNameDictionary.Add(name, false);
                                                else
                                                    ObjectRequirementNameDictionary.Add(name, true);
                                            }
                                        }
                                        else
                                            AllRequirementsFulfilled = false;
                                    }
                                }
                                if (ObjectRequirementNameDictionary.Values.All(value => value == false))
                                    AllRequirementsFulfilled = false;
                            }
                        }

                        if(AllRequirementsFulfilled == true)
                        {
                            //Check it has costs
                            IEnumerable<RDFOntologyTaxonomyEntry> ItemCosts = ItemFactAssertions.Where(entry => CostWords.Any(word => entry.ToString().Contains(word)));
                            RDFOntologyTaxonomyEntry ItemCostEntry = null;

                            if (ItemCosts.Count() > 1)
                            {
                                string GeneralCostPredicateName = CheckGeneralCost(stage);
                                ItemCosts = ItemCosts.Where(entry => entry.TaxonomyPredicate.ToString().Contains(GeneralCostPredicateName));
                                ItemCostEntry = ItemCosts.SingleOrDefault();
                            }
                            else if(ItemCosts.Count() == 1)
                                ItemCostEntry = ItemCosts.SingleOrDefault();

                            if (ItemCostEntry != null)
                            {
                                float CostValue = Convert.ToSingle(ItemCostEntry.TaxonomyObject.ToString().Substring(0, ItemCostEntry.TaxonomyObject.ToString().IndexOf('^')));
                                string ItemCostEntryPredicate = ItemCostEntry.TaxonomyPredicate.ToString().Substring(ItemCostEntry.TaxonomyPredicate.ToString().LastIndexOf('#')+1);
                                string GeneralLimitName = GetLimitByValue(stage, GeneralLimitValue.ToString());
                                string firstWord = GeneralLimitName.Split('_').ToList().FirstOrDefault() + "_";
                                GeneralLimitName = GeneralLimitName.Replace("Per_", "");

                                bool CostMatchGeneralLimit = CheckCostAndLimit(ItemCostEntryPredicate, GeneralLimitName);
                                if (CostMatchGeneralLimit == false)
                                {
                                    string PartialLimitName = GetLimitByValue(stage, PartialLimitValue.ToString());
                                    bool CostMatchPartialLimit = CheckCostAndLimit(ItemCostEntryPredicate, PartialLimitName);
                                    if (ItemCostEntryPredicate != PartialLimitName)
                                    {
                                        // Buscar límite K
                                        string RequirementCostName = "";
                                        List<string> ItemCostWords = ItemCostEntryPredicate.Split('_').ToList();
                                        List<string> CharacterClass = new List<string> { "Personaje", "Character", "Personnage" };
                                        bool CharacterDatatatypePropertyFound = false;
                                        int index = 0;
                                        ItemCostWords.Remove(ItemCostWords.FirstOrDefault());

                                        while (CharacterDatatatypePropertyFound == false)
                                        {
                                            RDFOntologyProperty Property = this.CharacterOntology.Model.PropertyModel.ElementAtOrDefault(index);
                                            if (this.CheckDatatypeProperty(Property.ToString().Substring(Property.ToString().LastIndexOf('#') + 1)))
                                            {
                                                RDFOntologyDatatypeProperty DatatypeProperty = Property as RDFOntologyDatatypeProperty;
                                                if (DatatypeProperty.Domain != null)
                                                {
                                                    string DomainClass = DatatypeProperty.Domain.ToString().Substring(DatatypeProperty.Domain.ToString().LastIndexOf('#') + 1);
                                                    if (CharacterClass.Any(word => DomainClass.Contains(word)))
                                                    {
                                                        if(!CharacterClass.Any(word => DatatypeProperty.ToString().Contains(word)))
                                                        {
                                                            string DatatypePropertyFirstWord = DatatypeProperty.ToString().Substring(DatatypeProperty.ToString().LastIndexOf('#') + 1).Split('_').ToList().FirstOrDefault();
                                                            ItemCostWords.Insert(0, DatatypePropertyFirstWord);
                                                            CharacterDatatatypePropertyFound = true;
                                                        }                                                        
                                                    }                                                        
                                                }
                                            }
                                            ++index;
                                        }

                                        if(CostWords.Any(word => ItemCostEntryPredicate.Contains(word)))
                                        {
                                            ItemCostWords.Remove(ItemCostWords.LastOrDefault());
                                            ItemCostWords.Add("Total");
                                        }

                                        for (int i = 0; i < ItemCostWords.Count() - 1; ++i)
                                            RequirementCostName += ItemCostWords.ElementAtOrDefault(i) + "_";
                                        RequirementCostName += ItemCostWords.LastOrDefault();

                                        RDFOntologyFact CharacterFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);

                                        if (!(CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + RequirementCostName) is RDFOntologyDatatypeProperty RequirementCostProperty))
                                        {
                                            IEnumerable<RDFOntologyTaxonomyEntry> RequirementCostAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                                            RequirementCostAssertions = RequirementCostAssertions.Where(entry => ItemCostWords.All(word => entry.TaxonomyPredicate.ToString().Contains(word)));
                                            RequirementCostName = RequirementCostAssertions.SingleOrDefault().TaxonomyPredicate.ToString().Substring(RequirementCostAssertions.SingleOrDefault().TaxonomyPredicate.ToString().LastIndexOf('#') + 1);
                                            RequirementCostProperty = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + RequirementCostName) as RDFOntologyDatatypeProperty;
                                        }

                                        RDFOntologyTaxonomyEntry entry = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact).SelectEntriesByPredicate(RequirementCostProperty).SingleOrDefault();
                                        float entryValue = Convert.ToSingle(entry.TaxonomyObject.ToString().Substring(0, entry.TaxonomyObject.ToString().IndexOf('^')));
                                        if (entryValue >= CostValue)
                                            AvailableOptions.Add(item.Name);
                                    }
                                    else
                                    {
                                        if (GeneralLimitValue >= CostValue)
                                            if (PartialLimitValue >= CostValue)
                                                AvailableOptions.Add(item.Name);
                                    }
                                }
                                else
                                {
                                    if (GeneralLimitValue >= CostValue)
                                        AvailableOptions.Add(item.Name);
                                 }                              
                            }
                        }                        
                    }
                }
            }
            return AvailableOptions;
        }

        /// <summary>
        /// Returns true if the given cost is related to the given limit
        /// </summary>
        /// <param name="cost">Name of the cost</param>
        /// <param name="limit">Name of the limit</param>
        /// <returns></returns>
        internal bool CheckCostAndLimit(string cost, string limit)
        {
            bool CostMatchLimit = true;
            List<string> IgnorableWords = new List<string> { "Coste", "Cost", "Coût", "Total"};
            
            List<string> CostWords = cost.Split('_').ToList();
            List<string> LimitWords = limit.Split('_').ToList();

            int comparableWords = Math.Min(CostWords.Count(), LimitWords.Count());
            if(CostWords.Count() == comparableWords)
            {
                if (IgnorableWords.Any(word => cost.Contains(word)))
                    --comparableWords;
            }
            else
            {
                if (IgnorableWords.Any(word => limit.Contains(word)))
                    --comparableWords;
            }

            if (comparableWords > 0)
                CostMatchLimit = true;
            else
                CostMatchLimit = false;

            for (int index = 0; index < comparableWords; ++index)
            {
                string ItemCostWord = CostWords.ElementAtOrDefault(index);
                string GeneralLimitWord = LimitWords.ElementAtOrDefault(index);
                int LowerLength = Math.Min(GeneralLimitWord.Length, ItemCostWord.Length);
                if (ItemCostWord.Length > LowerLength)
                    ItemCostWord = ItemCostWord.Substring(0, LowerLength);
                if (GeneralLimitWord.Length > LowerLength)
                    GeneralLimitWord = GeneralLimitWord.Substring(0, LowerLength);
                if (ItemCostWord != GeneralLimitWord)
                    CostMatchLimit = false;
            }
            return CostMatchLimit;
        }

        /// <summary>
        /// Returns semantic datatype given its name
        /// </summary>
        /// <param name="type">Name of the semantic datatype</param>
        /// <returns></returns>
        internal RDFModelEnums.RDFDatatypes CheckDatatypeFromString(string type)
        {
            var ReturnType = type switch
            {
                "XMLLiteral" => RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL,
                "string" => RDFModelEnums.RDFDatatypes.XSD_STRING,
                "boolean" => RDFModelEnums.RDFDatatypes.XSD_BOOLEAN,
                "decimal" => RDFModelEnums.RDFDatatypes.XSD_DECIMAL,
                "float" => RDFModelEnums.RDFDatatypes.XSD_FLOAT,
                "double" => RDFModelEnums.RDFDatatypes.XSD_DOUBLE,
                "positiveInteger" => RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER,
                "negativeInteger" => RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER,
                "nonPositiveInteger" => RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER,
                "nonNegativeInteger" => RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER,
                "integer" => RDFModelEnums.RDFDatatypes.XSD_INTEGER,
                "long" => RDFModelEnums.RDFDatatypes.XSD_LONG,
                "int" => RDFModelEnums.RDFDatatypes.XSD_INT,
                "short" => RDFModelEnums.RDFDatatypes.XSD_SHORT,
                "byte" => RDFModelEnums.RDFDatatypes.XSD_BYTE,
                "unsignedLong" => RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG,
                "unsignedShort" => RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT,
                "unsignedByte" => RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE,
                "unsignedInt" => RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT,
                "duration" => RDFModelEnums.RDFDatatypes.XSD_DURATION,
                "dateTime" => RDFModelEnums.RDFDatatypes.XSD_DATETIME,
                "date" => RDFModelEnums.RDFDatatypes.XSD_DATE,
                "time" => RDFModelEnums.RDFDatatypes.XSD_TIME,
                "gYear" => RDFModelEnums.RDFDatatypes.XSD_GYEAR,
                "gMonth" => RDFModelEnums.RDFDatatypes.XSD_GMONTH,
                "gDay" => RDFModelEnums.RDFDatatypes.XSD_GDAY,
                "gYearMonth" => RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH,
                "gMonthDay" => RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY,
                "hexBinary" => RDFModelEnums.RDFDatatypes.XSD_HEXBINARY,
                "base64Binary" => RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY,
                "anyURI" => RDFModelEnums.RDFDatatypes.XSD_ANYURI,
                "QName" => RDFModelEnums.RDFDatatypes.XSD_QNAME,
                "notation" => RDFModelEnums.RDFDatatypes.XSD_NOTATION,
                "language" => RDFModelEnums.RDFDatatypes.XSD_LANGUAGE,
                "normalizedString" => RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING,
                "token" => RDFModelEnums.RDFDatatypes.XSD_TOKEN,
                "NMToken" => RDFModelEnums.RDFDatatypes.XSD_NMTOKEN,
                "name" => RDFModelEnums.RDFDatatypes.XSD_NAME,
                "NCName" => RDFModelEnums.RDFDatatypes.XSD_NCNAME,
                "ID" => RDFModelEnums.RDFDatatypes.XSD_ID,
                _ => RDFModelEnums.RDFDatatypes.RDFS_LITERAL,
            };
            return ReturnType;
        }

        /// <summary>
        /// Return class given a semantic datatype
        /// </summary>
        /// <param name="datatype">Semantic datatype</param>
        /// <returns></returns>
        internal RDFOntologyClass CheckClassFromDatatype(RDFModelEnums.RDFDatatypes datatype)
        {
            if (datatype.Equals(RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL))
                return new RDFOntologyClass(RDFVocabulary.RDF.XML_LITERAL);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.RDFS_LITERAL))
                return new RDFOntologyClass(RDFVocabulary.RDFS.LITERAL);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING))
                return new RDFOntologyClass(RDFVocabulary.XSD.STRING);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_ANYURI))
                return new RDFOntologyClass(RDFVocabulary.XSD.ANY_URI);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY))
                return new RDFOntologyClass(RDFVocabulary.XSD.BASE64_BINARY);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_BOOLEAN))
                return new RDFOntologyClass(RDFVocabulary.XSD.BOOLEAN);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_BYTE))
                return new RDFOntologyClass(RDFVocabulary.XSD.BYTE);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DATE))
                return new RDFOntologyClass(RDFVocabulary.XSD.DATE);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DATETIME))
                return new RDFOntologyClass(RDFVocabulary.XSD.DATETIME);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DECIMAL))
                return new RDFOntologyClass(RDFVocabulary.XSD.DECIMAL);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DOUBLE))
                return new RDFOntologyClass(RDFVocabulary.XSD.DOUBLE);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DURATION))
                return new RDFOntologyClass(RDFVocabulary.XSD.DURATION);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_FLOAT))
                return new RDFOntologyClass(RDFVocabulary.XSD.FLOAT);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GDAY))
                return new RDFOntologyClass(RDFVocabulary.XSD.G_DAY);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GMONTH))
                return new RDFOntologyClass(RDFVocabulary.XSD.G_MONTH);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY))
                return new RDFOntologyClass(RDFVocabulary.XSD.G_MONTH_DAY);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GYEAR))
                return new RDFOntologyClass(RDFVocabulary.XSD.G_YEAR);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH))
                return new RDFOntologyClass(RDFVocabulary.XSD.G_YEAR_MONTH);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_HEXBINARY))
                return new RDFOntologyClass(RDFVocabulary.XSD.HEX_BINARY);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INT))
                return new RDFOntologyClass(RDFVocabulary.XSD.INT);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                return new RDFOntologyClass(RDFVocabulary.XSD.INTEGER);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_LANGUAGE))
                return new RDFOntologyClass(RDFVocabulary.XSD.LANGUAGE);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_LONG))
                return new RDFOntologyClass(RDFVocabulary.XSD.LONG);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NAME))
                return new RDFOntologyClass(RDFVocabulary.XSD.NAME);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NCNAME))
                return new RDFOntologyClass(RDFVocabulary.XSD.NCNAME);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_ID))
                return new RDFOntologyClass(RDFVocabulary.XSD.ID);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER))
                return new RDFOntologyClass(RDFVocabulary.XSD.NEGATIVE_INTEGER);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NMTOKEN))
                return new RDFOntologyClass(RDFVocabulary.XSD.NMTOKEN);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER))
                return new RDFOntologyClass(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER))
                return new RDFOntologyClass(RDFVocabulary.XSD.NON_POSITIVE_INTEGER);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING))
                return new RDFOntologyClass(RDFVocabulary.XSD.NORMALIZED_STRING);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NOTATION))
                return new RDFOntologyClass(RDFVocabulary.XSD.NOTATION);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER))
                return new RDFOntologyClass(RDFVocabulary.XSD.POSITIVE_INTEGER);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_QNAME))
                return new RDFOntologyClass(RDFVocabulary.XSD.QNAME);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_SHORT))
                return new RDFOntologyClass(RDFVocabulary.XSD.SHORT);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_TIME))
                return new RDFOntologyClass(RDFVocabulary.XSD.TIME);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_TOKEN))
                return new RDFOntologyClass(RDFVocabulary.XSD.TOKEN);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE))
                return new RDFOntologyClass(RDFVocabulary.XSD.UNSIGNED_BYTE);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT))
                return new RDFOntologyClass(RDFVocabulary.XSD.UNSIGNED_INT);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG))
                return new RDFOntologyClass(RDFVocabulary.XSD.UNSIGNED_LONG);
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT))
                return new RDFOntologyClass(RDFVocabulary.XSD.UNSIGNED_SHORT);
            else
                //Unknown datatypes default to instances of "rdfs:Literal"
                return new RDFOntologyClass(RDFVocabulary.RDFS.LITERAL);
        }
        #endregion

        #region Get

        /// <summary>
        /// Returns class of the element
        /// </summary>
        /// <param name="elementName">Name of the element</param>
        /// <returns></returns>
        internal string GetElementClass(string elementName)
        {
            RDFOntologyData GameDataModel = GameOntology.Data;
            RDFOntologyTaxonomyEntry elementClassType = GameDataModel.Relations.ClassType.FirstOrDefault(entry => entry.TaxonomySubject.ToString().Contains(elementName));
            string elementClass = elementClassType.TaxonomyObject.ToString().Substring(elementClassType.TaxonomyObject.ToString().LastIndexOf('#') + 1);
            return elementClass;
        }

        /// <summary>
        /// Returns description of the element
        /// </summary>
        /// <param name="elementName">Name of the element</param>
        /// <returns></returns>
        internal string GetElementDescription(string elementName)
        {
            string elementDesc = null;
            RDFOntologyData GameDataModel = GameOntology.Data;
            RDFOntologyTaxonomyEntry elementCommentAnnotation = GameDataModel.Annotations.Comment.FirstOrDefault(entry => entry.TaxonomySubject.ToString().Contains(elementName));
            if (elementCommentAnnotation != null)
                elementDesc = elementCommentAnnotation.TaxonomyObject.ToString().Replace("^^http://www.w3.org/2001/XMLSchema#string", "");
            return elementDesc;
        }

        /// <summary>
        /// Returns a list of individuals given the name of their class
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        internal ObservableCollection<Item> GetIndividuals(string className, bool applyOnCharacter = false)
        {
            ObservableCollection<Item> individuals = new ObservableCollection<Item>();
            if (applyOnCharacter)
            {
                RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;
                RDFOntologyData GameDataModel = CharacterOntology.Data;
                RDFOntologyClass currentClass = CharacterClassModel.SelectClass(CurrentGameContext + className);
                RDFOntologyTaxonomy classAssertions = GameDataModel.Relations.ClassType.SelectEntriesByObject(currentClass);
                foreach (RDFOntologyTaxonomyEntry assertion in classAssertions)
                {
                    RDFOntologyResource individual = assertion.TaxonomySubject;
                    string individualName = individual.ToString().Substring(individual.ToString().LastIndexOf('#') + 1);
                    individuals.Add(new Item(individualName));
                }
            }
            else
            {
                RDFOntologyClassModel GameClassModel = GameOntology.Model.ClassModel;
                RDFOntologyData GameDataModel = GameOntology.Data;

                RDFOntologyClass currentClass = GameClassModel.SelectClass(CurrentGameContext + className);
                RDFOntologyTaxonomy classAssertions = GameDataModel.Relations.ClassType.SelectEntriesByObject(currentClass);
                foreach (RDFOntologyTaxonomyEntry assertion in classAssertions)
                {
                    RDFOntologyResource individual = assertion.TaxonomySubject;
                    string individualName = individual.ToString().Substring(individual.ToString().LastIndexOf('#') + 1);
                    individuals.Add(new Item(individualName));
                }
            }
            return individuals;
        }

        /// <summary>
        /// Returns a list of groups of individuals given the name of the root class
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        internal dynamic GetIndividualsGrouped(string className, bool applyOnCharacter = false)
        {
            dynamic groups;
            ObservableCollection<Group> subclasses = GetSubClasses(className, applyOnCharacter);
            if (subclasses != null)
            {
                groups = subclasses;
                foreach (Group groupItem in groups)
                    groupItem.GroupList = GetIndividualsGrouped(groupItem.Title, applyOnCharacter);
            }
            else
            {
                groups = GetIndividuals(className, applyOnCharacter);
            }

            return groups;
        }

        internal string GetCreationSchemeRootClass()
        {
            string creationSchemeRootUri  = GameOntology.Data.Annotations.CustomAnnotations.SelectEntriesByPredicate(new RDFOntologyAnnotationProperty(new RDFResource(CurrentGameContext + "CreationSchemeRoot"))).Single().TaxonomySubject.ToString();
            return creationSchemeRootUri.Substring(creationSchemeRootUri.LastIndexOf('#') + 1).Replace("Def_", "");
        }
        /// <summary>
        /// Returns the class of the given fact
        /// </summary>
        /// <param name="factName">Name of the fact</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        internal string GetClass(string factName, bool applyOnCharacter = false)
        {
            string resultFact;
            if(applyOnCharacter)
            {
                RDFOntologyData CharacterDataModel = CharacterOntology.Data;
                RDFOntologyFact SubjectFact = CharacterDataModel.SelectFact(CurrentCharacterContext + factName);

                RDFOntologyTaxonomyEntry SubjectFactEntry = CharacterDataModel.Relations.ClassType.SelectEntriesBySubject(SubjectFact).SingleOrDefault();
                resultFact = SubjectFactEntry.TaxonomyObject.ToString().Substring(SubjectFactEntry.TaxonomyObject.ToString().LastIndexOf('#')+1);
            }
            else
            {
                RDFOntologyData GameDataModel = GameOntology.Data;
                RDFOntologyFact SubjectFact = GameDataModel.SelectFact(CurrentGameContext + factName);

                RDFOntologyTaxonomy SubjectFactClassTypeAssertions = GameDataModel.Relations.ClassType.SelectEntriesBySubject(SubjectFact);
                RDFOntologyTaxonomyEntry SubjectFactEntry = SubjectFactClassTypeAssertions.SingleOrDefault();
                resultFact = SubjectFactEntry.TaxonomyObject.ToString().Substring(SubjectFactEntry.TaxonomyObject.ToString().LastIndexOf('#') + 1);
            }
            return resultFact;
        }

        /// <summary>
        /// Returns true if the given element has available points
        /// </summary>
        /// <param name="ElementName">Name of the element</param>
        /// <param name="AvailablePoints">Available points obtained</param>
        /// <returns></returns>
        internal string GetAvailablePoints(string ElementName, out float? AvailablePoints)
        {
            AvailablePoints = null;
            string LimitPropertyName = null;
            List<string> AvailableWords = new List<string>()
            {
                "Disponible",
                "Available"
            };

            RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;
            int FilterResultsCounter = 0;
            int index = 0;

            List<string> ElementWords = ElementName.Split('_').ToList();
            List<string> CompareList = new List<string>();
            List<RDFOntologyProperty> ResultProperties = new List<RDFOntologyProperty>();

            string ElementDefinitionIndividual = "Def_" + ElementName;
            RDFOntologyFact ElementDefinitionFact = GameOntology.Data.SelectFact(CurrentGameContext + ElementDefinitionIndividual);
            RDFOntologyTaxonomy ElementDefinitionAnnotations = GameOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(ElementDefinitionFact);
            RDFOntologyTaxonomyEntry ElementGeneralCostAnnotation = ElementDefinitionAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("GeneralCostDefinedBy")).SingleOrDefault();
            bool generalCostAnnotationFound = (ElementGeneralCostAnnotation != null) ? true : false;
            
            if(ElementGeneralCostAnnotation != null)
            {
                string AnnotationValue = ElementGeneralCostAnnotation.TaxonomyObject.ToString();
                if(AnnotationValue.Contains('^'))
                    AnnotationValue = AnnotationValue.Substring(0, AnnotationValue.IndexOf('^'));

                if (string.IsNullOrEmpty(AnnotationValue))
                    return null;

                RDFOntologyProperty GeneralCostProperty = GamePropertyModel.SelectProperty(this.CurrentGameContext + AnnotationValue);
                ResultProperties.Add(GeneralCostProperty);
                FilterResultsCounter = ResultProperties.Count();
            }
            else
            {
                index = 0;
                ResultProperties = GamePropertyModel.Where(entry => AvailableWords.Any(word => entry.ToString().Contains(word))).ToList();

                FilterResultsCounter = ResultProperties.Count();

                while (FilterResultsCounter > 1 && CompareList.Count() < ElementWords.Count())
                {
                    CompareList.Add(ElementWords.ElementAtOrDefault(index));
                    ResultProperties = ResultProperties.Where(entry => CompareList.All(word => entry.ToString().Contains(word))).ToList();
                    FilterResultsCounter = ResultProperties.Count();
                    ++index;
                }
            }


            if (FilterResultsCounter > 0)
            {
                int currentResultPropertiesCount = ResultProperties.Count();
                RDFOntologyProperty property;
                if (generalCostAnnotationFound == false)
                {
                    if (currentResultPropertiesCount == 1)
                    {
                        property = ResultProperties.SingleOrDefault();
                        string propertyName = property.ToString().Substring(property.ToString().LastIndexOf('#') + 1);
                        List<string> propertyWords = propertyName.Split('_').ToList();
                        if (propertyWords.Count() == ElementWords.Count() + 1)
                        {
                            if (!ElementWords.All(word => propertyName.Contains(word)) || !AvailableWords.Any(word => propertyName.Contains(word)))
                                currentResultPropertiesCount = 0;
                        }
                        else if (propertyWords.Count() == ElementWords.Count() + 2)
                        {
                            if (!ElementWords.All(word => propertyName.Contains(word)) || !AvailableWords.Any(word => propertyName.Contains(word)) || !propertyName.Contains("Total"))
                                currentResultPropertiesCount = 0;
                        }
                    }
                    else
                        currentResultPropertiesCount = 0;
                }
                    

                if (currentResultPropertiesCount != 0)
                {
                    RDFOntologyProperty LimitProperty = ResultProperties.SingleOrDefault();
                    LimitPropertyName = LimitProperty.ToString().Substring(LimitProperty.ToString().LastIndexOf('#') + 1);
                    RDFOntologyDatatypeProperty CharacterLimitProperty;
                    if (CheckDatatypeProperty(LimitPropertyName) == false)
                        CharacterLimitProperty = CreateDatatypeProperty(LimitPropertyName);
                    else
                        CharacterLimitProperty = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + LimitPropertyName) as RDFOntologyDatatypeProperty;
                    RDFOntologyTaxonomyEntry LimitPropertyAssertion = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(CharacterLimitProperty).SingleOrDefault();
                    if (LimitPropertyAssertion == null)
                    {
                        IEnumerable<RDFOntologyTaxonomyEntry> ResultAnnotations = GameOntology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(LimitProperty);
                        FilterResultsCounter = ResultAnnotations.Count();
                        CompareList.Clear();

                        while (FilterResultsCounter > 1)
                        {
                            CompareList.Add(ElementWords.ElementAtOrDefault(index));
                            ResultAnnotations = ResultAnnotations.Where(entry => CompareList.All(word => entry.ToString().Contains(word)));
                            FilterResultsCounter = ResultAnnotations.Count();
                            ++index;
                        }

                        if (ResultAnnotations.Count() > 0)
                        {
                            string LimitPropertyDefinition = ResultAnnotations.SingleOrDefault().TaxonomyObject.ToString();
                            if (LimitPropertyDefinition.Contains('^'))
                                LimitPropertyDefinition = LimitPropertyDefinition.Substring(0, LimitPropertyDefinition.IndexOf('^'));

                            //Meter en bucle foreach
                            if (!Regex.IsMatch(LimitPropertyDefinition, @"\d"))
                                AvailablePoints = GetValue(LimitPropertyDefinition);
                            else
                            {
                                if (!Regex.IsMatch(LimitPropertyDefinition, @"\D"))
                                    AvailablePoints = Convert.ToSingle(LimitPropertyDefinition);
                                else
                                    AvailablePoints = GetValue(LimitPropertyDefinition);
                            }
                        }
                    }
                    else
                    {
                        string AvailablePointsValue = LimitPropertyAssertion.TaxonomyObject.ToString();
                        AvailablePointsValue = AvailablePointsValue.Substring(0, AvailablePointsValue.IndexOf('^'));
                        AvailablePoints = Convert.ToSingle(AvailablePointsValue);
                    }
                }
            }
            
            if((FilterResultsCounter == 0) || LimitPropertyName == null)
            {
                string parents = GetParentClasses(ElementName);
                if(parents != null)
                {
                    List<string> parentsList = parents.Split(':').ToList();
                    foreach (string parent in parentsList)
                    {
                        if (AvailablePoints == null)
                        {
                            string parentHasAvailablePoints = GetAvailablePoints(parent, out float? parentAvailablePoints);
                            if (parentHasAvailablePoints != null)
                            {
                                LimitPropertyName = parentHasAvailablePoints;
                                AvailablePoints = parentAvailablePoints;
                            }
                        }
                    }
                }
            }
            return LimitPropertyName;
        }

        /// <summary>
        /// Returns true if the given element has a limit.
        /// </summary>
        /// <param name="ElementName">Name of the element</param>
        /// <param name="LimitValue">Limit value obtained</param>
        /// <returns></returns>
        internal string GetLimit(string ElementName, out float? LimitValue)
        {
            string LimitPropertyName = null;
            bool hasLimit = false;
            LimitValue = null;
            List<string> LimitWords = new List<string>()
            {
                "Límite",
                "Limit",
                "Limite"
            };
            
            RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;
            
            IEnumerable<RDFOntologyProperty> ResultProperties = GamePropertyModel.Where(entry => LimitWords.Any(word => entry.ToString().Contains(word)));
            List<string> ElementWords = ElementName.Split('_').ToList();
            List<string> CompareList = new List<string>();
            int index = 0;
            int FilterResultsCounter = ResultProperties.Count();
            
            while (FilterResultsCounter > 1)
            {
                CompareList.Add(ElementWords.ElementAtOrDefault(index));
                ResultProperties = ResultProperties.Where(entry => CompareList.All(word => entry.ToString().Contains(word)));
                FilterResultsCounter = ResultProperties.Count();
                if(ElementWords.Count()-1 == index && ResultProperties.Count() > 1)
                    break;
                ++index;
            }

            if (FilterResultsCounter > 0)
            {
                hasLimit = true;
                RDFOntologyDatatypeProperty LimitProperty;
                if (FilterResultsCounter > 1)
                    LimitProperty = ResultProperties.FirstOrDefault() as RDFOntologyDatatypeProperty;
                else
                    LimitProperty = ResultProperties.SingleOrDefault() as RDFOntologyDatatypeProperty;
                LimitPropertyName = LimitProperty.ToString().Substring(LimitProperty.ToString().LastIndexOf('#') + 1);
                RDFOntologyDatatypeProperty CharacterLimitProperty;
                if (!CheckDatatypeProperty(LimitPropertyName))
                    CharacterLimitProperty = CreateDatatypeProperty(LimitPropertyName);
                else
                    CharacterLimitProperty = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + LimitPropertyName) as RDFOntologyDatatypeProperty;

                RDFOntologyTaxonomy CharacterPropertyAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(CharacterLimitProperty);
                if(CharacterPropertyAssertions.Count() == 0)
                {
                    IEnumerable<RDFOntologyTaxonomyEntry> ResultAnnotations = GameOntology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(LimitProperty);
                    FilterResultsCounter = ResultAnnotations.Count();
                    CompareList.Clear();

                    while (FilterResultsCounter > 1)
                    {
                        CompareList.Add(ElementWords.ElementAtOrDefault(index));
                        ResultAnnotations = ResultAnnotations.Where(entry => CompareList.All(word => entry.ToString().Contains(word)));
                        FilterResultsCounter = ResultAnnotations.Count();
                        ++index;
                    }

                    if (ResultAnnotations.Count() > 0)
                    {
                        string LimitPropertyDefinition = ResultAnnotations.SingleOrDefault().TaxonomyObject.ToString();
                        if(LimitPropertyDefinition.Contains('^'))
                            LimitPropertyDefinition = LimitPropertyDefinition.Substring(0, LimitPropertyDefinition.IndexOf('^'));

                        //Meter en bucle foreach
                        if (!Regex.IsMatch(LimitPropertyDefinition, @"\d"))
                            LimitValue = GetValue(LimitPropertyDefinition);
                        else
                            LimitValue = Convert.ToSingle(LimitPropertyDefinition);
                    }
                }
                else
                {                    
                    RDFOntologyTaxonomyEntry entry = CharacterPropertyAssertions.SingleOrDefault();
                    string value = entry.TaxonomyObject.ToString().Substring(0, entry.TaxonomyObject.ToString().IndexOf('^'));
                    LimitValue = Convert.ToSingle(value);
                }
            }
            else
            {
                string parents = GetParentClasses(ElementName);
                if (parents != null)
                {
                    List<string> parentList = parents.Split(':').ToList();
                    foreach (string parent in parentList)
                    {
                        if (hasLimit == false)
                        {
                            LimitPropertyName = GetLimit(parent, out LimitValue);
                            hasLimit = LimitPropertyName != null;
                        }
                    }
                }
            }
            return LimitPropertyName;
        }

        /// <summary>
        /// Returns the name of a limit given its value
        /// </summary>
        /// <param name="value">Value of the limit</param>
        /// <returns></returns>
        internal string GetLimitByValue(string stage, string value)
        {
            string Limit = null;

            RDFOntologyFact CharacterFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);
            RDFOntologyTaxonomy CharacterAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
            IEnumerable<RDFOntologyTaxonomyEntry> CharacterAssertionsValueEntries = CharacterAssertions.Where(entry => entry.TaxonomyObject.ToString().Contains(value));

            if(CharacterAssertionsValueEntries.Count() == 1)
                Limit = CharacterAssertionsValueEntries.SingleOrDefault().TaxonomyPredicate.ToString().Substring(CharacterAssertionsValueEntries.SingleOrDefault().TaxonomyPredicate.ToString().LastIndexOf('#')+1);

            else 
            { 
                if(CharacterAssertionsValueEntries.Count() > 1)
                {
                    string StageGeneralLimit = GetAvailablePoints(stage, out float? LimitValue);
                    if(LimitValue.ToString() != value)
                    {
                        string StagePartialLimit = GetLimit(stage, out LimitValue);
                        if (LimitValue.ToString() != value)
                        {
                            string parents = GetParentClasses(stage);
                            if(parents != null)
                            {
                                List<string> parentList = parents.Split(':').ToList();
                                foreach (string parent in parentList)
                                    Limit = GetLimitByValue(parent, value);
                            }
                        }
                        else
                            Limit = StagePartialLimit;
                    }
                    else
                        Limit = StageGeneralLimit;
                }
            }

            return Limit;
        }

        /// <summary>
        /// Returns the name of the object property associated to the stage given
        /// </summary>
        /// <param name="stage">Name of the stage</param>
        /// <returns></returns>
        internal string GetObjectPropertyAssociated(string stage)
        {
            string propertyName = null;
            bool propertyNameFound = false;
            List<string> StageWords = stage.Split('_').ToList();
            int wordCounter = StageWords.Count();
            while(propertyNameFound == false && wordCounter > 0)
            {
                string ObjectPropertyName = "tiene";
                for (int i = 0; i < wordCounter; ++i)
                    ObjectPropertyName += StageWords.ElementAtOrDefault(i);
               
                IEnumerable<RDFOntologyProperty> ObjectPropertyAssertions = GameOntology.Model.PropertyModel.Where(entry => entry.Range != null && entry.Range.ToString().Contains(stage));
                if(ObjectPropertyAssertions.Count() > 1)
                {
                    ObjectPropertyAssertions = ObjectPropertyAssertions.Where(entry => entry.ToString().Contains(ObjectPropertyName));
                    propertyName = ObjectPropertyAssertions.SingleOrDefault().ToString().Substring(ObjectPropertyAssertions.SingleOrDefault().ToString().LastIndexOf('#') + 1);
                    propertyNameFound = true;
                }
                else if(ObjectPropertyAssertions.Count() == 1)
                {
                    propertyName = ObjectPropertyAssertions.SingleOrDefault().ToString().Substring(ObjectPropertyAssertions.SingleOrDefault().ToString().LastIndexOf('#') + 1);
                    propertyNameFound = true;
                }
                --wordCounter;
            }
            if(wordCounter == 0)
            {
                string parents = GetParentClasses(stage);
                if(parents != null)
                {
                    List<string> parentList = parents.Split(':').ToList();
                    foreach(string parent in parentList)
                    {
                        if(propertyNameFound == false)
                        {
                            propertyName = GetObjectPropertyAssociated(parent);
                            if (propertyName != null)
                                propertyNameFound = true;
                        }
                    }
                }
            }

            return propertyName;
        }

        /// <summary>
        /// Returns a SortedList with the order and the name of the substages given a stage
        /// </summary>
        /// <param name="stage"></param>
        /// <returns></returns>
        internal SortedList<int, string> GetOrderedSubstages(string stage)
        {
            ObservableCollection<Group> Substages = this.GetSubClasses(stage);
            Dictionary<int, string> SubstagesAndOrder = new Dictionary<int, string>();

            foreach (Group substage in Substages)
            {
                string SubstageName = "Def_" + substage.Title;
                RDFOntologyFact SubstageDefinitionFact = this.GameOntology.Data.SelectFact(this.CurrentGameContext + SubstageName);
                RDFOntologyTaxonomyEntry SubstageOrderEntry = this.GameOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(SubstageDefinitionFact).Where(entry => entry.TaxonomyPredicate.ToString().Contains("SubstageOrder")).SingleOrDefault();
                if (SubstageOrderEntry != null)
                    SubstagesAndOrder.Add(Convert.ToInt32(SubstageOrderEntry.TaxonomyObject.ToString().Substring(0, SubstageOrderEntry.TaxonomyObject.ToString().IndexOf('^'))), substage.Title);
            }

            return new SortedList<int, string>(SubstagesAndOrder);
        }

        /// <summary>
        /// Returns a string containing the names of the types of the given element
        /// </summary>
        /// <param name="elementName">Name of the element</param>
        /// <returns></returns>
        internal string GetParentClasses(string elementName)
        {
            string parent = null;
            RDFOntologyClass elementClass = GameOntology.Model.ClassModel.SelectClass(CurrentGameContext + elementName);
            if(elementClass != null)
            {
                RDFOntologyTaxonomy elementClassEntries = GameOntology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(elementClass);
                foreach (RDFOntologyTaxonomyEntry entry in elementClassEntries)
                    parent += entry.TaxonomyObject.ToString().Substring(entry.TaxonomyObject.ToString().LastIndexOf('#') + 1) + ":";
                if (parent != null)
                    if (parent.EndsWith(':'))
                        parent = parent[0..^1];
            }
            else
            {
                RDFOntologyFact elementFact = GameOntology.Data.SelectFact(CurrentGameContext + elementName);
                RDFOntologyTaxonomy ElementClassAssertions = GameOntology.Data.Relations.ClassType.SelectEntriesBySubject(elementFact);
                foreach(RDFOntologyTaxonomyEntry entry in ElementClassAssertions)
                    parent += entry.TaxonomyObject.ToString().Substring(entry.TaxonomyObject.ToString().LastIndexOf('#') + 1) + ":";
                if (parent != null)
                    if (parent.EndsWith(':'))
                        parent = parent[0..^1];
            }
            
            return parent;
        }

        /// <summary>
        /// Returns the general cost of an element given its name and the current stage
        /// </summary>
        /// <param name="stage">Name of the current stage</param>
        /// <param name="ElementName">Name of the element</param>
        /// <returns></returns>
        internal string GetGeneralCost(string stage, string ElementName, out float generalCost)
        {
            string GeneralCostName = null;
            generalCost = 0;
            List<string> CostWords = new List<string> { "Coste", "Cost", "Coût" };
            RDFOntologyFact ItemFact = GameOntology.Data.SelectFact(CurrentGameContext + ElementName);
            RDFOntologyTaxonomy ItemFactAssertions = GameOntology.Data.Relations.Assertions.SelectEntriesBySubject(ItemFact);
            IEnumerable<RDFOntologyTaxonomyEntry> ItemCosts = ItemFactAssertions.Where(entry => CostWords.Any(word => entry.ToString().Contains(word)));

            string GeneralCostPredicateName = CheckGeneralCost(stage);
            ItemCosts = ItemCosts.Where(entry => entry.TaxonomyPredicate.ToString().Contains(GeneralCostPredicateName));

            if (ItemCosts.Count() > 0)
            {
                RDFOntologyTaxonomyEntry ItemCostEntry = ItemCosts.SingleOrDefault();
                generalCost = Convert.ToSingle(ItemCostEntry.TaxonomyObject.ToString().Substring(0, ItemCostEntry.TaxonomyObject.ToString().IndexOf('^')));
                GeneralCostName = ItemCostEntry.TaxonomyPredicate.ToString().Substring(ItemCostEntry.TaxonomyPredicate.ToString().LastIndexOf('#') + 1);
            }

            return GeneralCostName; 
        }

        /// <summary>
        /// Returns the partial cost of an element given its name and the current stage
        /// </summary>
        /// <param name="stage">Name of the current stage</param>
        /// <param name="ElementName">Name of the element</param>
        /// <returns></returns>
        internal string GetPartialCost(string stage, string ElementName, out float partialCost)
        {
            partialCost = 0;
            List<string> CostWords = new List<string> { "Coste", "Cost", "Coût" };
            RDFOntologyFact ItemFact = GameOntology.Data.SelectFact(CurrentGameContext + ElementName);
            RDFOntologyTaxonomy ItemFactAssertions = GameOntology.Data.Relations.Assertions.SelectEntriesBySubject(ItemFact);
            IEnumerable<RDFOntologyTaxonomyEntry> ItemCosts = ItemFactAssertions.Where(entry => CostWords.Any(word => entry.ToString().Contains(word)));
            if (ItemCosts.Count() > 1)
            {
                string GeneralCostPredicateName = CheckGeneralCost(stage);
                ItemCosts = ItemCosts.Where(entry => !entry.TaxonomyPredicate.ToString().Contains(GeneralCostPredicateName));
            }

            RDFOntologyTaxonomyEntry ItemCostEntry = ItemCosts.SingleOrDefault();
            if (ItemCostEntry != null)
                if (ItemCostEntry != null)
                    partialCost = Convert.ToSingle(ItemCostEntry.TaxonomyObject.ToString().Substring(0, ItemCostEntry.TaxonomyObject.ToString().IndexOf('^')));

            return ItemCostEntry.TaxonomyPredicate.ToString().Substring(ItemCostEntry.TaxonomyPredicate.ToString().LastIndexOf('#') + 1);
        }

        internal string GetCostRequirement(string property, out float? CostRequirementValue)
        {
            string CostRequirement;

            RDFOntologyFact CharacterFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);
            RDFOntologyTaxonomy CharacterAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
            int index = 0;
            string FirstWord = null;
            while(FirstWord == null)
            {
                RDFOntologyTaxonomyEntry entry = CharacterAssertions.ElementAtOrDefault(index);
                string entryPredicate = entry.TaxonomyPredicate.ToString().Substring(entry.TaxonomyPredicate.ToString().LastIndexOf('#') + 1);
                if (CheckDatatypeProperty(entryPredicate))
                    FirstWord = entryPredicate.Split('_').FirstOrDefault();
                else
                    ++index;
            }

            string propertyFirstWord = property.Split('_').FirstOrDefault();
            string propertyLastWord = property.Split('_').LastOrDefault();
            CostRequirement = property.Replace(propertyFirstWord, FirstWord).Replace(propertyLastWord, "Total");

            RDFOntologyDatatypeProperty predicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + CostRequirement) as RDFOntologyDatatypeProperty;
            RDFOntologyTaxonomyEntry CostRequirementEntry = CharacterAssertions.SelectEntriesByPredicate(predicate).SingleOrDefault();

            string value = CostRequirementEntry.TaxonomyObject.ToString().Substring(0, CostRequirementEntry.TaxonomyObject.ToString().IndexOf('^'));
            CostRequirementValue = Convert.ToSingle(value);

            return CostRequirement;
        }

        /// <summary>
        /// Returns a value given a string with the formula, the current item name and the user input
        /// </summary>
        /// <param name="valueDefinition">Formula definition</param>
        /// <param name="itemName">Name of the current item</param>
        /// <param name="User_Input">Value given by the user to the current item</param>
        /// <returns></returns>
        internal float GetValue(string valueDefinition, string itemName = null, string User_Input = null)
        {
            string SubjectRef = null;
            string CurrentValue = null;
            List<string> operators = new string[] { "+", "-", "*", "/", "%", "<", ">", "<=", ">=", "=", "!=" }.ToList();
            List<dynamic> currentList = null;
            bool hasUpperLimit = false;
            float UpperLimit = 0;

            valueDefinition = valueDefinition.Replace("Item", itemName).Replace("__","_");
            
            List<string> expression = valueDefinition.Split(':').Select(innerItem => innerItem.Trim()).ToList();
            
            for (int index = 0; index < expression.Count(); ++index)
            {
                string element = expression.ElementAtOrDefault(index);
                if (element.Contains("Ref"))
                    element = element.Replace("Ref", SubjectRef);

                if (element.EndsWith("()"))
                {
                    string method = element.Substring(0, element.IndexOf("(") - 1);
                    MethodInfo mi;
                    if (method.Contains("Math."))
                    {
                        string methodFunction = method.Split('.').LastOrDefault();
                        mi = typeof(Math).GetMethod(methodFunction, new[] { typeof(float) });
                        List<ParameterInfo> methodParameters = mi.GetParameters().ToList();
                        if (methodParameters.Count > 0)
                        {
                            object[] parameters = { CurrentValue };
                            CurrentValue = mi.Invoke(currentList, parameters).ToString();
                        }
                        else
                            CurrentValue = mi.Invoke(currentList, null).ToString();
                    }
                    else
                    {
                        mi = typeof(Math).GetMethod(method, new[] { typeof(float) });
                        if (mi != null)
                        {
                            List<ParameterInfo> methodParameters = mi.GetParameters().ToList();
                            if (methodParameters.Count > 0)
                            {
                                object[] parameters = { CurrentValue };
                                CurrentValue = mi.Invoke(currentList, parameters).ToString();
                            }
                            else
                                CurrentValue = mi.Invoke(currentList, null).ToString();
                        }
                    }
                    
                }
                else if (operators.Any(op => element == op))
                {
                    string NextElement = expression.ElementAtOrDefault(index + 1);
                    bool isValue = float.TryParse(NextElement, out float nextValue);
                    bool isFloat = false;
                    if (isValue == false)
                    {
                        RDFOntologyFact CharacterFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);
                        if (CheckObjectProperty(NextElement))
                        {
                            int row_index = index;
                            RDFOntologyObjectProperty nextElementProperty = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + NextElement) as RDFOntologyObjectProperty;
                            RDFOntologyTaxonomy CharacterFactAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                            RDFOntologyTaxonomyEntry nextElementEntry = CharacterFactAssertions.Where(item => item.TaxonomyPredicate == nextElementProperty).SingleOrDefault();
                            RDFOntologyFact nextElementFact = nextElementEntry.TaxonomyObject as RDFOntologyFact;

                            ++row_index;
                            NextElement = expression.ElementAtOrDefault(row_index + 1).Replace("Item", itemName);
                            if (CheckDatatypeProperty(NextElement, false))
                            {
                                RDFOntologyDatatypeProperty nextElementDatatypeProperty;
                                if (!CheckDatatypeProperty(NextElement))
                                    nextElementDatatypeProperty = CreateDatatypeProperty(NextElement);
                                else
                                    nextElementDatatypeProperty = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + NextElement) as RDFOntologyDatatypeProperty;

                                RDFOntologyTaxonomy ObjectFactAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(nextElementFact);
                                nextElementEntry = ObjectFactAssertions.Where(item => item.TaxonomyPredicate == nextElementDatatypeProperty).SingleOrDefault();
                                string nextValueString = nextElementEntry.TaxonomyObject.ToString();
                                if (!nextValueString.Contains("float"))
                                {
                                    nextValueString = nextValueString.Substring(0, nextValueString.IndexOf('^'));
                                    if (nextValueString.Contains(','))
                                        nextValueString = nextValueString.Split(',').ElementAtOrDefault(0);
                                    nextValue = Convert.ToSingle(nextValueString);
                                }
                                else
                                {
                                    isFloat = true;
                                    nextValue = Convert.ToSingle(nextValueString);
                                }
                            }
                            else
                            {
                                List<string> NextElementWords = NextElement.Split('_').ToList();
                                int wordCounter = NextElementWords.Count() - 1;
                                //Poner bucle descomponer en palabras y comprobar por conjuntos reducidos de palabras
                                while (!CheckDatatypeProperty(NextElement) && wordCounter > 0)
                                {
                                    NextElement = "";
                                    for (int i = 0; i < wordCounter - 1; ++i)
                                        NextElement += NextElementWords.ElementAtOrDefault(i) + "_";
                                    NextElement += NextElementWords.ElementAtOrDefault(wordCounter);
                                    --wordCounter;
                                }
                                if(wordCounter > 1)
                                {
                                    RDFOntologyDatatypeProperty nextElementDatatypeProperty;
                                    if (!CheckDatatypeProperty(NextElement))
                                        nextElementDatatypeProperty = CreateDatatypeProperty(NextElement);
                                    else
                                        nextElementDatatypeProperty = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + NextElement) as RDFOntologyDatatypeProperty;

                                    RDFOntologyTaxonomy ObjectFactAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(nextElementFact);
                                    nextElementEntry = ObjectFactAssertions.Where(item => item.TaxonomyPredicate == nextElementDatatypeProperty).SingleOrDefault();
                                    string nextValueString = nextElementEntry.TaxonomyObject.ToString();
                                    if (!nextValueString.Contains("float"))
                                    {
                                        nextValueString = nextValueString.Substring(0, nextValueString.IndexOf('^'));
                                        if (nextValueString.Contains(','))
                                            nextValueString = nextValueString.Split(',').ElementAtOrDefault(0);
                                        nextValue = Convert.ToSingle(nextValueString);
                                    }
                                    else
                                    {
                                        isFloat = true;
                                        nextValue = Convert.ToSingle(nextValueString);
                                    }
                                }
                                else
                                {
                                    string itemParents = GetParentClasses(itemName);

                                    if(itemParents != null)
                                    {
                                        string newItem = null;
                                        string newValueDefinition = " ";
                                        List<string> ParentList = itemParents.Split(':').ToList();
                                        
                                        foreach (string parent in ParentList)
                                        {
                                            if (newItem == null)
                                            {
                                                for(int i = index; i < expression.Count()-1; ++i)
                                                {
                                                    string newNextElement = expression.ElementAt(i + 1).Replace(itemName, parent);
                                                    if (CheckDatatypeProperty(newNextElement, false) == true)
                                                    {
                                                        List<string> newValueList = valueDefinition.Split(':').ToList();

                                                        //Buscar propiedad que contenga PD
                                                        string basePointsWord = null;
                                                        bool descriptionFound = false;
                                                        string itemClass = GetParentClasses(itemName);
                                                        string itemClassDescription = null;
                                                        while(descriptionFound == false)
                                                        {
                                                            string classDefinition = "Def_" + itemClass;
                                                            RDFOntologyFact classDefinitionFact = GameOntology.Data.SelectFact(CurrentGameContext + classDefinition);
                                                            if(classDefinitionFact != null)
                                                            {
                                                                IEnumerable<RDFOntologyTaxonomyEntry> classDefinitionAnnotations = GameOntology.Data.Annotations.CustomAnnotations.Where(entry => entry.TaxonomySubject == classDefinitionFact);
                                                                if (classDefinitionAnnotations.Count() > 0)
                                                                {
                                                                    IEnumerable<RDFOntologyTaxonomyEntry> ValuedListInfoAnnotations = classDefinitionAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("ValuedListInfo"));
                                                                    if (ValuedListInfoAnnotations.Count() > 0)
                                                                    {
                                                                        itemClassDescription = ValuedListInfoAnnotations.SingleOrDefault().TaxonomyObject.ToString().Substring(0, ValuedListInfoAnnotations.SingleOrDefault().TaxonomyObject.ToString().IndexOf('^'));
                                                                        descriptionFound = true;
                                                                    }
                                                                }
                                                            }
                                                            itemClass = GetParentClasses(itemClass);
                                                        }

                                                        List<string> DescriptionRows = itemClassDescription.Split('\n').ToList();
                                                        foreach(string row in DescriptionRows)
                                                        {
                                                            List<string> rowElements = row.Split(',').ToList();
                                                            bool userEditValue = Convert.ToBoolean(rowElements.Where(element => element.Contains("user_edit")).SingleOrDefault().Split(':').LastOrDefault());
                                                            if(userEditValue == true)
                                                            {
                                                                basePointsWord = rowElements.FirstOrDefault().Split('_').LastOrDefault();
                                                                break;
                                                            }
                                                        }

                                                        int listCount = newValueList.Count();
                                                        for(int listIndex = 0; listIndex < listCount; ++listIndex)
                                                        {
                                                            if(newValueList.ElementAtOrDefault(listIndex).Contains(basePointsWord))
                                                            {
                                                                newValueList.RemoveAt(listIndex);
                                                                newValueList.Insert(listIndex, User_Input);
                                                            }                                                                
                                                        }

                                                        foreach(string item in newValueList)
                                                        {
                                                            int listIndex = newValueList.IndexOf(item);
                                                            if (listIndex == i + 1)
                                                                newValueDefinition += newNextElement + ':';
                                                            else
                                                                newValueDefinition += item + ':';
                                                        }
                                                        break;
                                                    }
                                                }
                                                
                                            }
                                            else
                                                break;
                                        }
                                        if (newValueDefinition.EndsWith(':'))
                                            newValueDefinition = newValueDefinition[0..^1];

                                        nextValue = GetValue(newValueDefinition.ToString(), itemName, User_Input);                                        
                                    }
                                }                              
                            }
                        }
                        else
                        {
                            RDFOntologyDatatypeProperty predicate;
                            string characterClassName = GetClass(CurrentCharacterName, true);
                            string nextElementFirstWord = NextElement.Split('_').ToList().First();
                            if (!characterClassName.Contains(nextElementFirstWord))
                            {
                                //Buscar elemento que contenga la palabra nextElementFirstWord
                                RDFOntologyClass nextElementClass = GameOntology.Model.ClassModel.Where(item => item.ToString().Contains(nextElementFirstWord)).SingleOrDefault();
                                string nextElementClassName = nextElementClass.ToString().Substring(nextElementClass.ToString().LastIndexOf('#') + 1);

                                RDFOntologyTaxonomyEntry CharacterNextElementEntry = CharacterOntology.Data.Relations.ClassType.Where(entry => entry.TaxonomyObject.ToString().Contains(nextElementClassName)).SingleOrDefault();
                                string CharacterNextElementFactName = CharacterNextElementEntry.TaxonomySubject.ToString().Substring(CharacterNextElementEntry.TaxonomySubject.ToString().LastIndexOf('#') + 1);
                                if (!CheckIndividual(CharacterNextElementFactName))
                                    CharacterFact = CreateIndividual(CharacterNextElementFactName);
                                else
                                    CharacterFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CharacterNextElementFactName);
                            }
                            else
                                CharacterFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);

                            if (!CheckDatatypeProperty(NextElement))
                                predicate = CreateDatatypeProperty(NextElement);
                            else
                                predicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + NextElement) as RDFOntologyDatatypeProperty;

                            //Seguir aquí
                            string nextValueString = null;
                            RDFOntologyTaxonomy CharacterPredicateAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                            RDFOntologyTaxonomyEntry CharacterPredicateEntry = CharacterPredicateAssertions.Where(entry => entry.TaxonomyPredicate == predicate).SingleOrDefault();
                            if (CharacterPredicateEntry == null)
                            {
                                string predicateName = predicate.ToString().Substring(predicate.ToString().LastIndexOf('#') + 1);
                                RDFOntologyDatatypeProperty GamePredicate = GameOntology.Model.PropertyModel.SelectProperty(CurrentGameContext + predicateName) as RDFOntologyDatatypeProperty;
                                CharacterPredicateEntry = GameOntology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(GamePredicate).SingleOrDefault();
                                string predicateDefinition = CharacterPredicateEntry.TaxonomyObject.ToString().Substring(0, CharacterPredicateEntry.TaxonomyObject.ToString().IndexOf('^'));
                                string predicateType = CharacterPredicateEntry.TaxonomyObject.ToString().Substring(CharacterPredicateEntry.TaxonomyObject.ToString().IndexOf('^'));
                                nextValueString = GetValue(predicateDefinition).ToString() + predicateType;
                            }
                            else
                                nextValueString = CharacterPredicateEntry.TaxonomyObject.ToString();

                            //string nextValueSubstring = nextValueString.Substring(0, nextValueString.IndexOf('^'));
                            //if (Regex.IsMatch(nextValueSubstring, @"\D"))
                            //    nextValue = GetValue(nextValueSubstring, itemName, User_Input);

                            // check if nextValuestring has words and no digits
                            string nextValueDigits = nextValueString.Substring(0, nextValueString.IndexOf('^'));
                           
                            if (!nextValueString.Contains("float"))
                            {
                                if (nextValueDigits.Contains(','))
                                    nextValueDigits = nextValueString.Split(',').ElementAtOrDefault(0);

                                nextValue = Convert.ToSingle(nextValueDigits);
                            }
                            else
                            {
                                isFloat = true;
                                nextValueString = nextValueString.Substring(0, nextValueString.IndexOf('^'));
                                nextValue = Convert.ToSingle(nextValueString, CultureInfo.InvariantCulture);
                            }
                        }
                    }

                    if (element == "/" && nextValue == 0)
                        nextValue = 1;
                    dynamic operatorResult = ConvertToOperator(element, Convert.ToSingle(CurrentValue), nextValue);

                    if (operatorResult.GetType().ToString().Contains("boolean"))
                    {
                        foreach (Item individual in GetIndividuals(itemName))
                            if (operatorResult == true)
                                currentList.Add(individual.Name);
                    }
                    else
                    {
                        if (isFloat == false)
                        {
                            string resultString = operatorResult.ToString();
                            if (resultString.Contains(','))
                                CurrentValue = resultString.Split(',').ElementAtOrDefault(0);
                            else
                                CurrentValue = resultString;
                        }
                        else
                            CurrentValue = operatorResult.ToString();
                    }
                }
                else if ((Regex.IsMatch(element, @"\d") && (!Regex.IsMatch(element, @"\D"))))
                {
                    if (index == 0)
                        CurrentValue = element.ToString();
                    else
                        continue;
                }
                else if (CheckClass(element, false))
                {
                    string property = expression.ElementAtOrDefault(index + 1);

                    RDFOntologyFact currentFact = GameOntology.Data.SelectFact(CurrentGameContext + element + "_" + User_Input);
                    RDFOntologyProperty currentProperty = GameOntology.Model.PropertyModel.SelectProperty(CurrentGameContext + property);
                    RDFOntologyTaxonomyEntry elementAssertion = GameOntology.Data.Relations.Assertions.SelectEntriesBySubject(currentFact).Where(entry => entry.TaxonomyPredicate == currentProperty).SingleOrDefault();

                    CurrentValue = elementAssertion.TaxonomyObject.ToString().Substring(0, elementAssertion.TaxonomyObject.ToString().IndexOf('^'));
                }
                else if (CheckIndividual(element, false))
                {
                    string property = expression.ElementAtOrDefault(index + 1);

                    RDFOntologyFact currentFact = GameOntology.Data.SelectFact(CurrentGameContext + element);
                    RDFOntologyProperty currentProperty = GameOntology.Model.PropertyModel.SelectProperty(CurrentGameContext + property);
                    if(currentProperty == null)
                    {
                        List<string> propertyWords = property.Split('_').ToList();
                        int wordCounter = propertyWords.Count();
                        bool elementFound = false;
                        string propertyName = "";

                        while (elementFound == false && wordCounter > 1)
                        {
                            propertyName = "";
                            for (int i = 0; i < wordCounter - 1; ++i)
                                propertyName += propertyWords.ElementAtOrDefault(i) + "_";
                            propertyName += propertyWords.LastOrDefault();

                            if (this.CheckDatatypeProperty(propertyName, false) == true)
                            {
                                elementFound = true;
                                currentProperty = GameOntology.Model.PropertyModel.SelectProperty(CurrentGameContext + propertyName);
                            }
                            --wordCounter;
                        }
                    }

                    RDFOntologyTaxonomyEntry elementPropertyEntry = GameOntology.Data.Relations.Assertions.SelectEntriesBySubject(currentFact).Where(entry => entry.TaxonomyPredicate == currentProperty).SingleOrDefault();
                    CurrentValue = elementPropertyEntry.TaxonomyObject.ToString().Substring(0, elementPropertyEntry.TaxonomyObject.ToString().IndexOf('^'));
                }
                else if (CheckObjectProperty(element, false))
                {
                    string currentPropertyName = expression.ElementAtOrDefault(index);
                    string previousProperty = expression.ElementAtOrDefault(index - 1);
                    if (!operators.Any(op => previousProperty == op))
                    {
                        RDFOntologyFact SubjectFact;
                        RDFOntologyObjectProperty objectPredicate;
                        RDFOntologyTaxonomy SubjectFactAssertions;
                        RDFOntologyTaxonomyEntry SubjectFactPredicateEntry;
                        List<string> CharacterClass = new List<string> { "Personaje", "Character", "Personnage" };

                        string ItemClassName;
                        if (itemName != null)
                            ItemClassName = GetClass(itemName);
                        else
                            ItemClassName = null;
                        string elementFirstWord = element.Split('_').ToList().First();

                        RDFOntologyFact CharacterFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);
                        IEnumerable<RDFOntologyTaxonomyEntry> CharacterAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                        bool datatypeAssertionFound = false;
                        string characterDatatypePropertyFirstWord = null;
                        foreach(RDFOntologyTaxonomyEntry entry in CharacterAssertions)
                        {
                            string propertyName = entry.TaxonomyPredicate.ToString().Substring(entry.TaxonomyPredicate.ToString().LastIndexOf('#')+1);
                            if(CheckDatatypeProperty(propertyName) == true)
                            {
                                RDFOntologyDatatypeProperty DatatypeProperty = entry.TaxonomyPredicate as RDFOntologyDatatypeProperty;
                                if(DatatypeProperty.Domain != null)
                                {
                                    if (CharacterClass.Any(word => DatatypeProperty.Domain.ToString().Contains(word)))
                                    {
                                        characterDatatypePropertyFirstWord = DatatypeProperty.ToString().Substring(DatatypeProperty.ToString().LastIndexOf('#') + 1);
                                        characterDatatypePropertyFirstWord = characterDatatypePropertyFirstWord.Split('_').FirstOrDefault();
                                        datatypeAssertionFound = true;
                                    }
                                }                                    
                            }

                            if (datatypeAssertionFound == true)
                                break;
                        }

                        if (ItemClassName != null && ItemClassName.Contains(characterDatatypePropertyFirstWord))
                        {
                            if(CharacterClass.Any(word => word.Contains(characterDatatypePropertyFirstWord)))
                                SubjectFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);
                            else
                            {
                                if (!CheckIndividual(itemName))
                                    CreateIndividual(itemName);
                                SubjectFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + itemName);
                            }                                
                        }
                        else if(ItemClassName == null)
                        {
                            SubjectFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);
                        }
                        else
                        {
                            bool useCharacterContext = CheckObjectProperty(element);
                            if(useCharacterContext == true)
                                objectPredicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + element) as RDFOntologyObjectProperty;
                            else
                                objectPredicate = GameOntology.Model.PropertyModel.SelectProperty(CurrentGameContext + element) as RDFOntologyObjectProperty;

                            RDFOntologyObjectProperty ParentProperty = objectPredicate;
                            bool elementHasParentProperty = true;
                            while(elementHasParentProperty == true)
                            {
                                if (useCharacterContext == true)
                                {
                                    RDFOntologyTaxonomy predicateParentAssertion = CharacterOntology.Model.PropertyModel.Relations.SubPropertyOf.SelectEntriesBySubject(objectPredicate);
                                    if (predicateParentAssertion.Count() == 0)
                                    {
                                        ParentProperty = objectPredicate;
                                        elementHasParentProperty = false;
                                    }
                                    else
                                        objectPredicate = predicateParentAssertion.SingleOrDefault().TaxonomyObject as RDFOntologyObjectProperty;
                                }
                                else
                                {
                                    RDFOntologyTaxonomy predicateParentAssertion = GameOntology.Model.PropertyModel.Relations.SubPropertyOf.SelectEntriesBySubject(objectPredicate);
                                    if (predicateParentAssertion.Count() == 0)
                                    {
                                        ParentProperty = objectPredicate;
                                        elementHasParentProperty = false;
                                    }
                                    else
                                        objectPredicate = predicateParentAssertion.SingleOrDefault().TaxonomyObject as RDFOntologyObjectProperty;
                                }
                            }

                            if (CharacterClass.Any(word => ParentProperty.ToString().Contains(word)))
                                SubjectFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);
                            else
                            {
                                if (!CheckIndividual(itemName))
                                    CreateIndividual(itemName);
                                SubjectFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + itemName);
                            }

                        }

                        if (!CheckObjectProperty(element))
                            CreateObjectProperty(element);

                        string SubjectContext = SubjectFact.ToString().Substring(0, SubjectFact.ToString().IndexOf('#')+1);
                        if (SubjectContext == CurrentCharacterContext)
                            objectPredicate = CharacterOntology.Model.PropertyModel.SelectProperty(CharacterOntology + element) as RDFOntologyObjectProperty;
                        else
                            objectPredicate = GameOntology.Model.PropertyModel.SelectProperty(CurrentGameContext + element) as RDFOntologyObjectProperty;

                        if (SubjectContext == CurrentCharacterContext)
                            SubjectFactAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);
                        else
                            SubjectFactAssertions = GameOntology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);

                        if (SubjectFactAssertions.Count() > 1)
                            SubjectFactPredicateEntry = SubjectFactAssertions.SelectEntriesByPredicate(objectPredicate).SingleOrDefault();
                        else
                            SubjectFactPredicateEntry = SubjectFactAssertions.SingleOrDefault();

                        if(SubjectFactPredicateEntry != null)
                            SubjectRef = SubjectFactPredicateEntry.TaxonomyObject.ToString().Substring(SubjectFactPredicateEntry.TaxonomyObject.ToString().LastIndexOf('#') + 1);

                        string nextProperty = expression.ElementAtOrDefault(index + 1);
                        if (nextProperty.Contains("Item"))
                            nextProperty = nextProperty.Replace("Item", itemName);
                        if (nextProperty.Contains("Ref"))
                            nextProperty = nextProperty.Replace("Ref", SubjectRef);

                        int nextPropertyCounter = 1;
                        if (CheckDatatypeProperty(nextProperty))
                        {
                            currentPropertyName = nextProperty;
                            RDFOntologyDatatypeProperty DatatypePredicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + nextProperty) as RDFOntologyDatatypeProperty;

                            string characterClassName = GetClass(CurrentCharacterName, true);
                            string nextPropertyFirstWord = nextProperty.Split('_').ToList().First();
                            if (characterClassName.Contains(nextPropertyFirstWord))
                                CharacterFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);
                            else
                                CharacterFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + SubjectRef);

                            RDFOntologyTaxonomy CharacterFactAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                            RDFOntologyTaxonomyEntry CharacterFactPredicateEntry = CharacterFactAssertions.SelectEntriesByPredicate(DatatypePredicate).SingleOrDefault();
                            if (CharacterFactPredicateEntry == null)
                            {
                                CurrentValue = 0.ToString();
                                SubjectRef = CurrentValue;
                            }
                            else
                            {
                                int row_index = index;

                                RDFOntologyDatatypeProperty GameDatatypePredicate = GameOntology.Model.PropertyModel.SelectProperty(CurrentGameContext + nextProperty) as RDFOntologyDatatypeProperty;
                                RDFOntologyTaxonomy PropertyAnnotations = GameOntology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(GameDatatypePredicate);
                                IEnumerable<RDFOntologyTaxonomyEntry> PropertyUpperLimitAnnotation = PropertyAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("hasUpperLimit"));
                                if(PropertyUpperLimitAnnotation.Count() > 0)
                                {
                                    hasUpperLimit = true;
                                    UpperLimit = Convert.ToSingle(PropertyUpperLimitAnnotation.SingleOrDefault().TaxonomyObject.ToString().Substring(0, PropertyUpperLimitAnnotation.SingleOrDefault().TaxonomyObject.ToString().IndexOf('^')));
                                }
                                string nextPropertyObject = CharacterFactPredicateEntry.TaxonomyObject.ToString();
                                if (!nextPropertyObject.Contains("float"))
                                {
                                    nextPropertyObject = nextPropertyObject.Substring(0, nextPropertyObject.IndexOf('^'));
                                    if (nextPropertyObject.Contains(','))
                                    {
                                        CurrentValue = nextPropertyObject.Split(',').ElementAtOrDefault(0);
                                        SubjectRef = CurrentValue;
                                        continue;
                                    }
                                    else
                                    {
                                        CurrentValue = nextPropertyObject;
                                        SubjectRef = CurrentValue;
                                        continue;
                                    }
                                }

                                else
                                {
                                    CurrentValue = nextPropertyObject.Substring(0, nextPropertyObject.IndexOf('^'));
                                    SubjectRef = CurrentValue;
                                    continue;
                                }
                                                                
                            }
                        }
                        else
                        {
                            bool firstTime = true;
                            while (CheckObjectProperty(nextProperty))
                            {
                                if (firstTime)
                                {
                                    if (!CheckIndividual(itemName))
                                        SubjectFact = CreateIndividual(itemName);
                                    else
                                        SubjectFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + itemName);
                                    firstTime = false;
                                }

                                if (!CheckObjectProperty(element))
                                    objectPredicate = CreateObjectProperty(element);
                                else
                                    objectPredicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentGameContext + element) as RDFOntologyObjectProperty;

                                RDFOntologyTaxonomy ItemFactAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);
                                RDFOntologyTaxonomyEntry ItemFactPredicateEntry = ItemFactAssertions.Where(assertion => assertion.TaxonomyPredicate == objectPredicate).SingleOrDefault();
                                SubjectFact = ItemFactPredicateEntry.TaxonomyObject as RDFOntologyFact;
                                ++nextPropertyCounter;
                                nextProperty = expression.ElementAtOrDefault(index + nextPropertyCounter);
                            }
                            currentPropertyName = nextProperty;
                        }

                        if (CheckDatatypeProperty(currentPropertyName, false))
                        {
                            RDFOntologyDatatypeProperty currentProperty;
                            string characterClassName = GetClass(CurrentCharacterName, true);
                            string nextElementFirstWord = currentPropertyName.Split('_').ToList().First();
                            if (characterClassName.Contains(nextElementFirstWord))
                                SubjectFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);
                            else
                                SubjectFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + SubjectRef);

                            if (!CheckDatatypeProperty(currentPropertyName))
                                currentProperty = CreateDatatypeProperty(currentPropertyName);
                            else
                                currentProperty = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + currentPropertyName) as RDFOntologyDatatypeProperty;
                            RDFOntologyTaxonomy subjectFactAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);
                            RDFOntologyTaxonomyEntry subjectFactPropertyEntry = subjectFactAssertions.Where(entry => entry.TaxonomyPredicate == currentProperty).SingleOrDefault();
                            if (subjectFactPropertyEntry == null)
                            {
                                CurrentValue = 0.ToString();
                                SubjectRef = CurrentValue;
                            }
                            else
                            {
                                string nextPropertyObject = subjectFactPropertyEntry.TaxonomyObject.ToString();
                                if (!nextPropertyObject.Contains("float"))
                                {
                                    nextPropertyObject = nextPropertyObject.Substring(0, nextPropertyObject.IndexOf('^'));
                                    if (nextPropertyObject.Contains(','))
                                        CurrentValue = nextPropertyObject.Split(',').ElementAtOrDefault(0);
                                    else
                                        CurrentValue = nextPropertyObject;
                                    SubjectRef = CurrentValue;
                                }

                                else
                                {
                                    CurrentValue = nextPropertyObject.Substring(0, nextPropertyObject.IndexOf('^'));
                                    SubjectRef = CurrentValue;
                                }
                            }
                        }
                    }
                }
                else if (CheckDatatypeProperty(element, false))
                {
                    if (index == 0)
                    {
                        RDFOntologyFact CharacterFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + CurrentCharacterName);
                        RDFOntologyDatatypeProperty predicate;
                        if (!CheckDatatypeProperty(element))
                            predicate = CreateDatatypeProperty(element);
                        else
                            predicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + element) as RDFOntologyDatatypeProperty;
                        RDFOntologyTaxonomy CharacterAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                        RDFOntologyTaxonomyEntry CharacterPredicateEntry = CharacterAssertions.Where(entry => entry.TaxonomyPredicate == predicate).SingleOrDefault();
                        if (CharacterPredicateEntry == null)
                        {
                            RDFOntologyDatatypeProperty GamePredicate = GameOntology.Model.PropertyModel.SelectProperty(CurrentGameContext + element) as RDFOntologyDatatypeProperty;
                            RDFOntologyTaxonomyEntry PredicateDefaultValueEntry = GameOntology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(GamePredicate).SingleOrDefault();
                            if(PredicateDefaultValueEntry != null)
                            {
                                string PredicateDefaultValueDefinition = PredicateDefaultValueEntry.TaxonomyObject.ToString();
                                string valuetype = PredicateDefaultValueDefinition.Substring(PredicateDefaultValueDefinition.LastIndexOf('#') + 1);
                                PredicateDefaultValueDefinition = PredicateDefaultValueDefinition.Substring(0, PredicateDefaultValueDefinition.IndexOf('^'));
                                string predicateValue = GetValue(PredicateDefaultValueDefinition).ToString();
                                AddDatatypeProperty(CurrentCharacterContext + CurrentCharacterName, predicate.ToString(), predicateValue, valuetype);
                                SaveCharacter();
                                CharacterAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                                CharacterPredicateEntry = CharacterAssertions.Where(entry => entry.TaxonomyPredicate == predicate).SingleOrDefault();
                            }
                            else
                            {
                                string valuetype = predicate.Range.ToString().Substring(predicate.Range.ToString().LastIndexOf('#')+1);
                                AddDatatypeProperty(CurrentCharacterContext + CurrentCharacterName, predicate.ToString(), User_Input.ToString(), valuetype);
                                SaveCharacter();
                                CharacterAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                                CharacterPredicateEntry = CharacterAssertions.Where(entry => entry.TaxonomyPredicate == predicate).SingleOrDefault();
                            }
                            
                        }

                        RDFOntologyTaxonomy PropertyAnnotations = GameOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(predicate);
                        IEnumerable<RDFOntologyTaxonomyEntry> PropertyUpperLimitAnnotation = PropertyAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("hasUpperLimit"));
                        if (PropertyUpperLimitAnnotation.Count() > 0)
                        {
                            hasUpperLimit = true;
                            UpperLimit = Convert.ToSingle(PropertyUpperLimitAnnotation.SingleOrDefault().TaxonomyObject.ToString().Substring(0, PropertyUpperLimitAnnotation.SingleOrDefault().TaxonomyObject.ToString().IndexOf('^')));
                        }
                        else
                        {
                            string CurrentValueString = CharacterPredicateEntry.TaxonomyObject.ToString();
                            if (!CurrentValueString.Contains("float"))
                            {
                                CurrentValueString = CurrentValueString.Substring(0, CurrentValueString.IndexOf('^'));
                                if (CurrentValueString.Contains(','))
                                    CurrentValue = CurrentValueString.Split(',').ElementAtOrDefault(0);
                                else
                                    CurrentValue = CurrentValueString;
                                SubjectRef = CurrentValue;
                            }
                            else
                            {
                                CurrentValue = CurrentValueString.Substring(0, CurrentValueString.IndexOf('^'));
                                SubjectRef = CurrentValue;
                            }
                        }                            
                    }
                    else 
                    {
                        string previousElement = expression.ElementAtOrDefault(index - 1);
                        if(CheckIndividual(element, false))
                        {
                            RDFOntologyFact subjectFact;
                            if (!CheckIndividual(element))
                                subjectFact = CreateIndividual(element);
                            else
                                subjectFact = CharacterOntology.Data.SelectFact(CurrentCharacterContext + element);

                            RDFOntologyDatatypeProperty predicate;
                            if (!CheckDatatypeProperty(element))
                                predicate = CreateDatatypeProperty(element);
                            else
                                predicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + element) as RDFOntologyDatatypeProperty;

                            RDFOntologyTaxonomy SubjectFactPredicateAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(subjectFact);
                            SubjectFactPredicateAssertions = SubjectFactPredicateAssertions.SelectEntriesByPredicate(predicate);

                            if(SubjectFactPredicateAssertions.Count() > 0)
                            {
                                RDFOntologyTaxonomyEntry entry = SubjectFactPredicateAssertions.SingleOrDefault();

                                RDFOntologyTaxonomy PropertyAnnotations = GameOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(predicate);
                                IEnumerable<RDFOntologyTaxonomyEntry> PropertyUpperLimitAnnotation = PropertyAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("hasUpperLimit"));
                                if (PropertyUpperLimitAnnotation.Count() > 0)
                                {
                                    string UpperLimitValue = PropertyUpperLimitAnnotation.SingleOrDefault().TaxonomyObject.ToString().Substring(0, PropertyUpperLimitAnnotation.SingleOrDefault().TaxonomyObject.ToString().IndexOf('^'));
                                    string PropertyObjectValue = entry.TaxonomyObject.ToString().Substring(0, entry.TaxonomyObject.ToString().IndexOf('^'));

                                    dynamic limitResult = ConvertToOperator("<", Convert.ToSingle(PropertyObjectValue), Convert.ToSingle(UpperLimitValue));
                                    if (limitResult.GetType().ToString().Contains("boolean"))
                                    {
                                        if (limitResult == true)
                                        {
                                            CurrentValue = PropertyObjectValue;
                                            SubjectRef = CurrentValue;
                                        }
                                        else
                                        {
                                            CurrentValue = UpperLimitValue;
                                            SubjectRef = CurrentValue;
                                        }
                                    }
                                }
                                else
                                {
                                    string CurrentValueString = entry.TaxonomyObject.ToString();
                                    if (!CurrentValueString.Contains("float"))
                                    {
                                        CurrentValueString = CurrentValueString.Substring(0, CurrentValueString.IndexOf('^'));
                                        if (CurrentValueString.Contains(','))
                                            CurrentValue = CurrentValueString.Split(',').ElementAtOrDefault(0);
                                        else
                                            CurrentValue = CurrentValueString;
                                        SubjectRef = CurrentValue;
                                    }
                                    else
                                    {
                                        CurrentValue = CurrentValueString.Substring(0, CurrentValueString.IndexOf('^'));
                                        SubjectRef = CurrentValue;
                                    }
                                }
                            }
                            else
                            {
                                CurrentValue = 0.ToString();
                                SubjectRef = CurrentValue;
                            }
                        }                        
                    }
                }
            }
            if (CurrentValue == null)
                CurrentValue = "0";
            else if(CurrentValue.Contains("^"))
                CurrentValue = CurrentValue.Substring(0, CurrentValue.IndexOf('^'));

            if(hasUpperLimit == true)
            {
                dynamic result = ConvertToOperator("<", Convert.ToSingle(CurrentValue), UpperLimit);
                if (result == false)
                    CurrentValue = UpperLimit.ToString();
            }

            return Convert.ToSingle(CurrentValue);
        }

        /// <summary>
        /// Returns a collection of groups if given class has subclasses.
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>

        internal ObservableCollection<Group> GetSubClasses(string className, bool applyOnCharacter = false)
        {
            ObservableCollection<Group> subclasses = new ObservableCollection<Group>();

            if (applyOnCharacter)
            {
                RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;
                RDFOntologyClass RootClass = CharacterClassModel.SelectClass(CurrentGameContext + className);
                RDFOntologyTaxonomy SubClassesOfRoot = CharacterClassModel.Relations.SubClassOf.SelectEntriesByObject(RootClass);

                if (SubClassesOfRoot.EntriesCount != 0)
                    foreach (RDFOntologyTaxonomyEntry currentEntry in SubClassesOfRoot)
                    {
                        string groupName = currentEntry.TaxonomySubject.ToString().Substring(currentEntry.TaxonomySubject.ToString().LastIndexOf('#') + 1);
                        RDFOntologyClass currentClass = CharacterClassModel.SelectClass(CurrentGameContext + groupName);
                        RDFOntologyTaxonomy UpperClassesOfCurrent = CharacterClassModel.Relations.SubClassOf.SelectEntriesBySubject(currentClass);

                        foreach (RDFOntologyTaxonomyEntry currentUpperClassEntry in UpperClassesOfCurrent)
                        {
                            string upperClass = currentUpperClassEntry.TaxonomyObject.ToString().Substring(currentUpperClassEntry.TaxonomyObject.ToString().LastIndexOf('#') + 1);
                            if (CharacterClassModel.SelectClass(CurrentCharacterContext + upperClass) == RootClass)
                                subclasses.Add(new Group(groupName));
                        }
                    }
                else
                    subclasses = null;
            }
            else
            {
                RDFOntologyClassModel GameClassModel = GameOntology.Model.ClassModel;
                RDFOntologyClass RootClass = GameClassModel.SelectClass(CurrentGameContext + className);
                RDFOntologyTaxonomy SubClassesOfRoot = GameClassModel.Relations.SubClassOf.SelectEntriesByObject(RootClass);

                if (SubClassesOfRoot.EntriesCount != 0)
                    foreach (RDFOntologyTaxonomyEntry currentEntry in SubClassesOfRoot)
                    {
                        string groupName = currentEntry.TaxonomySubject.ToString().Substring(currentEntry.TaxonomySubject.ToString().LastIndexOf('#') + 1);
                        RDFOntologyClass currentClass = GameClassModel.SelectClass(CurrentGameContext + groupName);
                        RDFOntologyTaxonomyEntry UpperClassesOfCurrent = GameClassModel.Relations.SubClassOf.SelectEntriesBySubject(currentClass).SingleOrDefault();

                        string upperClass = UpperClassesOfCurrent.TaxonomyObject.ToString().Substring(UpperClassesOfCurrent.TaxonomyObject.ToString().LastIndexOf('#') + 1);
                        if (GameClassModel.SelectClass(CurrentGameContext + upperClass) == RootClass)
                            subclasses.Add(new Group(groupName));
                    }
                else
                    subclasses = null;
            }
            return subclasses;
        }

        internal string GetDatatypeUri(string datatypeName)
        {
            var ReturnType = datatypeName switch
            {
                "XMLLiteral" => RDFVocabulary.RDF.XML_LITERAL.URI.ToString(),
                "string" => RDFVocabulary.XSD.STRING.URI.ToString(),
                "boolean" => RDFVocabulary.XSD.BOOLEAN.URI.ToString(),
                "decimal" => RDFVocabulary.XSD.DECIMAL.URI.ToString(),
                "float" => RDFVocabulary.XSD.FLOAT.URI.ToString(),
                "double" => RDFVocabulary.XSD.DOUBLE.URI.ToString(),
                "positiveInteger" => RDFVocabulary.XSD.POSITIVE_INTEGER.URI.ToString(),
                "negativeInteger" => RDFVocabulary.XSD.NEGATIVE_INTEGER.URI.ToString(),
                "nonPositiveInteger" => RDFVocabulary.XSD.NON_POSITIVE_INTEGER.URI.ToString(),
                "nonNegativeInteger" => RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.URI.ToString(),
                "integer" => RDFVocabulary.XSD.INTEGER.URI.ToString(),
                "long" => RDFVocabulary.XSD.LONG.URI.ToString(),
                "int" => RDFVocabulary.XSD.INT.URI.ToString(),
                "short" => RDFVocabulary.XSD.SHORT.URI.ToString(),
                "byte" => RDFVocabulary.XSD.BYTE.URI.ToString(),
                "unsignedLong" => RDFVocabulary.XSD.UNSIGNED_LONG.URI.ToString(),
                "unsignedShort" => RDFVocabulary.XSD.UNSIGNED_SHORT.URI.ToString(),
                "unsignedByte" => RDFVocabulary.XSD.UNSIGNED_BYTE.URI.ToString(),
                "unsignedInt" => RDFVocabulary.XSD.UNSIGNED_INT.URI.ToString(),
                "duration" => RDFVocabulary.XSD.DURATION.URI.ToString(),
                "dateTime" => RDFVocabulary.XSD.DATETIME.URI.ToString(),
                "date" => RDFVocabulary.XSD.DATE.URI.ToString(),
                "time" => RDFVocabulary.XSD.TIME.URI.ToString(),
                "gYear" => RDFVocabulary.XSD.G_YEAR.URI.ToString(),
                "gMonth" => RDFVocabulary.XSD.G_MONTH.URI.ToString(),
                "gDay" => RDFVocabulary.XSD.G_DAY.URI.ToString(),
                "gYearMonth" => RDFVocabulary.XSD.G_YEAR_MONTH.URI.ToString(),
                "gMonthDay" => RDFVocabulary.XSD.G_MONTH_DAY.URI.ToString(),
                "hexBinary" => RDFVocabulary.XSD.HEX_BINARY.URI.ToString(),
                "base64Binary" => RDFVocabulary.XSD.BASE64_BINARY.URI.ToString(),
                "anyURI" => RDFVocabulary.XSD.ANY_URI.URI.ToString(),
                "QName" => RDFVocabulary.XSD.QNAME.URI.ToString(),
                "notation" => RDFVocabulary.XSD.NOTATION.URI.ToString(),
                "language" => RDFVocabulary.XSD.LANGUAGE.URI.ToString(),
                "normalizedString" => RDFVocabulary.XSD.NORMALIZED_STRING.URI.ToString(),
                "token" => RDFVocabulary.XSD.TOKEN.URI.ToString(),
                "NMToken" => RDFVocabulary.XSD.NMTOKEN.URI.ToString(),
                "name" => RDFVocabulary.XSD.NAME.URI.ToString(),
                "NCName" => RDFVocabulary.XSD.NCNAME.URI.ToString(),
                "ID" => RDFVocabulary.XSD.ID.URI.ToString(),
                _ => RDFVocabulary.RDFS.LITERAL.URI.ToString()
            };
            return ReturnType;
        }

        /// <summary>
        /// Shows in console the information tree of the current character
        /// </summary>
        internal void ShowCharacterFile()
        {
            SortedDictionary<string, Group> CharacterGroups = new SortedDictionary<string, Group>();
            SortedSet<RDFOntologyClass> CharacterClasses = new SortedSet<RDFOntologyClass>();
            SortedSet<RDFOntologyClass> CharacterSubclasses = new SortedSet<RDFOntologyClass>();

            RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;
            RDFOntologyData CharacterDataModel = CharacterOntology.Data;
            RDFOntologyTaxonomy Classes = CharacterDataModel.Relations.ClassType;
            foreach (RDFOntologyTaxonomyEntry entry in Classes)
            {
                RDFOntologyClass currentClass = CharacterClassModel.SelectClass(entry.TaxonomySubject.ToString());
                CharacterClasses.Add(currentClass);
            }

            RDFOntologyTaxonomy Subclasses = CharacterClassModel.Relations.SubClassOf;
            foreach (RDFOntologyTaxonomyEntry entry in Subclasses)
            {
                RDFOntologyClass currentClass = CharacterClassModel.SelectClass(entry.TaxonomySubject.ToString());
                CharacterSubclasses.Add(currentClass);
            }

            foreach (RDFOntologyClass item in CharacterClasses)
                if (!CharacterSubclasses.Contains(item))
                    CharacterClasses.Remove(item);

            //Para cada elemento en RootClasses, crear un grupo con el nombre de la clase
            foreach (RDFOntologyClass item in CharacterClasses)
            {
                string groupName = item.ToString().Substring(item.ToString().LastIndexOf('#') + 1);
                CharacterGroups.Add(groupName, new Group(groupName));
            }

            foreach (Group group in CharacterGroups.Values)
                group.ShowGroup();
        }

        #endregion

        #region Misc
        /// <summary>
        /// Returns the result of "value1" -> "op" -> "value2"
        /// </summary>
        /// <param name="op"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        internal static dynamic ConvertToOperator(string op, float value1, float value2)
        {
            return op switch
            {
                "+" => value1 + value2,
                "-" => value1 - value2,
                "*" => value1 * value2,
                "/" => value1 / value2,
                "%" => value1 % value2,
                "<" => value1 < value2,
                ">" => value1 > value2,
                "<=" => value1 <= value2,
                ">=" => value1 >= value2,
                "==" => value1 == value2,
                "!=" => value1 != value2,
                _ => throw new ArgumentException("op"),
            };
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes datatype property assertions from the character given the predicate. Is optional to give a specific value.
        /// </summary>
        /// <param name="predicateName">Name of the predicate</param>
        /// <param name="literal">Value of the property</param>
        internal void RemoveDatatypeProperty(string predicateName, string literal = null)
        {
            RDFOntologyDatatypeProperty predicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + predicateName) as RDFOntologyDatatypeProperty;
            RDFOntologyTaxonomy CharacterPredicateAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate);
            if(literal == null)
            {
                foreach(RDFOntologyTaxonomyEntry entry in CharacterPredicateAssertions)
                {
                    RDFOntologyFact entrySubject = entry.TaxonomySubject as RDFOntologyFact;
                    RDFOntologyLiteral entryLiteral = entry.TaxonomyObject as RDFOntologyLiteral;
                    CharacterOntology.Data.RemoveAssertionRelation(entrySubject, predicate, entryLiteral);
                    SaveCharacter();
                }
            }
            else
            {
                RDFOntologyLiteral entryLiteral = CharacterOntology.Data.SelectLiteral(literal);
                RDFOntologyTaxonomy entries = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SelectEntriesByObject(entryLiteral);
                foreach(RDFOntologyTaxonomyEntry entry in entries)
                {
                    RDFOntologyFact entrySubject = entry.TaxonomySubject as RDFOntologyFact;
                    CharacterOntology.Data.RemoveAssertionRelation(entrySubject, predicate, entryLiteral);
                    SaveCharacter();
                }
            }
            SaveCharacter();
        }

        /// <summary>
        /// Removes object property assertions from the character given the predicate. Is optional to give a specific object.
        /// </summary>
        /// <param name="predicateName"></param>
        /// <param name="objectFactName"></param>
        internal void RemoveObjectProperty(string predicateName, string objectFactName = null)
        {
            RDFOntologyObjectProperty predicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + predicateName) as RDFOntologyObjectProperty;
            RDFOntologyTaxonomy CharacterPredicateAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate);
            if (objectFactName == null)
            {
                foreach (RDFOntologyTaxonomyEntry entry in CharacterPredicateAssertions)
                {
                    RDFOntologyFact entrySubject = entry.TaxonomySubject as RDFOntologyFact;
                    RDFOntologyFact entryObject = entry.TaxonomyObject as RDFOntologyFact;
                    CharacterOntology.Data.RemoveAssertionRelation(entrySubject, predicate, entryObject);
                    SaveCharacter();
                }
            }
            else
            {
                RDFOntologyFact entryObject = CharacterOntology.Data.SelectFact(CurrentCharacterContext + objectFactName);
                RDFOntologyTaxonomyEntry entry = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SelectEntriesByObject(entryObject).SingleOrDefault();
                RDFOntologyFact entrySubject = entry.TaxonomySubject as RDFOntologyFact;
                CharacterOntology.Data.RemoveAssertionRelation(entrySubject, predicate, entryObject);
                SaveCharacter();
            }
            SaveCharacter();
        }
        #endregion
        #region Save
        /// <summary>
        /// Saves current character info
        /// </summary>
        internal void SaveCharacter()
        {
            RDFGraph CharacterGraph = CharacterOntology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData);
            CharacterGraph.ToFile(RdfFormat, CurrentCharacterFile);
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects a game from the app
        /// </summary>
        /// <param name="name">Name of the game</param>
        /// <param name="version">Version of the game</param>
        internal void SelectGame(string name, string version)
        {
            CurrentGameName = name.Replace(' ', '_');
            CurrentGameFile = GameFolder +"/"+ name + "/" + version + ".owl";
            CurrentGameContext = "http://ARPEGOS_Project/Games/" + name +"#";
            string CurrentGamePrefix = string.Concat(Regex.Matches(name, "[A-Z]").OfType<Match>().Select(match => match.Value)).ToLower();

            RDFGraph GameGraph = RDFGraph.FromFile(RdfFormat, CurrentGameFile);
            CurrentGameContext = GameGraph.Context.ToString() + '#';
            GameGraph.SetContext(new Uri(CurrentGameContext));
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(CurrentGamePrefix, GameGraph.Context.ToString()));
            GameOntology = RDFOntology.FromRDFGraph(GameGraph);
        }

        /// <summary>
        /// Selects an existing character from the current game
        /// </summary>
        /// <param name="name">Name of the character</param>
        internal void SelectCharacter(string name)
        {
            name = Text.ToTitleCase(name.Replace(" ","_"));
            CurrentCharacterName = name;
            CurrentCharacterFile = Path.Combine(CharacterFolder,CurrentGameName, name + ".owl");
            CurrentCharacterContext = "http://ARPEGOS_Project/Games/" + CurrentGameName + "/characters/" + name + "#";

            RDFGraph CharacterGraph = new RDFGraph();
            CharacterGraph.SetContext(new Uri(CurrentCharacterContext));
            CharacterOntology = RDFOntology.FromRDFGraph(CharacterGraph);
        }
        #endregion

        #region Update

        /// <summary>
        /// Update available points limit given the element name
        /// </summary>
        /// <param name="stage">Element which contains the limit</param>
        /// <param name="update">Updated value of the limit</param>
        internal bool UpdateAvailablePoints(string stage, float? update)
        {
            bool hasUpdated = false;
            List<string> AvailableWords = new List<string>()
            {
                "Disponible",
                "Available"
            };

            RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;

            IEnumerable<RDFOntologyProperty> ResultProperties = GamePropertyModel.Where(entry => AvailableWords.Any(word => entry.ToString().Contains(word)));
            List<string> ElementWords = stage.Split('_').ToList();
            List<string> CompareList = new List<string>();
            int index = 0;
            int FilterResultsCounter = ResultProperties.Count();

            while (FilterResultsCounter > 1)
            {
                CompareList.Add(ElementWords.ElementAtOrDefault(index));
                ResultProperties = ResultProperties.Where(entry => CompareList.All(word => entry.ToString().Contains(word)));
                FilterResultsCounter = ResultProperties.Count();
                ++index;
            }

            if (FilterResultsCounter > 0)
            {
                RDFOntologyProperty LimitProperty = ResultProperties.SingleOrDefault();
                string propertyName = LimitProperty.ToString().Substring(LimitProperty.ToString().LastIndexOf('#') + 1);
                UpdateDatatypeAssertion(propertyName, update.ToString());
            }
            else
            {
                string parents = GetParentClasses(stage);
                if(parents != null)
                {
                    List<string> parentList = parents.Split(':').ToList();
                    foreach (string parent in parentList)
                    {
                        if (hasUpdated == false)
                            hasUpdated = UpdateAvailablePoints(parent, update);
                    }
                }                
            }
            SaveCharacter();
            return hasUpdated;
        }

        /// <summary>
        /// Updates a datatype assertion in character given the predicate and the new value.
        /// </summary>
        /// <param name="predicateName">Name of the predicate</param>
        /// <param name="value">New value of the assertion</param>
        internal void UpdateDatatypeAssertion(string predicateName, string value)
        {
            bool hasPredicate = CheckDatatypeProperty(predicateName);
            RDFOntologyDatatypeProperty predicate;
            if (hasPredicate == false)
                predicate = CreateDatatypeProperty(predicateName);
            else
                predicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + predicateName) as RDFOntologyDatatypeProperty;

            string subject, valuetype;

            if (hasPredicate == true)
            {
                RDFOntologyTaxonomyEntry CharacterPredicateAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SingleOrDefault();
                if(CharacterPredicateAssertions != null)
                {
                    valuetype = CharacterPredicateAssertions.TaxonomyObject.ToString().Substring(CharacterPredicateAssertions.TaxonomyObject.ToString().LastIndexOf('#') + 1);
                    subject = CharacterPredicateAssertions.TaxonomySubject.ToString();
                    RemoveDatatypeProperty(predicateName);
                    SaveCharacter();
                }
                else
                {
                    subject = CurrentCharacterContext + CurrentCharacterName;
                    valuetype = predicate.Range.Value.ToString().Substring(predicate.Range.Value.ToString().LastIndexOf('#') + 1);
                }
            }
            else
            {
                subject = CurrentCharacterContext + CurrentCharacterName;
                valuetype = predicate.Range.Value.ToString().Substring(predicate.Range.Value.ToString().LastIndexOf('#') + 1);
            }
            AddDatatypeProperty(subject, CurrentCharacterContext + predicateName, value, valuetype);
            SaveCharacter();
        }

        /// <summary>
        /// Updates an object assertion in character given the predicate and the new object.
        /// </summary>
        /// <param name="predicateName">>Name of the predicate</param>
        /// <param name="objectName">New object of the assertion</param>
        internal void UpdateObjectAssertion(string predicateName, string objectName)
        {
            bool hasPredicate = CheckObjectProperty(predicateName);
            RDFOntologyObjectProperty predicate;
            if (hasPredicate == false)
                predicate = CreateObjectProperty(predicateName);
            else
                predicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + predicateName) as RDFOntologyObjectProperty;

            string subject;

            if (hasPredicate == true)
            {
                RDFOntologyTaxonomyEntry CharacterPredicateAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SingleOrDefault();
                if(CharacterPredicateAssertions != null)
                {
                    subject = CharacterPredicateAssertions.TaxonomySubject.ToString();
                    RemoveDatatypeProperty(predicateName);
                    SaveCharacter();
                }
                else
                    subject = CurrentCharacterContext + CurrentCharacterName;
            }
            else
                subject = CurrentCharacterContext + CurrentCharacterName;
            AddObjectProperty(subject, CurrentCharacterContext + predicateName, CurrentCharacterContext + objectName);
            SaveCharacter();
        }

        internal bool UpdateLimit(string ElementName, float? update)
        {
            bool hasUpdated = false;
            List<string> LimitWords = new List<string>()
            {
                "Límite",
                "Limit",
                "Limite"
            };

            RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;

            IEnumerable<RDFOntologyProperty> ResultProperties = GamePropertyModel.Where(entry => LimitWords.Any(word => entry.ToString().Contains(word)));
            List<string> ElementWords = ElementName.Split('_').ToList();
            List<string> CompareList = new List<string>();
            int index = 0;
            int FilterResultsCounter = ResultProperties.Count();

            while (FilterResultsCounter > 1)
            {
                CompareList.Add(ElementWords.ElementAtOrDefault(index));
                ResultProperties = ResultProperties.Where(entry => CompareList.All(word => entry.ToString().Contains(word)));
                FilterResultsCounter = ResultProperties.Count();
                ++index;
            }

            if (FilterResultsCounter > 0)
            {
                RDFOntologyDatatypeProperty LimitProperty = ResultProperties.SingleOrDefault() as RDFOntologyDatatypeProperty;
                string propertyName = LimitProperty.ToString().Substring(LimitProperty.ToString().LastIndexOf('#') + 1);
                UpdateDatatypeAssertion(propertyName, update.ToString());
                hasUpdated = true;
            }
            else
            {
                string parents = GetParentClasses(ElementName);
                if (parents != null)
                {
                    List<string> parentList = parents.Split(':').ToList();
                    foreach (string parent in parentList)
                    {
                        if (hasUpdated == false)
                            hasUpdated = UpdateLimit(parent, update);
                    }
                }
            }
            SaveCharacter();
            return hasUpdated;
        }
        #endregion

        #endregion
    }
}

