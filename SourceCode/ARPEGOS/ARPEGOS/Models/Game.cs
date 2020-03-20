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
            RDFOntologyFact Fact;

            if (applyOnCharacter)
            {
                RDFOntologyData CharacterDataModel = CharacterOntology.Data;
                Fact = new RDFOntologyFact(new RDFResource(CurrentCharacterContext + name));
                if (CharacterDataModel.SelectFact(Fact.ToString()) != null)
                    FactExists = true;
            }
            else
            {
                RDFOntologyData GameDataModel = GameOntology.Data;
                Fact = new RDFOntologyFact(new RDFResource(CurrentGameContext + name));
                if (GameDataModel.SelectFact(Fact.ToString()) != null)
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
            RDFOntologyClass Class;

            if (applyOnCharacter)
            {
                RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;
                Class = new RDFOntologyClass(new RDFResource(CurrentCharacterContext + name));
                if (CharacterClassModel.SelectClass(Class.ToString()) != null)
                    ClassExists = true;
            }
            else
            {
                RDFOntologyClassModel GameClassModel = GameOntology.Model.ClassModel;
                Class = new RDFOntologyClass(new RDFResource(CurrentGameContext + name));
                if (GameClassModel.SelectClass(Class.ToString()) != null)
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
            RDFOntologyObjectProperty Property;

            if (applyOnCharacter)
            {
                RDFOntologyPropertyModel CharacterPropertyModel = CharacterOntology.Model.PropertyModel;
                Property = new RDFOntologyObjectProperty(new RDFResource(CurrentCharacterContext + name));
                if (CharacterPropertyModel.SelectProperty(Property.ToString()) != null)
                    PropertyExists = true;
            }
            else
            {
                RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;
                Property = new RDFOntologyObjectProperty(new RDFResource(CurrentGameContext + name));
                if (GamePropertyModel.SelectProperty(Property.ToString()) != null)
                    PropertyExists = true;
            }
            return PropertyExists;
        }

        /// <summary>
        /// Check if datatype property exists inside the current game or character file
        /// </summary>
        /// <param name="name">Name of the datatype property</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        internal bool CheckDatatypeFromStringProperty(string name, bool applyOnCharacter = true)
        {
            name = name.Replace(" ", "_");
            bool PropertyExists = false;
            RDFOntologyDatatypeProperty Property;

            if (applyOnCharacter)
            {
                RDFOntologyPropertyModel CharacterPropertyModel = CharacterOntology.Model.PropertyModel;
                Property = new RDFOntologyDatatypeProperty(new RDFResource(CurrentCharacterContext + name));
                if (CharacterPropertyModel.SelectProperty(Property.ToString()) != null)
                    PropertyExists = true;
            }
            else
            {
                RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;
                Property = new RDFOntologyDatatypeProperty(new RDFResource(CurrentGameContext + name));
                if (GamePropertyModel.SelectProperty(Property.ToString()) != null)
                    PropertyExists = true;
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
        /// Returns semantic datatype given its name
        /// </summary>
        /// <param name="type">Name of the semantic datatype</param>
        /// <returns></returns>
        internal RDFModelEnums.RDFDatatypes CheckDatatypeFromString(string type)
        {
            RDFModelEnums.RDFDatatypes ReturnType;
            switch (type)
            {
                case "XMLLiteral": ReturnType = RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL; break;
                case "string": ReturnType = RDFModelEnums.RDFDatatypes.XSD_STRING; break;
                case "boolean": ReturnType = RDFModelEnums.RDFDatatypes.XSD_BOOLEAN; break;
                case "decimal": ReturnType = RDFModelEnums.RDFDatatypes.XSD_DECIMAL; break;
                case "float": ReturnType = RDFModelEnums.RDFDatatypes.XSD_FLOAT; break;
                case "double": ReturnType = RDFModelEnums.RDFDatatypes.XSD_DOUBLE; break;
                case "positiveInteger": ReturnType = RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER; break;
                case "negativeInteger": ReturnType = RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER; break;
                case "nonPositiveInteger": ReturnType = RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER; break;
                case "nonNegativeInteger": ReturnType = RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER; break;
                case "integer": ReturnType = RDFModelEnums.RDFDatatypes.XSD_INTEGER; break;
                case "long": ReturnType = RDFModelEnums.RDFDatatypes.XSD_LONG; break;
                case "int": ReturnType = RDFModelEnums.RDFDatatypes.XSD_INT; break;
                case "short": ReturnType = RDFModelEnums.RDFDatatypes.XSD_SHORT; break;
                case "byte": ReturnType = RDFModelEnums.RDFDatatypes.XSD_BYTE; break;
                case "unsignedLong": ReturnType = RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG; break;
                case "unsignedShort": ReturnType = RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT; break;
                case "unsignedByte": ReturnType = RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE; break;
                case "unsignedInt": ReturnType = RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT; break;
                case "duration": ReturnType = RDFModelEnums.RDFDatatypes.XSD_DURATION; break;
                case "dateTime": ReturnType = RDFModelEnums.RDFDatatypes.XSD_DATETIME; break;
                case "date": ReturnType = RDFModelEnums.RDFDatatypes.XSD_DATE; break;
                case "time": ReturnType = RDFModelEnums.RDFDatatypes.XSD_TIME; break;
                case "gYear": ReturnType = RDFModelEnums.RDFDatatypes.XSD_GYEAR; break;
                case "gMonth": ReturnType = RDFModelEnums.RDFDatatypes.XSD_GMONTH; break;
                case "gDay": ReturnType = RDFModelEnums.RDFDatatypes.XSD_GDAY; break;
                case "gYearMonth": ReturnType = RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH; break;
                case "gMonthDay": ReturnType = RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY; break;
                case "hexBinary": ReturnType = RDFModelEnums.RDFDatatypes.XSD_HEXBINARY; break;
                case "base64Binary": ReturnType = RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY; break;
                case "anyURI": ReturnType = RDFModelEnums.RDFDatatypes.XSD_ANYURI; break;
                case "QName": ReturnType = RDFModelEnums.RDFDatatypes.XSD_QNAME; break;
                case "notation": ReturnType = RDFModelEnums.RDFDatatypes.XSD_NOTATION; break;
                case "language": ReturnType = RDFModelEnums.RDFDatatypes.XSD_LANGUAGE; break;
                case "normalizedString": ReturnType = RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING; break;
                case "token": ReturnType = RDFModelEnums.RDFDatatypes.XSD_TOKEN; break;
                case "NMToken": ReturnType = RDFModelEnums.RDFDatatypes.XSD_NMTOKEN; break;
                case "name": ReturnType = RDFModelEnums.RDFDatatypes.XSD_NAME; break;
                case "NCName": ReturnType = RDFModelEnums.RDFDatatypes.XSD_NCNAME; break;
                case "ID": ReturnType = RDFModelEnums.RDFDatatypes.XSD_ID; break;
                default: ReturnType = RDFModelEnums.RDFDatatypes.RDFS_LITERAL; break;
            }
            return ReturnType;
        }

        /// <summary>
        /// Return class given a semantic datatype
        /// </summary>
        /// <param name="datatype">Semantic datatype</param>
        /// <returns></returns>
        internal RDFOntologyClass GetClassFromDatatype(RDFModelEnums.RDFDatatypes datatype)
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

