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
    public class CharacterOntologyServiceTests
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

        #region Add Tests
        [Test]
        [TestCase("Guybrush Threepwood", "tieneCategoría", "Categoría_Novel", ExpectedResult = true)]
        public async Task<bool> AddObjectProperty_Test(string Subject, string Predicate, string Object)
        {
            await Task.Run(() => Character.AddObjectProperty(Subject, Predicate, Object));

            var escapedSubject = FileService.EscapedName(Subject);
            var escapedPredicate = FileService.EscapedName(Predicate);
            var escapedObject = FileService.EscapedName(Object);

            var SubjectFact = Character.Ontology.Data.SelectFact($"{Character.Context}{escapedSubject}");
            var PredicateProperty = Character.Ontology.Model.PropertyModel.SelectProperty($"{Character.Context}{escapedPredicate}");
            var ObjectFact = Character.Ontology.Data.SelectFact($"{Character.Context}{escapedObject}");
            var checkAssertion = false;
            if (SubjectFact != null && PredicateProperty != null && ObjectFact != null)
                checkAssertion = Character.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact)
                                                                                 .SelectEntriesByPredicate(PredicateProperty)
                                                                                 .SelectEntriesByObject(ObjectFact).Count() > 0;
            var annotationPredicate = Character.Ontology.Model.PropertyModel.SelectProperty($"{Character.Context}Visualization");
            var checkClassification = Character.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(PredicateProperty).SelectEntriesByPredicate(annotationPredicate).Single() != null;
            return checkAssertion && checkClassification;
        }

        [Test]
        [TestCase("Guybrush Threepwood", "Per_Nivel", "1", "unsignedInt", ExpectedResult = true)]
        public async Task<bool> AddDatatypeProperty_Test(string Subject, string Predicate, string Value, string ValueType)
        {
            await Task.Run(() => Character.AddDatatypeProperty(Subject, Predicate, Value, ValueType));

            var escapedSubject = FileService.EscapedName(Subject);
            var escapedPredicate = FileService.EscapedName(Predicate);

            var SubjectFact = Character.Ontology.Data.SelectFact($"{Character.Context}{escapedSubject}");
            var PredicateProperty = Character.Ontology.Model.PropertyModel.SelectProperty($"{Character.Context}{escapedPredicate}");
            var checkAssertion = false;
            if (SubjectFact != null && PredicateProperty != null)
            {
                var entry = Character.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact).SelectEntriesByPredicate(PredicateProperty).First();
                var entryLiteral = entry.TaxonomyObject.ToString().Split('^');
                var checkLiteralValue = string.Equals(Value, entryLiteral.First());
                var checkLiteralType = entryLiteral.Last().ToLower().Contains(ValueType.ToLower());
                if (checkLiteralValue && checkLiteralType)
                    checkAssertion = true;
            }

            var annotationPredicate = Character.Ontology.Model.PropertyModel.SelectProperty($"{Character.Context}Visualization");
            var checkClassification = Character.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(PredicateProperty).SelectEntriesByPredicate(annotationPredicate).Single() != null;
            return checkAssertion && checkClassification;
        }

        #endregion

        #region Check Tests
        [Test]
        [TestCase("1", "unsignedInt", true, ExpectedResult = true)]
        [TestCase("-30", "integer", true, ExpectedResult = false)]
        [TestCase("150000", "unsignedInt", false, ExpectedResult = true)]
        [TestCase("Hola, soy Guybrush Threepwood y ¡quiero ser un pirata!", "string", false, ExpectedResult = false)]
        public bool CheckLiteral_Test(string value, string type, bool applyOnCharacter = true)
        {
            return Character.CheckLiteral(value, type, applyOnCharacter);
        }

        [Test]
        [TestCase("Guybrush Threepwood", true, ExpectedResult = true)]
        [TestCase("Pirata", true, ExpectedResult = false)]
        [TestCase("Categoría Novel", false, ExpectedResult = true)]
        [TestCase("Monkey Island", false, ExpectedResult = false)]
        public bool CheckFact_Test(string elementName, bool applyOnCharacter)
        {
            var context = applyOnCharacter ? Character.Context : Game.Context;
            return Character.CheckFact($"{context}{FileService.EscapedName(elementName)}", applyOnCharacter);
        }

        [Test]
        [TestCase("Personaje", true, ExpectedResult = true)]
        [TestCase("Aventura Gráfica", true, ExpectedResult = false)]
        [TestCase("Categoría", false, ExpectedResult = true)]
        [TestCase("Emulador", false, ExpectedResult = false)]
        public bool CheckClass_Test(string elementName, bool applyOnCharacter)
        {
            var context = applyOnCharacter ? Character.Context : Game.Context;
            return Character.CheckClass($"{context}{FileService.EscapedName(elementName)}", applyOnCharacter);
        }

        [Test]
        [TestCase("tieneCategoría", true, ExpectedResult = true)]
        [TestCase("tieneHambre", true, ExpectedResult = false)]
        [TestCase("CategoríaTieneObjeto", false, ExpectedResult = true)]
        [TestCase("CategoríaTienePuntos", false, ExpectedResult = false)]
        public bool CheckObjectProperty_Test(string elementName, bool applyOnCharacter)
        {
            var context = applyOnCharacter ? Character.Context : Game.Context;
            return Character.CheckObjectProperty($"{context}{FileService.EscapedName(elementName)}", applyOnCharacter);
        }

        [Test]
        [TestCase("Per Nivel", true, ExpectedResult = true)]
        [TestCase("Per Lógica", true, ExpectedResult = false)]
        [TestCase("Vía Límite", false, ExpectedResult = true)]
        [TestCase("Duración", false, ExpectedResult = false)]
        public bool CheckDatatypeProperty_Test(string elementName, bool applyOnCharacter)
        {
            var context = applyOnCharacter ? Character.Context : Game.Context;
            return Character.CheckDatatypeProperty($"{context}{FileService.EscapedName(elementName)}", applyOnCharacter);
        }

        [Test]
        [TestCase("Habilidad Combate General", true, ExpectedResult = null)]
        [TestCase("Per Nivel", true, ExpectedResult = null)]
        [TestCase("Habilidad Combate General", false, ExpectedResult = "Hab_Disponible_PD_Coste")]
        [TestCase("Per Nivel", false, ExpectedResult = null)]
        public string CheckGeneralCost_Test(string stageName, bool applyOnCharacter)
        {
            string generalCost = null;
            if (Character.CheckClass(FileService.EscapedName(stageName)))
                generalCost = Character.CheckGeneralCost(FileService.EscapedName(stageName), applyOnCharacter);
            return generalCost;
        }

        [Test]
        [TestCase("Habilidad Combate General", false, ExpectedResult = "Per_Item_PD, numerical: true, User_Edit: true\r\n" +
                                                                       "Per_Item_Base, numerical: true, User_Edit: false, Per_Item_PD: /: tieneCategoría :Cat_Item_Coste\r\n" +
                                                                       "Per_Item_Bono_Característica, numerical: true, User_Edit: false, Hab_CarPrincipal: Per_Ref_Bono\r\n" +
                                                                       "Per_Item_Bono_Categoría, numerical: true, User_Edit: false, tieneCategoría: Cat_BI_Item: *: Per_Nivel\r\n" +
                                                                       "Per_Item_Total, numerical: true, User_Edit: false, Per_Item_Base: +: Per_Item_Bono_Característica: +: Per_Item_Bono_Categoría")]
        public string CheckValueListInfo_Test(string stageName, bool applyOnCharacter)
        {
            var context = applyOnCharacter ? Character.Context : Game.Context;
            return Character.CheckValueListInfo($"{context}{FileService.EscapedName(stageName)}", applyOnCharacter);
        }

        [Test]
        [TestCase("Arte Marcial Básica", true, "Per_Disponible_PD", 1000, "Per_Disponible_Habilidad_Combate", 1000, null, ExpectedResult = true)]
        public bool CheckAvailableOptions_Test(string stageName, bool hasGeneralLimitValue, string GeneralLimitName, double generalLimitValue, string StageLimitName, double partialLimitValue, string groupName = null)
        {
            var expectedItems = new List<string> { "Kempo", "Sambo", "Shotokan", "Tae Kwon Do" };
            var itemList = Character.CheckAvailableOptions($"{Game.Context}{FileService.EscapedName(stageName)}", hasGeneralLimitValue, GeneralLimitName, generalLimitValue, StageLimitName, partialLimitValue, groupName);
            var itemNameList = new List<string>();
            foreach (var item in itemList)
                if(!itemNameList.Contains(item.ShortName))
                    itemNameList.Add(item.ShortName);    
            
            return !expectedItems.Except(itemNameList).ToList().Any() && !itemNameList.Except(expectedItems).ToList().Any();
        }

        #endregion



    }
}
