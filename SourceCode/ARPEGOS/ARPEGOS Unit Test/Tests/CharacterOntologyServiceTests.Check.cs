using ARPEGOS.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARPEGOS_Unit_Test.Tests
{
    [TestFixture]
    public partial class CharacterOntologyServiceTests
    {


        #region Check Tests
        [Test]
        [TestCase("10", "unsignedInt", true, ExpectedResult = true)]
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
            string context = applyOnCharacter ? Character.Context : Game.Context;
            string stageString = $"{context}{Character.GetString(FileService.EscapedName(stageName), applyOnCharacter)}";
            if (Character.CheckClass(stageString, applyOnCharacter))
                generalCost = Character.CheckGeneralCost(FileService.EscapedName(stageName), applyOnCharacter);
            return generalCost;
        }

        [Test]
        [TestCase("Habilidad Combate General", false, ExpectedResult = "Per_Item_PD, numerical: true, User_Edit: true\n" +
                                                                       "Per_Item_Base, numerical: true, User_Edit: false, Per_Item_PD: /: tieneCategoría :Cat_Item_Coste\n" +
                                                                       "Per_Item_Bono_Característica, numerical: true, User_Edit: false, Hab_CarPrincipal: Per_Ref_Bono\n" +
                                                                       "Per_Item_Bono_Categoría, numerical: true, User_Edit: false, tieneCategoría: Cat_BI_Item: *: Per_Nivel\n" +
                                                                       "Per_Item_Total, numerical: true, User_Edit: false, Per_Item_Base: +: Per_Item_Bono_Característica: +: Per_Item_Bono_Categoría")]
        public string CheckValueListInfo_Test(string stageName, bool applyOnCharacter)
        {
            var context = applyOnCharacter ? Character.Context : Game.Context;
            return Character.CheckValueListInfo($"{context}{FileService.EscapedName(stageName)}", applyOnCharacter);
        }

        [Test]
        [TestCase("Libro Luz", true, "Per_Disponible_PD", 1000, "Per_Disponible_Habilidad_Mística", 1000, null, ExpectedResult = true)]
        public bool CheckAvailableOptions_Test(string stageName, bool hasGeneralLimitValue, string GeneralLimitName, double generalLimitValue, string StageLimitName, double partialLimitValue, string groupName = null)
        {
            var gotExpectedOptions = true;
            var expectedItems = new List<string> 
            { 
                "Armadura De Luz",
                "Ascensión",
                "Bendición",
                "Creación De Luz",
                "Crear Luz",
                "Crear Sentimientos Positivos",
                "Cuerpo A Luz",
                "Descarga De Luz",
                "Destrucción De Sombras",
                "Destruir Sentimientos Negativos",
                "Detectar Lo Negativo",
                "Detectar Vida",
                "Dominio Lumínico",
                "Encontrar",
                "Escudar Contra Lo Negativo",
                "Escudo Luz",
                "Esencia De Luz",
                "Esfera Buscadora",
                "Espía De Luz",
                "Esquema Hipnótico",
                "Flash Cegador",
                "Holocausto De Luz",
                "Holograma",
                "Imbuir Calma",
                "Introducirse En Los Sueños",
                "Lazos De Luz",
                "Luz Catastrófica",
                "Luz Sanadora",
                "Objetos Luminosos Materiales",
                "Omnisciencia Radial",
                "Percibir",
                "Predecir",
                "Prisión De Luz",
                "Prisma Reflectante",
                "Restituir",
                "Señor De Los Sueños",
                "Transmisión Por Luz",
                "Ver Realmente",
                "Zona De Detección",
                "Éxtasis"
            };
            var stageString = $"{Game.Context}{FileService.EscapedName(stageName)}";
            var itemList = Character.CheckAvailableOptions(stageString, hasGeneralLimitValue, GeneralLimitName, generalLimitValue, StageLimitName, partialLimitValue, groupName);
            var itemNameList = new List<string>();
            foreach (var item in itemList)
                if (!itemNameList.Contains(item.ShortName))
                    itemNameList.Add(FileService.FormatName(item.ShortName));

            expectedItems.Sort();
            itemNameList.Sort();

            var notSharedItems = expectedItems.Except(itemNameList);
            if (notSharedItems.Any())
                gotExpectedOptions = false;
            return gotExpectedOptions;
        }

        #endregion
    }
}
