using ARPEGOS.Helpers;
using ARPEGOS.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDFSharp.Semantics.OWL;
using System.Collections;
using ARPEGOS;
using RDFSharp.Model;
using System.Collections.ObjectModel;

namespace ARPEGOS_Unit_Test.Tests
{
    [TestFixture]
    public partial class CharacterOntologyServiceTests
    {
        [Test]
        [TestCase("Per Nivel" , "General" , ExpectedResult = true)]
        [TestCase("tieneCategoría" , "Categoría" , ExpectedResult = false)]
        public bool GetPropertyVisualizationPosition_Test( string propertyName , string expectedString )
        {
            var gotExpectedString = false;
            var propertyString = $"{Character.Context}{FileService.EscapedName(propertyName)}";
            var result = Character.GetPropertyVisualizationPosition(propertyString);
            if (result.Equals(expectedString))
                gotExpectedString = true;
            return gotExpectedString;
        }

        [Test]
        [TestCase(8,ExpectedResult = true)]
        public bool GetCharacterProperties_Test(int expectedPropertiesCount)
        {
            var properties = Character.GetCharacterProperties();
            /*var propertiesEnumerator = properties.GetEnumerator();
            var expectedPropertiesEnumerator = expectedProperties.GetEnumerator();

            while (propertiesEnumerator.MoveNext() && expectedPropertiesEnumerator.MoveNext())
            {
                var currentProperty = propertiesEnumerator.Current;
                var currentExpectedProperty = expectedPropertiesEnumerator.Current;

                if (currentProperty.Key != currentExpectedProperty.Key)
                    gotExpectedProperties = false;

                var currentPropertySet = new HashSet<string>(currentProperty.Value);
                var currentExpectedPropertySet = new HashSet<string>(currentExpectedProperty.Value);

                if (!currentPropertySet.SetEquals(currentExpectedPropertySet))
                    gotExpectedProperties = false;
            }*/
            return expectedPropertiesCount == properties.Count();
        }

        [Test]
        [TestCase("Kempo" , ExpectedResult = false)]
        [TestCase("Don" , ExpectedResult = false)]
        [TestCase("Ataque" , ExpectedResult = true)]
        [TestCase("Arcano Dificultad" , ExpectedResult = false)]

        public bool GetCharacterSkills_Test( string name )
        {
            var gotExpectedSkills = false;
            var skillName = FileService.EscapedName(name);
            var skillPropertyName = string.Empty;
            var skillPropertyString = string.Empty;
            var skills = Character.GetIndividualsGrouped($"{Game.Context}Habilidad" , false);
            bool skillFound = false;
            foreach (var group in skills)
            {
                if (!skillFound)
                {
                    var items = Character.GetIndividuals(group.GroupString);
                    foreach (var item in items)
                    {
                        if (!skillFound)
                        {
                            if (item.ShortName == skillName)
                            {
                                skillFound = true;
                                if (Character.CheckDatatypeProperty($"{Game.Context}Per_{skillName}" , false))
                                {
                                    skillPropertyName = $"Per_{skillName}";
                                    skillPropertyString = $"{Character.Context}{skillPropertyName}";
                                    break;
                                }

                                var itemClass = $"{Game.Context}{item.Class}";
                                skillPropertyName = Character.GetObjectPropertyAssociated(itemClass , item , false).Split('#').Last();
                                skillPropertyString = $"{Character.Context}{skillPropertyName}";
                                break;
                            }
                        }
                    }
                }
            }
            var skillString = $"{Character.Context}{skillName}";
            var gameSkillString = $"{Game.Context}{skillPropertyName}";
            if (Character.CheckDatatypeProperty(gameSkillString , false))
            {
                var testValue = "10";
                var property = Game.Ontology.Model.PropertyModel.SelectProperty(gameSkillString);
                var testValueType = property.Range.ToString().Split('#').Last();
                Character.AddDatatypeProperty($"{Character.Context}{Character.Name}" , skillPropertyString , testValue , testValueType);

                var characterProperty = Character.Ontology.Model.PropertyModel.SelectProperty(skillPropertyString);
                var characterPropertyCustomAnnotations = Character.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(characterProperty);
                if (characterPropertyCustomAnnotations.EntriesCount > 0)
                {
                    var characterPropertySkillAnnotations = characterPropertyCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("ActiveSkill"));
                    if (characterPropertySkillAnnotations.Count() > 0)
                        gotExpectedSkills = true;
                    else
                        gotExpectedSkills = false;
                }
                Character.RemoveDatatypeProperty(skillPropertyString);
            }
            else if (Character.CheckObjectProperty(gameSkillString , false))
            {
                Character.AddObjectProperty($"{Character.Context}{Character.Name}" , skillPropertyString , skillString);
                var property = Character.Ontology.Model.PropertyModel.SelectProperty(skillPropertyString);
                var propertySkillAnnotations = Character.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(property).Where(entry => entry.TaxonomyPredicate.ToString().Contains("ActiveSkill"));
                if (propertySkillAnnotations.Count() > 0)
                    gotExpectedSkills = true;
                else
                    gotExpectedSkills = false;
                Character.RemoveObjectProperty(skillPropertyString);
            }

            return gotExpectedSkills;
        }

        [Test]
        [TestCase("Crear Frío" , ExpectedResult = true)]
        [TestCase("Hibernar" , ExpectedResult = false)]
        [TestCase("Ataque" , ExpectedResult = true)]
        [TestCase("Tirabuzón" , ExpectedResult = false)]
        public bool GetSkillValue_Test( string name )
        {
            var result = 0;
            var expectedResult = false;
            var random = new Random();
            var testValue = random.Next(100).ToString();

            var skillName = $"{FileService.EscapedName(name)}";
            var skillString = Character.GetFullString(skillName);
            var characterSkillString = string.Empty;

            if (string.IsNullOrEmpty(skillString))
            {
                var gameSkillString = Character.GetFullString($"Per_{skillName}_Total");
                if (gameSkillString != null)
                {
                    var valuetypeProperty = Game.Ontology.Model.PropertyModel.SelectProperty(gameSkillString);
                    if (valuetypeProperty != null)
                    {
                        var valuetype = valuetypeProperty.Range.ToString().Split('#').Last();
                        characterSkillString = $"{Character.Context}{skillString.Split('#').Last()}";
                        if (character.CheckDatatypeProperty(characterSkillString))
                        {
                            var property = Character.Ontology.Model.PropertyModel.SelectProperty(characterSkillString);
                            var value = Character.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(property).Single().TaxonomyObject.ToString().Split('^').First();
                            if (Convert.ToInt32(value) == Character.GetSkillValue(skillName))
                                expectedResult = true;
                        }
                        else
                        {
                            if (Character.CheckDatatypeProperty(characterSkillString , false))
                            {
                                Character.AddDatatypeProperty($"{Character.Context}{Character.Name}" , characterSkillString , testValue , valuetype);
                                if (Convert.ToInt32(testValue) == Character.GetSkillValue(skillName))
                                    expectedResult = true;
                                Character.RemoveDatatypeProperty(characterSkillString);
                            }
                        }
                    }
                }
            }
            else
            {
                var newSkillName = $"Per_{skillName}_Total";
                var gameSkillString = Character.GetFullString(newSkillName);
                if (gameSkillString != null)
                {
                    if (Character.CheckDatatypeProperty(gameSkillString , false))
                    {
                        var valuetype = Game.Ontology.Model.PropertyModel.SelectProperty(gameSkillString).Range.ToString().Split('#').Last();
                        characterSkillString = $"{Character.Context}{newSkillName}";
                        if (character.CheckDatatypeProperty(characterSkillString))
                        {
                            var property = Character.Ontology.Model.PropertyModel.SelectProperty(characterSkillString);
                            bool addedAssertion = false;
                            if (property != null)
                            {
                                var valueEntries = Character.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(property);
                                if (valueEntries.EntriesCount > 0)
                                    testValue = valueEntries.Single().TaxonomyObject.ToString().Split('^').First();
                                else
                                {
                                    addedAssertion = true;
                                    Character.AddDatatypeProperty($"{Character.Context}{Character.Name}" , characterSkillString , testValue , valuetype);
                                }
                            }
                            else
                            {
                                addedAssertion = true;
                                Character.AddDatatypeProperty($"{Character.Context}{Character.Name}" , characterSkillString , testValue , valuetype);
                            }

                            result = Character.GetSkillValue(skillName);
                            if (Convert.ToInt32(testValue) == result)
                                expectedResult = true;

                            if (addedAssertion)
                                Character.RemoveDatatypeProperty(characterSkillString);
                        }
                        else
                        {
                            Character.AddDatatypeProperty($"{Character.Context}{Character.Name}" , characterSkillString , testValue , valuetype);
                            if (Convert.ToInt32(testValue) == Character.GetSkillValue(skillName))
                                expectedResult = true;
                            Character.RemoveDatatypeProperty(characterSkillString);
                        }
                    }
                    else
                    {
                        var skillFact = Game.Ontology.Data.SelectFact(skillString);
                        var upperClassEntries = Game.Ontology.Data.Relations.ClassType.SelectEntriesBySubject(skillFact);
                        var upperClassString = upperClassEntries.Single().TaxonomyObject.ToString();
                        var upperClass = Game.Ontology.Model.ClassModel.SelectClass(upperClassString);
                        RDFOntologyObjectProperty skillValueProperty = null;
                        if (skillFact != null)
                        {
                            var objectPropertiesSet = new HashSet<string>();
                            bool skillPropertyFound = false;
                            while (!skillPropertyFound)
                            {
                                var objectPropertiesEnumerator = Game.Ontology.Model.PropertyModel.ObjectPropertiesEnumerator;
                                while (objectPropertiesEnumerator.MoveNext())
                                {
                                    var currentProperty = objectPropertiesEnumerator.Current;
                                    if (currentProperty.Range == upperClass)
                                        objectPropertiesSet.Add(currentProperty.ToString());
                                }
                                if (objectPropertiesSet.Count > 0)
                                {
                                    var skillValueAnnotationString = character.GetFullString($"SkillValue");
                                    var skillValueAnnotation = Game.Ontology.Model.PropertyModel.SelectProperty(skillValueAnnotationString);
                                    foreach (var item in objectPropertiesSet)
                                    {
                                        var property = Game.Ontology.Model.PropertyModel.SelectProperty(item) as RDFOntologyObjectProperty;
                                        var propertySkillValueEntries = Game.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(property).SelectEntriesByPredicate(skillValueAnnotation);
                                        if (propertySkillValueEntries.EntriesCount > 0)
                                        {
                                            skillPropertyFound = true;
                                            skillValueProperty = property;
                                            break;
                                        }
                                    }
                                    if (!skillPropertyFound)
                                    {
                                        upperClassEntries = Game.Ontology.Data.Relations.ClassType.SelectEntriesBySubject(upperClass);
                                        if (upperClassEntries.EntriesCount == 0)
                                            upperClass = Game.Ontology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(upperClass).Single().TaxonomyObject as RDFOntologyClass;
                                        else
                                            upperClass = upperClassEntries.Single().TaxonomyObject as RDFOntologyClass;
                                    }
                                }
                                else
                                {
                                    upperClassEntries = Game.Ontology.Data.Relations.ClassType.SelectEntriesBySubject(upperClass);
                                    if (upperClassEntries.EntriesCount == 0)
                                        upperClass = Game.Ontology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(upperClass).Single().TaxonomyObject as RDFOntologyClass;
                                    else
                                        upperClass = upperClassEntries.Single().TaxonomyObject as RDFOntologyClass;
                                }
                            }

                            var valuePropertyEntries = Game.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(skillValueProperty);
                            var valuePropertyName = valuePropertyEntries.Where(entry => entry.TaxonomyPredicate.ToString().Contains("SkillValue")).Single().TaxonomyObject.ToString().Split('^').First();
                            var characterValuePropertyString = character.GetFullString(valuePropertyName , true);
                            var value = string.Empty;
                            if (string.IsNullOrEmpty(characterValuePropertyString))
                            {
                                characterValuePropertyString = $"{Character.Context}{valuePropertyName}";
                                var valuetype = Game.Ontology.Model.PropertyModel.SelectProperty($"{Game.Context}{valuePropertyName}").Range.ToString().Split('#').First();
                                Character.AddDatatypeProperty($"{Character.Context}{Character.Name}" , characterValuePropertyString , testValue , valuetype);
                                value = testValue;
                                var skillValue = Character.GetSkillValue(valuePropertyName);
                                if (Convert.ToInt32(value) == skillValue)
                                    expectedResult = true;
                                Character.RemoveDatatypeProperty(characterValuePropertyString);
                            }
                            else
                            {
                                var property = Character.Ontology.Model.PropertyModel.SelectProperty(characterValuePropertyString);
                                var valueEntries = Character.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(property);
                                if (valueEntries.EntriesCount > 0)
                                {
                                    value = valueEntries.Single().TaxonomyObject.ToString().Split('^').First();
                                    if (Convert.ToInt32(value) == Character.GetSkillValue(valuePropertyName))
                                        expectedResult = true;
                                }
                            }
                        }
                    }
                }
            }
            return expectedResult;
        }

        [Test]
        [TestCase("Guybrush Threepwood" , true , ExpectedResult = "Personaje Jugador")]
        [TestCase("Kempo" , true , ExpectedResult = "Arte Marcial Básica")]
        [TestCase("Crear Frío" , false , ExpectedResult = "Libro Agua")]
        [TestCase("FailedTest" , false , ExpectedResult = "")]
        public string GetElementClass_Test( string elementName , bool applyOnCharacter )
        {
            var elementClassName = string.Empty;
            var escapedElementName = FileService.EscapedName(elementName);
            var elementString = character.GetFullString(escapedElementName , applyOnCharacter);
            if (!string.IsNullOrEmpty(elementString))
            {
                var elementClass = Character.GetElementClass(elementString , applyOnCharacter);
                if (elementClass != null)
                    elementClassName = elementClass.FormattedName;
            }
            return elementClassName;
        }

        static List<object[]> Descriptions = new List<object[]>
        {
            new object[]
            {
                "Kempo",
                "Es una disciplina de combate libre que emplea combinaciones de golpes. Su estilo consiste en atacar rápidamente al adversario, " +
                "esperando encontrar fallos en su guardia gracias a la acumulación de impactos.\n\nVentajas: Los rápidos encadenamientos de golpes" +
                " permiten al maestro de Kempo realizar los ataques adicionales con un penalizador de –10 a su habilidad, en lugar del –25 habitual. " +
                "Da una base de daño 20 más el bono de Fuerza, y ataca en Contundentes.\n\nRequisitos: Ninguno.\n\nConocimiento marcial: +10\n\nBono: Ninguno.",
                true,
                true
            },
            new object[]
            {
                "Crear Frío",
                "Nivel: 6 \nAcción: Activa \nTipo: Efecto\n" +
                "Efecto: Crea intensidades de frío o hielo. Mientras se mantenga el conjuro, la temperatura se conservará.\n\n" +
                "Grado    Base    Intermedio    Avanzado   Arcano\n" +
                "Zeon         30             50                   90            140\n" +
                "Int. R.         5               7                     10             12\n\n" +
                "Base: 1 Intensidad.\nIntermedio: 3 Intensidades.\nAvanzado: 5 Intensidades.\nArcano: 8 Intensidades.\n" +
                "Mantenimiento: 5 / 5 / 10 / 15 Diario",
                true,
                false
            },
            new object[]
            {
                "Kempo",
                "Es una disciplina de combate libre que emplea combinaciones de golpes. Su estilo consiste en atacar rápidamente al adversario, " +
                "esperando encontrar fallos en su guardia gracias a la acumulación de impactos.\n\nVentajas: Los rápidos encadenamientos de golpes" +
                " permiten al maestro de Kempo realizar los ataques adicionales con un penalizador de –10 a su habilidad, en lugar del –25 habitual. " +
                "Da una base de daño 20 más el bono de Fuerza, y ataca en Contundentes.\n\nRequisitos: Ninguno.\n\nConocimiento marcial: +10\n\nBono: Ninguno.",
                false,
                true
            },
            new object[]
            {
                "Crear Frío",
                "Nivel: 6 \nAcción: Activa \nTipo: Efecto\n" +
                "Efecto: Crea intensidades de frío o hielo. Mientras se mantenga el conjuro, la temperatura se conservará.\n\n" +
                "Grado    Base    Intermedio    Avanzado   Arcano\n" +
                "Zeon         30             50                   90            140\n" +
                "Int. R.         5               7                     10             12\n\n" +
                "Base: 1 Intensidad.\nIntermedio: 3 Intensidades.\nAvanzado: 5 Intensidades.\nArcano: 8 Intensidades.\n" +
                "Mantenimiento: 5 / 5 / 10 / 15 Diario",
                false,
                false
            },
        };

        [Test]
        [TestCaseSource(nameof(Descriptions))]
        public void GetElementDescription_Test( string elementName , string expectedDescription , bool applyOnCharacter , bool expectedResult )
        {
            var gotExpectedResult = false;
            var gotExpectedDescription = false;
            var escapedElementName = FileService.EscapedName(elementName);
            var elementString = character.GetFullString(escapedElementName , applyOnCharacter);
            if (!string.IsNullOrEmpty(elementString))
            {
                var currentOntology = applyOnCharacter ? Character.Ontology : Game.Ontology;
                if (character.CheckIndividual(elementString , applyOnCharacter))
                {
                    var elementFact = currentOntology.Data.SelectFact(elementString);
                    var descriptionEntries = currentOntology.Data.Annotations.Comment.SelectEntriesBySubject(elementFact);
                    if (descriptionEntries.EntriesCount > 0)
                    {
                        var description = descriptionEntries.Single().TaxonomyObject.ToString().Split('^').First();
                        gotExpectedDescription = string.Equals(expectedDescription , description);
                        gotExpectedResult = (gotExpectedDescription == expectedResult) ? true : false;
                    }
                }
                else if (character.CheckDatatypeProperty(elementString , applyOnCharacter) || character.CheckObjectProperty(elementString , applyOnCharacter))
                {
                    var elementProperty = currentOntology.Model.PropertyModel.SelectProperty(elementString);
                    var descriptionEntries = currentOntology.Model.PropertyModel.Annotations.Comment.SelectEntriesBySubject(elementProperty);
                    if (descriptionEntries.EntriesCount > 0)
                    {
                        var description = descriptionEntries.Single().TaxonomyObject.ToString().Split('^').First();
                        gotExpectedDescription = string.Equals(expectedDescription , description);
                        gotExpectedResult = (gotExpectedDescription == expectedResult) ? true : false;
                    }
                }
            }
            Assert.AreEqual(expectedResult , gotExpectedResult);
        }

        static List<object[]> Individuals = new List<object[]>
        {
            new object[]
            {
                "Arte Marcial Básica",
                new List<string>
                {
                    "Aikido",
                    "Capoeira",
                    "Grappling",
                    "Kempo",
                    "Kung Fu",
                    "Moai Thai",
                    "Sambo",
                    "Shotokan",
                    "Tae Kwon Do",
                    "Tai Chi"
                },
                false,
                true
            },

            new object[]
            {
                "Arte Marcial Básica",
                new List<string> {"Kempo"},
                true,
                true
            },

            new object[]
            {
                "Libro_Agua",
                new List<string>
                {
                    "Ataque De Hielo",
                    "Burbuja Protectora",
                    "Capacidad Acuática",
                    "Congelar",
                    "Congelar La Magia",
                    "Congelar Las Emociones",
                    "Control De Las Mareas",
                    "Control Reflejo",
                    "Control Sobre Los Líquidos",
                    "Controlar El Frío",
                    "Crear Frío",
                    "Crear Líquidos",
                    "Crear Ondina",
                    "Cristalización",
                    "Cuerpo Líquido",
                    "En El Interior Del Espejo",
                    "Enlentecer El Tiempo",
                    "Glacial",
                    "Impacto De Agua",
                    "Inmunidad Al Frío",
                    "Manantial",
                    "Pantalla De Hielo",
                    "Prisión De Agua",
                    "Reflejar Estados",
                    "Reflejo Del Alma",
                    "Señor De Las Aguas",
                    "Señor De Los Hielos",
                    "Tormenta De Hielo",
                    "Tsunami",
                    "Un Mundo Perfecto"
                },
                false,
                true
            },

            new object[]
            {
                "Libro_Agua",
                new List<string>{"Crear Frío"},
                true,
                false
            },
        };

        [Test]
        [TestCaseSource(nameof(Individuals))]
        public void GetIndividuals_Test( string className , List<string> expectedIndividuals , bool applyOnCharacter , bool expectedResult )
        {
            var gotExpectedIndividuals = false;
            var escapedClassName = FileService.EscapedName(className);
            var classString = character.GetFullString(escapedClassName , applyOnCharacter);
            if (!string.IsNullOrEmpty(classString))
            {
                var individuals = new List<string>();
                var items = character.GetIndividuals(classString , applyOnCharacter);
                foreach (var item in items)
                    individuals.Add(FileService.FormatName(item.FullName.Split('#').Last()));
                gotExpectedIndividuals = !expectedIndividuals.Except(individuals).Any();
            }
            Assert.AreEqual(expectedResult , gotExpectedIndividuals);
        }

        static List<object[]> IndividualsGrouped_TestCases = new List<object[]>
        {
            new object[]
            {
                "Raza",
                new List<List<string>>
                {
                    new List<string>
                    {
                        "Nephilim D'anjayni",
                        "Nephilim Ebudan",
                        "Nephilim Jayán",
                        "Nephilim Daimah",
                        "Nephilim Sylvain",
                        "Nephilim Duk'zarist"
                    },
                    new List<string>
                    {
                        "Humano"
                    }
                },
                false,
                true
            },

            new object[]
            {
                "Raza",
                new List<List<string>>
                {
                    new List<string>
                    {
                        "Nephilim D'anjayni",
                        "Nephilim Ebudan",
                        "Nephilim Jayán",
                        "Nephilim Daimah",
                        "Nephilim Sylvain",
                        "Nephilim Duk'zarist"
                    },
                    new List<string>
                    {
                        "Humano"
                    }
                },
                true,
                false
            },

            new object[]
            {
                "Raza",
                new List<List<string>>
                {
                    new List<string>
                    {
                        "Humano"
                    }
                },
                true,
                true
            },
        };

        [Test]
        [TestCaseSource(nameof(IndividualsGrouped_TestCases))]
        public void GetIndividualsGrouped_Test( string className , List<List<string>> expectedGroups , bool applyOnCharacter , bool expectedResult )
        {
            var gotExpectedGroups = true;
            var escapedClassName = FileService.EscapedName(className);
            var classString = character.GetFullString(escapedClassName , applyOnCharacter);
            if (!string.IsNullOrEmpty(classString))
            {
                bool errorDetected = false;
                var groups = new ObservableCollection<Group>(Character.GetIndividualsGrouped(classString , applyOnCharacter));
                var groupsCount = groups.Count;
                var expectedGroupsCount = expectedGroups.Count();
                if (int.Equals(expectedGroupsCount , groupsCount))
                {
                    var groupsEnumerator = groups.GetEnumerator();
                    while (!errorDetected && groupsEnumerator.MoveNext())
                    {
                        bool groupFound = false;
                        var currentGroup = groupsEnumerator.Current;
                        var currentIndividuals = new List<string>();
                        foreach (var item in currentGroup.Elements)
                            currentIndividuals.Add(FileService.FormatName(item.FullName.Split('#').Last()));
                        var expectedGroupEnumerator = expectedGroups.GetEnumerator();
                        while (!groupFound && expectedGroupEnumerator.MoveNext())
                        {
                            var currentExpectedGroup = expectedGroupEnumerator.Current;
                            var currentExpectedIndividuals = new List<string>();
                            foreach (var item in currentExpectedGroup)
                                currentExpectedIndividuals.Add(item);
                            if (!currentExpectedIndividuals.Except(currentIndividuals).Any())
                                groupFound = true;
                        }
                        errorDetected = !groupFound;
                    }
                    gotExpectedGroups = !errorDetected;
                }
                else
                    gotExpectedGroups = false;
            }
            Assert.AreEqual(expectedResult , gotExpectedGroups);
        }

        [Test]
        [TestCase("Habilidad Combate General" , 1500 , true , ExpectedResult = true)]
        public bool GetAvailablePoints_Test( string elementName , double? expectedPoints , bool applyOnCharacter = true )
        {
            var elementString = Character.GetFullString(FileService.EscapedName(elementName) , applyOnCharacter);
            Character.GetAvailablePoints(elementString , out var availablePoints , applyOnCharacter);
            return expectedPoints == availablePoints;
        }

        [Test]
        [TestCase("Per Nivel" , "Per_Límite_Nivel" , false , false , ExpectedResult = true)]
        [TestCase("Habilidad Combate General" , "Per_Límite_PD" , true , true , ExpectedResult = true)]
        [TestCase("Arte Marcial Básica" , "Per_Disponible_Habilidad_Combate" , false , true , ExpectedResult = true)]
        public bool GetLimit_Test( string stageName , string expectedLimit , bool isGeneral = false , bool editGeneralLimit = false )
        {
            var limit = Character.GetLimit(FileService.EscapedName(stageName) , isGeneral , editGeneralLimit);
            return string.Equals(expectedLimit , limit);
        }

        [Test]
        [TestCase("Acrobacias" , "Habilidades Atléticas" , 2 , ExpectedResult = true)]
        [TestCase("Conocimiento Marcial" , "Habilidad Combate General" , 5 , ExpectedResult = true)]
        public bool GetStep_Test( string itemName , string stageName , double expectedStep )
        {
            var step = Character.GetStep(FileService.EscapedName(itemName) , FileService.EscapedName(stageName));
            return double.Equals(expectedStep , step);
        }

        [Test]
        [TestCase("Per_Límite_Nivel" , 20 , ExpectedResult = true)]
        [TestCase("Per_Límite_Habilidad_Combate" , 900 , ExpectedResult = true)]
        public bool GetLimitValue_Test( string limitPropertyName , double expectedValue )
        {
            var value = Character.GetLimitValue(FileService.EscapedName(limitPropertyName));
            return double.Equals(expectedValue , value);
        }

        [Test]
        [TestCase("Arte Marcial Básica" , "Kempo" , true , "tieneArte Marcial Básica" , ExpectedResult = true)]
        [TestCase("Raza Pura" , "Humano" , true , "tieneRaza" , ExpectedResult = true)]
        public bool GetObjectPropertyAssociated_Test( string stageName , string itemName , bool applyOnCharacter , string expectedProperty )
        {
            var escapedStageName = FileService.EscapedName(stageName);
            var escapedItemName = FileService.EscapedName(itemName);
            var currentOntology = applyOnCharacter ? Character.Ontology : Game.Ontology;
            var stageString = Character.GetFullString(escapedStageName , applyOnCharacter);
            var stageElements = Character.GetIndividuals(stageString , applyOnCharacter);
            var item = stageElements.Where(element => string.Equals(element.FullName.Split('#').Last() , escapedItemName)).Single();
            var property = Character.GetObjectPropertyAssociated(stageString , item , applyOnCharacter).Split('#').Last();
            return string.Equals(FileService.EscapedName(expectedProperty) , property);
        }

        [Test]
        [TestCase("Crear Frío", false,"Libro Agua", ExpectedResult = true)]
        [TestCase("Kempo", true ,"Arte Marcial Básica", ExpectedResult = true)]
        [TestCase("Humano", true,"Raza Pura", ExpectedResult = true)]
        
        public bool GetParentClasses_Test(string itemName, bool applyOnCharacter, string expectedClassName)
        {
            var itemString = Character.GetFullString(FileService.EscapedName(itemName) , applyOnCharacter);
            var escapedExpectedClassName = FileService.EscapedName(expectedClassName);
            var expectedParentClassesString = Character.GetParentClasses(itemString , applyOnCharacter);
            var expectedParentClasses = expectedParentClassesString.Split('|').ToList();
            return expectedParentClasses.Any(parent => parent.Contains(escapedExpectedClassName));
        }

        static List<object[]> Subclasses_TestCases = new List<object[]>
        {
            new object[]
            {
                "Tabla Armas",
                false,
                new List<string>
                {
                    "Arma Similar",
                    "Armas Arquetipo",
                    "Armas Tipología",
                    "Tabla Estilo",
                    "Tablas Armas Generales"
                }
            },

            new object[]
            {
                "Conjuro Vía Mágica",
                false,
                new List<string>
                {
                    "Libro_Agua",
                    "Libro_Aire",
                    "Libro_Creación",
                    "Libro_Destrucción",
                    "Libro_Esencia",
                    "Libro_Fuego",
                    "Libro_Ilusión",
                    "Libro_Luz",
                    "Libro_Nigromancia",
                    "Libro_Oscuridad",
                    "Libro_Tierra"
                }
            },
        };

        [Test]
        [TestCaseSource(nameof(Subclasses_TestCases))]
        public void GetSubclasses_Test(string className, bool applyOnCharacter, List<string> expectedSubclasses)
        {
            var classString = Character.GetFullString(FileService.EscapedName(className) , applyOnCharacter);
            var groups = Character.GetSubClasses(classString , applyOnCharacter).ToList();
            var subclasses = new List<string>();
            foreach (var group in groups)
                subclasses.Add(group.Title);
            subclasses.Sort();
            expectedSubclasses.Sort();
            Assert.IsTrue(expectedSubclasses.SequenceEqual(subclasses));
        }

        [Test]
        [TestCase("XMLLiteral" , "http://www.w3.org/1999/02/22-rdf-syntax-ns#XMLLiteral", ExpectedResult = true)]
        public bool GetDatatypeUri_Test( string datatypeName , string expectedResult)
        {
            var result = Character.GetDatatypeUri(datatypeName);
            return string.Equals(expectedResult , result);
        }

        [Test]
        [TestCase("Guybrush Threepwood", true, "http://arpegos_project/Games/Anima_Beyond_Fantasy/characters/Guybrush_Threepwood#Guybrush_Threepwood")]
        [TestCase("Descrear", false, "urn:absolute:arpegos-project.org/games/anima_beyond_fantasy#Descrear")]
        public void GetString_Test(string elementName, bool applyOnCharacter, string expectedElementString)
        {
            var elementString = Character.GetFullString(FileService.EscapedName(elementName) , applyOnCharacter);
            Assert.IsTrue(string.Equals(expectedElementString ,elementString));
        }


        static List<object[]> GetValue_TestCases = new List<object[]>
        {

            new object[]
            {
                "90",
                null,
                null,
                false,
                90
            },

            new object[]
            {
                "Per_Nivel",
                null,
                null,
                true,
                10
            },

            new object[]
            {
                "tieneCategoría:Cat_Item_Coste",
                "Esquiva",
                null,
                true,
                2
            },

            new object[]
            {
                "Per_Item_PD",
                "Esquiva",
                null,
                true,
                20
            },

            new object[]
            {
                "Per_Item_PD: /: tieneCategoría :Cat_Item_Coste",
                "Esquiva",
                null,
                true,
                10
            },

            new object[]
            {
                "Per_Item_PD: /: tieneCategoría :Cat_Item_Coste: +: Per_Item_Bono_Categoría",
                "Esquiva",
                null,
                true,
                20
            },
        };
        
        [Test]
        [TestCaseSource(nameof(GetValue_TestCases))]
        public void GetValue_Test( string valueDefinition , string itemName = null , string User_Input = null , bool applyOnCharacter = false, float expectedValue = 0)
        {
            var escapedItemName = string.Empty;
            if(!string.IsNullOrEmpty(itemName))
                escapedItemName = FileService.EscapedName(itemName);
            var value = Character.GetValue(valueDefinition , escapedItemName , User_Input, applyOnCharacter );
            Assert.IsTrue(float.Equals(expectedValue , value));
        }
    }
}

