using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.ViewModels;
using RDFSharp.Model;
using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ARPEGOS.Services
{
    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Returns the hierarchy of the property given
        /// </summary>
        /// <param name="propertyString"> String of the property given </param>
        /// <returns> String with the hierarchy of the property given </returns>
        public string GetPropertyVisualizationPosition (string propertyString)
        {
            var CurrentProperty = this.Ontology.Model.PropertyModel.SelectProperty(propertyString);
            var AnnotationProperty = this.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.Where(property => property.TaxonomyPredicate.ToString().Contains("Visualization")).First().TaxonomyPredicate;
            var CharacterVisualizationAnnotations = this.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesByPredicate(AnnotationProperty);
            var CharacterVisualizationAnnotation = CharacterVisualizationAnnotations.SelectEntriesBySubject(CurrentProperty).Single();
            return CharacterVisualizationAnnotation.TaxonomyObject.ToString().Split('^').First();
        } //MODIFIED

        /// <summary>
        /// Returns all the properties of the current character 
        /// </summary>
        /// <returns> Dictionary with the properties of the current character and its values</returns>
        public Dictionary<string, List<string>> GetCharacterProperties ()
        {
            var CharacterProperties = new Dictionary<string, List<string>>();
            var characterName = FileService.EscapedName(this.Name);
            var CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{characterName}");
            var CharacterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
            foreach (var assertion in CharacterAssertions)
            {
                var property = assertion.TaxonomyPredicate.ToString();
                var valueString = assertion.TaxonomyObject.ToString();
                if(!CharacterProperties.ContainsKey(property))
                    CharacterProperties.Add(property, new List<string> { valueString });
                else
                    CharacterProperties[property].Add(valueString);                
            }
            return CharacterProperties;
        }//MODIFIED

        /// <summary>
        /// Returns the properties that represent the skills of the character
        /// </summary>
        /// <returns> Set of string which contains the names of the skills </returns>
        public IEnumerable<Item> GetCharacterSkills ()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            var characterProperties = GetCharacterProperties();
            var skills = new ObservableCollection<Item>();
            var AnnotationType = "ActiveSkill";
            foreach (var pair in characterProperties)
            {
                var propertyName = pair.Key.Split('#').Last().Replace("Per_", string.Empty).Replace("_Total", string.Empty);
                var propertyString = character.GetString(propertyName);
                if(!string.IsNullOrEmpty(propertyString))
                {
                    var propertyFact = game.Ontology.Data.SelectFact(propertyString);
                    if (propertyFact != null)
                    {
                        var propertyFactCustomAnnotations = game.Ontology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(propertyFact);
                        if(propertyFactCustomAnnotations.Count() > 0)
                        {
                            var propertyFactSkillAnnotations = propertyFactCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains(AnnotationType));
                            if(propertyFactSkillAnnotations.Count()>0)
                            {
                                var skillDescriptionEntries = game.Ontology.Data.Annotations.Comment.SelectEntriesBySubject(propertyFact);
                                if(skillDescriptionEntries.Count() > 0)
                                {
                                    var skillDescription = skillDescriptionEntries.Single().TaxonomyObject.ToString();
                                    skills.Add(new Item(propertyString, skillDescription));
                                }
                            }
                        }
                        else
                        {
                            var propertyClass = game.Ontology.Data.Relations.ClassType.SelectEntriesBySubject(propertyFact).Single().TaxonomyObject as RDFOntologyClass;
                            var propertyClassCustomAnnotations = game.Ontology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(propertyClass);
                            if(propertyClassCustomAnnotations.Count() > 0)
                            {
                                var propertyClassSkillAnnotations = propertyClassCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains(AnnotationType));
                                if(propertyClassSkillAnnotations.Count() > 0)
                                {
                                    var skillDescriptionEntries = game.Ontology.Data.Annotations.Comment.SelectEntriesBySubject(propertyFact);
                                    if (skillDescriptionEntries.Count() > 0)
                                    {
                                        var skillDescription = skillDescriptionEntries.Single().TaxonomyObject.ToString();
                                        skills.Add(new Item(propertyName, skillDescription));
                                    }
                                }
                            }
                        }
                    } 
                }

            }
            return skills;
        }//MODIFIED

        /// <summary>
        /// Returns the numeric value associated to the skill given
        /// </summary>
        /// <param name="skillName">Name of the skill</param>
        /// <returns></returns>
        public int GetSkillValue (string skillName)
        {
            var game = DependencyHelper.CurrentContext.CurrentGame;
            var skillPropertyName = $"Per_{skillName}_Total";
            var skillPropertyString = this.GetString(skillPropertyName, true);
            var currentString = string.Empty;
            if (this.CheckDatatypeProperty(skillPropertyString) == true)
            {
                skillName = skillPropertyName;
                currentString = skillPropertyString;
            }
            else
                currentString = this.GetString(skillName, true);

            int skillValue = 0;
            var character = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
            if (this.CheckIndividual(currentString))
            {
                var skillFact = this.Ontology.Data.SelectFact(currentString);
                var skillProperty = this.Ontology.Data.Relations.Assertions.SelectEntriesByObject(skillFact).Single().TaxonomyPredicate;
                var valuePropertyEntries = this.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(skillProperty);
                var valuePropertyName = valuePropertyEntries.Where(entry => entry.TaxonomyPredicate.ToString().Contains("SkillValue")).Single().TaxonomyObject.ToString().Split('^').First();
                var valuePropertyString = this.GetString(valuePropertyName, true);
                var valueProperty = this.Ontology.Model.PropertyModel.SelectProperty(valuePropertyString);
                skillValue = Convert.ToInt32(this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(valueProperty).Single().TaxonomyObject.ToString().Split('^').First()); 

                /*var annotationPropertyName = "SkillValue";
                var annotationProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{annotationPropertyName}");
                var skillValueAssertionFound = false;
                var parentClass = this.Ontology.Model.ClassModel.SelectClass($"{this.Context}{this.GetElementClass(skillName, true)}");
                var annotationValue = string.Empty;

                var skillFact = this.Ontology.Data.SelectFact($"{this.Context}{skillName}");
                var skillValueAnnotationTaxonomy = this.Ontology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(character).SelectEntriesByPredicate(annotationProperty);
                if (skillValueAnnotationTaxonomy != null)
                {
                    skillValueAssertionFound = true;
                    annotationValue = skillValueAnnotationTaxonomy.Single().TaxonomyObject.ToString().Split('^').First();
                }

                while (skillValueAssertionFound == false)
                {
                    skillValueAnnotationTaxonomy = this.Ontology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(parentClass);
                    foreach (var assertion in skillValueAnnotationTaxonomy)
                    {
                        if (assertion.TaxonomyPredicate.ToString().Contains(annotationPropertyName))
                        {
                            annotationValue = assertion.TaxonomyObject.ToString().Split('^').First();
                            skillValueAssertionFound = true;
                            break;
                        }
                    }
                    if (skillValueAssertionFound == false)
                    {
                        var parentClassName = parentClass.Value.ToString().Split('#').Last();
                        parentClass = this.Ontology.Model.ClassModel.SelectClass($"{this.Context}{this.GetElementClass(parentClassName, true)}"); ;
                    }
                }

                var annotationElements = annotationValue.Split(':').ToList();
                var operators = new List<string>() { "+", "-", "*", "/" };
                for (var index = 0; index < annotationElements.Count(); ++index)
                {
                    var element = annotationElements[index];
                    if (operators.Any(op => element == op))
                    {
                        var nextElement = annotationElements[index + 1];
                        if ((Regex.IsMatch(nextElement, @"\d") && (!Regex.IsMatch(nextElement, @"\D"))))
                            skillValue = OntologyService.ConvertToOperator(element, skillValue, Convert.ToInt32(nextElement));
                        else
                        {
                            var nextElementProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{nextElement}");
                            var nextElementPropertyValue = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(nextElementProperty).Single().TaxonomyObject.ToString().Split('^').First();
                            skillValue = OntologyService.ConvertToOperator(element, skillValue, Convert.ToInt32(nextElementPropertyValue));
                        }
                    }
                    else if ((Regex.IsMatch(element, @"\d") && (!Regex.IsMatch(element, @"\D"))))
                    {
                        if (index == 0)
                            skillValue = Convert.ToInt32(element);
                    }
                    else
                    {
                        if (index == 0)
                        {
                            var elementProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{element}");
                            var elementPropertyValue = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(elementProperty).Single().TaxonomyObject.ToString().Split('^').First();
                            skillValue = Convert.ToInt32(elementPropertyValue);
                        }
                    }
                }*/
            }
            else if (this.CheckDatatypeProperty(currentString) == true)
            {
                var currentProperty = this.Ontology.Model.PropertyModel.SelectProperty(currentString);
                var assertion = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(character).SelectEntriesByPredicate(currentProperty).Single();
                skillValue = Convert.ToInt32(assertion.TaxonomyObject.ToString().Split('^').First());
            }
            return skillValue;
        }

        /// <summary>
        /// Returns class of the given element, choosing if the element is from the character or not
        /// </summary>
        /// <param name="elementName">Name of the element given</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public Item GetElementClass (string elementString, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            string currentElementString = elementString;
            Item elementClass = null;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
                var shortName = elementString.Split('#').Last();
                currentElementString = $"{this.Context}{shortName}";
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            if (!elementString.Contains('#'))
                currentElementString = this.GetString(elementString, applyOnCharacter);                

            var DataModel = CurrentOntology.Data;
            var currentElementClass = CurrentOntology.Model.ClassModel.SelectClass(currentElementString);
            if(currentElementClass != null)
            {
                var elementClassType = DataModel.Relations.ClassType.SelectEntriesBySubject(currentElementClass).Single();
                var elementClassString = elementClassType.TaxonomyObject.ToString();
                var elementClassResource = this.Ontology.Model.ClassModel.SelectClass(elementClassString);
                var elementClassDescription = this.Ontology.Model.ClassModel.Annotations.Comment.SelectEntriesBySubject(elementClassResource).Single().TaxonomyObject.ToString().Split('^').First();
                elementClass = new Item(elementClassString, elementClassDescription);
            }
            else
            {
                var currentElementFact = DataModel.SelectFact(currentElementString);
                if(currentElementFact != null)
                {
                    var elementClassType = DataModel.Relations.ClassType.SelectEntriesBySubject(currentElementFact).Single();
                    var elementClassString = elementClassType.TaxonomyObject.ToString();
                    var elementClassResource = this.Ontology.Model.ClassModel.SelectClass(elementClassString);
                    var elementClassDescription = this.GetElementDescription(elementClassString);
                    elementClass = new Item(elementClassString, elementClassDescription);
                }
            }
            
            return elementClass;
        }//MODIFIED

        /// <summary>
        /// Returns description of the element
        /// </summary>
        /// <param name="elementName">Name of the element</param>
        /// <returns></returns>
        public string GetElementDescription (string elementString, bool applyOnCharacter = false)
        {
            var elementDesc = string.Empty;
            RDFOntology CurrentOntology;
            var CurrentContext = string.Empty;
            string currentElementString = elementString;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
                var shortName = elementString.Split('#').Last();
                currentElementString = $"{this.Context}{shortName}";
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            RDFOntologyResource currentElementResource;
            RDFOntologyTaxonomy elementCommentAnnotationEntries;
            if (CheckClass(currentElementString, applyOnCharacter))
            {
                currentElementResource = CurrentOntology.Model.ClassModel.SelectClass(currentElementString);
                elementCommentAnnotationEntries = CurrentOntology.Model.ClassModel.Annotations.Comment.SelectEntriesBySubject(currentElementResource);
            }
            else if (CheckFact(currentElementString, applyOnCharacter))
            {
                currentElementResource = CurrentOntology.Data.SelectFact(currentElementString);
                elementCommentAnnotationEntries = CurrentOntology.Data.Annotations.Comment.SelectEntriesBySubject(currentElementResource);
            }
            else
            {
                currentElementResource = CurrentOntology.Model.PropertyModel.SelectProperty(currentElementString);
                elementCommentAnnotationEntries = CurrentOntology.Model.PropertyModel.Annotations.Comment.SelectEntriesBySubject(currentElementResource);
            }

            if (elementCommentAnnotationEntries.EntriesCount > 0)
            {
                var elementCommentAnnotation = elementCommentAnnotationEntries.Single().TaxonomyObject;
                if (elementCommentAnnotation != null)
                    elementDesc = elementCommentAnnotation.ToString().Split('^').First();
            }
            
            return elementDesc;
        }//MODIFIED

        /// <summary>
        /// Returns a list of individuals given the name of their class
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public IEnumerable<Item> GetIndividuals (string classString, bool applyOnCharacter = false)
        {
            var individuals = new ObservableCollection<Item>();
            RDFOntology CurrentOntology;
            string CurrentContext;
            string currentClassString = classString;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
                var shortName = classString.Split('#').Last();
                currentClassString = $"{this.Context}{shortName}";
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }
            var CostWords = new List<string> { "Coste", "Cost", "Coût" };
            var ClassModel = CurrentOntology.Model.ClassModel;
            var DataModel = CurrentOntology.Data;
            var currentClass = ClassModel.SelectClass(currentClassString);
            var currentClassName = currentClassString.Split('#').Last();
            var classAssertions = DataModel.Relations.ClassType.SelectEntriesByObject(currentClass);
            foreach (var assertion in classAssertions)
            {
                var individualFact = assertion.TaxonomySubject;
                var individualDescriptionEntries = CurrentOntology.Data.Annotations.Comment.SelectEntriesBySubject(individualFact);
                var individualDescription = string.Empty;
                if(individualDescriptionEntries.EntriesCount > 0)
                    individualDescription = individualDescriptionEntries.Single().TaxonomyObject.ToString().Split('^').First();
                var individualString = individualFact.ToString();
                var individualAssertions = CurrentOntology.Data.Relations.Assertions.SelectEntriesBySubject(individualFact);
                var individualCostAssertions = individualAssertions.Where(entry => CostWords.Any(word => entry.TaxonomyPredicate.ToString().Contains(word)));
                double individualValue = 1;
                if (individualCostAssertions.Count() > 1)
                {
                    var currentClassCustomAnnotations = CurrentOntology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(currentClass);
                    var currentClassGeneralCostAnnotation = currentClassCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("GeneralCost"));
                    if(currentClassGeneralCostAnnotation.Count() > 0)
                    {
                        var currentClassGeneralCost = currentClassGeneralCostAnnotation.Single().TaxonomyObject.ToString().Split('^').First();
                        var individualGeneralCostAssertion = individualCostAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains(currentClassGeneralCost));
                        individualValue = Convert.ToDouble(individualCostAssertions.Single().TaxonomyObject.ToString().Split('^').First());
                    }
                    else
                    {
                        var parentClassAssertions = CurrentOntology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(currentClass);
                        RDFOntologyClass parentClass;
                        if (parentClassAssertions.Count() > 0)
                        {
                            if (parentClassAssertions.Count() > 1)
                                parentClass = parentClassAssertions.First().TaxonomyObject as RDFOntologyClass;
                            else    
                                parentClass = parentClassAssertions.Single().TaxonomyObject as RDFOntologyClass;
                            currentClassCustomAnnotations = CurrentOntology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(parentClass);
                            currentClassGeneralCostAnnotation = currentClassCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("GeneralCost"));
                            if (currentClassGeneralCostAnnotation.Count() > 0)
                            {
                                var currentClassGeneralCost = currentClassGeneralCostAnnotation.Single().TaxonomyObject.ToString().Split('^').First();
                                var individualGeneralCostAssertion = individualCostAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains(currentClassGeneralCost));
                                if(individualGeneralCostAssertion.Count() > 0)
                                {
                                    var definition = individualGeneralCostAssertion.Single().TaxonomyObject.ToString();
                                    if (definition.Contains('^'))
                                        definition = definition.Split('^').First();
                                    individualValue = GetValue(definition);
                                }
                            }
                        }
                    }
                }
                else if (individualCostAssertions.Count() == 1)
                {
                    var definition = individualCostAssertions.Single().TaxonomyObject.ToString();
                    if (definition.Contains('^'))
                        definition = definition.Split('^').First();
                    individualValue = GetValue(definition, individualString.Split('#').Last());
                }

                individuals.Add(new Item(individualString, individualDescription, currentClassName, individualValue, 1, individualValue));
            }
            return individuals;
        }

        /// <summary>
        /// Returns a list of groups of individuals given the name of the root class
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public ObservableCollection<Group> GetIndividualsGrouped (string classString, bool applyOnCharacter = false)
        {
            ObservableCollection<Group> groups = null;
            ObservableCollection<Group> subclasses = GetSubClasses(classString, applyOnCharacter);
            if (subclasses != null)
            {
                groups = new ObservableCollection<Group>();
                foreach(var subclass in subclasses)
                    groups.Add(subclass);
            }
            return groups;
        }//MODIFIED

        /// <summary>
        /// Returns true if the given element has available points, and the available points left.
        /// </summary>
        /// <param name="ElementName">Name of the element</param>
        /// <param name="AvailablePoints">Available points obtained</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetAvailablePoints (string ElementString, out double? AvailablePoints, bool applyOnCharacter = false)
        {
            var Game = DependencyHelper.CurrentContext.CurrentGame;
            var Character = DependencyHelper.CurrentContext.CurrentCharacter;
            AvailablePoints = null;
            var LimitPropertyString = string.Empty;
            var AvailableWords = new List<string>()
            {
                "Disponible",
                "Available"
            };

            string currentElementString = ElementString;
            var shortName = ElementString.Split('#').Last();


            var FilterResultsCounter = 0;
            var index = 0;
            var ElementWords = shortName.Split('_').ToList();
            var CompareList = new List<string>();
            var ResultProperties = new List<RDFOntologyProperty>();

            var GameElementString = Character.GetString(ElementString.Split('#').Last());
            var GameElementClass = Game.Ontology.Model.ClassModel.SelectClass(currentElementString);
            var ElementClassAnnotations = Game.Ontology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(GameElementClass);
            var GeneralLimitAnnotations = ElementClassAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("GeneralLimit"));
            if(GeneralLimitAnnotations.Count() == 0)
                GeneralLimitAnnotations = Game.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("GeneralLimit"));

            var GeneralLimitAnnotation = GeneralLimitAnnotations.First();
            var generalCostAnnotationFound = (GeneralLimitAnnotation != null);
            if (generalCostAnnotationFound)
            {
                var ElementValue = GeneralLimitAnnotation.TaxonomySubject.ToString();
                if (string.IsNullOrEmpty(ElementValue))
                    return null;

                var GeneralCostProperty = Game.Ontology.Model.PropertyModel.SelectProperty(ElementValue);
                ResultProperties.Add(GeneralCostProperty);
                FilterResultsCounter = ResultProperties.Count();
            }
            else
            {
                index = 0;
                ResultProperties = Game.Ontology.Model.PropertyModel.Where(entry => AvailableWords.Any(word => entry.ToString().Contains(word))).ToList();
                FilterResultsCounter = ResultProperties.Count();
                while (FilterResultsCounter > 1 && CompareList.Count() < ElementWords.Count())
                {
                    CompareList.Add(ElementWords.ElementAt(index));
                    ResultProperties = ResultProperties.Where(entry => CompareList.All(word => entry.ToString().Contains(word))).ToList();
                    FilterResultsCounter = ResultProperties.Count();
                    ++index;
                }
            }

            if (FilterResultsCounter > 0)
            {
                var currentResultPropertiesCount = ResultProperties.Count();
                RDFOntologyProperty property;
                if (generalCostAnnotationFound == false)
                {
                    if (currentResultPropertiesCount == 1)
                    {
                        property = ResultProperties.Single();
                        var propertyName = property.ToString();
                        var propertyShortName = propertyName.Split('#').Last();
                        var propertyWords = propertyShortName.Split('_').ToList();
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
                    var LimitProperty = ResultProperties.Single();
                    LimitPropertyString = LimitProperty.ToString();
                    var LimitPropertyName = LimitProperty.ToString().Split('#').Last();
                    RDFOntologyDatatypeProperty CharacterLimitProperty;
                    RDFOntologyTaxonomyEntry LimitPropertyAssertion;
                    if (CheckDatatypeProperty(LimitProperty.ToString()) == false)
                    {
                        CharacterLimitProperty = CreateDatatypeProperty(LimitPropertyName);
                        LimitPropertyAssertion = null;
                    }
                    else
                    {
                        CharacterLimitProperty = this.Ontology.Model.PropertyModel.SelectProperty(LimitProperty.ToString()) as RDFOntologyDatatypeProperty;
                        LimitPropertyAssertion = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(CharacterLimitProperty).Single();
                    }
                        
                    if (LimitPropertyAssertion == null)
                    {
                        IEnumerable<RDFOntologyTaxonomyEntry> ResultAnnotations = Game.Ontology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(LimitProperty);
                        FilterResultsCounter = ResultAnnotations.Count();
                        CompareList.Clear();

                        while (FilterResultsCounter > 1)
                        {
                            CompareList.Add(ElementWords.ElementAt(index));
                            ResultAnnotations = ResultAnnotations.Where(entry => CompareList.All(word => entry.ToString().Contains(word)));
                            FilterResultsCounter = ResultAnnotations.Count();
                            ++index;
                        }

                        if (ResultAnnotations.Count() > 0)
                        {
                            var LimitPropertyDefinition = ResultAnnotations.Single().TaxonomyObject.ToString();
                            if (LimitPropertyDefinition.Contains('^'))
                                LimitPropertyDefinition = LimitPropertyDefinition.Split('^').First();

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
                        var AvailablePointsValue = LimitPropertyAssertion.TaxonomyObject.ToString();
                        AvailablePointsValue = AvailablePointsValue.Split('^').First();
                        AvailablePoints = Convert.ToSingle(AvailablePointsValue);
                    }
                }
            }

            if ((FilterResultsCounter == 0) || LimitPropertyString == null)
            {
                var parents = GetParentClasses(LimitPropertyString);
                if (parents != null)
                {
                    var parentsList = parents.Split('|').ToList();
                    foreach (string parent in parentsList)
                    {
                        if (AvailablePoints == null)
                        {
                            var parentHasAvailablePoints = GetAvailablePoints(parent, out double? parentAvailablePoints);
                            if (parentHasAvailablePoints != null)
                            {
                                LimitPropertyString = parentHasAvailablePoints;
                                AvailablePoints = parentAvailablePoints;
                            }
                        }
                    }
                }
            }
            return LimitPropertyString;
        }//MODIFIED

        /// <summary>
        /// Returns the limit of the element given.
        /// </summary>
        /// <param name="ElementName">Name of the element</param>
        /// <param name="LimitValue">Limit value obtained</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetLimit(string stageName, bool isGeneral = false, bool editGeneralLimit = false)
        {
            // Limite de etapa se actualiza de 630 a 750 en esta funcion. Comprobar por qué
            var game = DependencyHelper.CurrentContext.CurrentGame;
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var CharacterProperties = character.GetCharacterProperties();
            var CharacterStageString = string.Empty;
            var CharacterLimitString = string.Empty;
            var LimitName = string.Empty;
            var LimitValueString = string.Empty;
            if (stageName.Contains('#'))
                stageName = stageName.Split('#').Last();

            var stageString = character.GetString(stageName);
            var stageClass = game.Ontology.Model.ClassModel.SelectClass(stageString);
            if (stageClass != null)
            {

                string limitAnnotationName = string.Empty;
                var ClassCustomAnnotations = game.Ontology.Model.ClassModel.Annotations.CustomAnnotations;
                if (isGeneral)
                {
                    limitAnnotationName = "GeneralLimit";
                    var PropertyCustomAnnotations = game.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains(limitAnnotationName));
                    if (PropertyCustomAnnotations.Count() > 0)
                    {
                        LimitName = PropertyCustomAnnotations.Single().TaxonomySubject.ToString().Split('#').Last();
                        bool propertyFound = CharacterProperties.ContainsKey(LimitName);
                        if (!propertyFound)
                        {
                            var LimitPropertyString = character.GetString(LimitName);
                            var property = game.Ontology.Model.PropertyModel.SelectProperty(LimitPropertyString);
                            if (property != null)
                            {
                                var PropertyDefinitionAnnotationEntries = game.Ontology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(property);
                                if (PropertyDefinitionAnnotationEntries.EntriesCount > 0)
                                {
                                    CharacterLimitString = $"{character.Context}{LimitName}";
                                    LimitName = PropertyDefinitionAnnotationEntries.Single().TaxonomyObject.ToString().Split('^').First();
                                    LimitValueString = character.GetValue(LimitName).ToString();
                                    character.UpdateDatatypeAssertion(CharacterLimitString, LimitValueString);
                                }
                            }
                        }
                    }
                }
                else
                {
                    limitAnnotationName = "StageLimit";
                    var StageCustomAnnotations = ClassCustomAnnotations.SelectEntriesBySubject(stageClass);
                    if (StageCustomAnnotations.EntriesCount > 0)
                    {
                        var AnnotationEntries = StageCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Split('#').Last() == limitAnnotationName);
                        if (AnnotationEntries.Count() > 0)
                        { // Sujeto Predicado Objeto
                            LimitName = AnnotationEntries.Single().TaxonomyObject.ToString().Split('^').First();
                            bool propertyFound = CharacterProperties.ContainsKey(GetString(LimitName,true));
                            if (!propertyFound)
                            {
                                CharacterLimitString = $"{character.Context}{LimitName}";
                                var GameLimitString = character.GetString(LimitName);
                                var GameLimitProperty = game.Ontology.Model.PropertyModel.SelectProperty(GameLimitString);
                                var GamePropertyIsDefinedByAnnotations = game.Ontology.Model.PropertyModel.Annotations.IsDefinedBy;
                                var GameLimitIsDefinedByAnnotations = GamePropertyIsDefinedByAnnotations.SelectEntriesBySubject(GameLimitProperty);
                                if (GameLimitIsDefinedByAnnotations.EntriesCount > 0)
                                {
                                    var definitionValue = GameLimitIsDefinedByAnnotations.Single().TaxonomyObject.ToString().Split('^').First();
                                    var definitionString = character.GetString(definitionValue);
                                    var definitionProperty = game.Ontology.Model.PropertyModel.SelectProperty(definitionString);
                                    var GameDefinitionIsDefinedByAnnotations = GamePropertyIsDefinedByAnnotations.SelectEntriesBySubject(definitionProperty);
                                    if (GameDefinitionIsDefinedByAnnotations.EntriesCount > 0)
                                    {
                                        definitionValue = GameDefinitionIsDefinedByAnnotations.Single().TaxonomyObject.ToString().Split('^').First();
                                        LimitValueString = character.GetValue(definitionValue).ToString().Split('^').First();
                                        character.UpdateDatatypeAssertion(CharacterLimitString, LimitValueString);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                LimitName = stageName;
                var stageProperty = game.Ontology.Model.PropertyModel.SelectProperty(stageString);
                if (stageProperty != null)
                {
                    var PropertyCustomAnnotations = game.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(stageProperty);
                    if (PropertyCustomAnnotations.EntriesCount > 0)
                    {
                        var limitAnnotationName = isGeneral ? "GeneralLimit" : "StageLimit";
                        var LimitAnnotationEntries = PropertyCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Split('#').Last() == limitAnnotationName);
                        if(LimitAnnotationEntries.Count() > 0)
                        {
                            CharacterLimitString = $"{character.Context}{LimitName}";
                            LimitName = LimitAnnotationEntries.Single().TaxonomyObject.ToString().Split('^').First();
                            LimitValueString = character.GetValue(LimitName).ToString();
                            character.UpdateDatatypeAssertion(CharacterLimitString, LimitValueString);
                        }                        
                    }
                }
            }
            if(string.IsNullOrEmpty(LimitName))
            {
                var stageParentClassString = game.Ontology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(stageClass).Single().TaxonomyObject.ToString();
                LimitName = this.GetLimit(stageParentClassString, isGeneral, editGeneralLimit);
            }
            return LimitName;
        }//MODIFIED

        /// <summary>
        /// Returns item unitary cost for the view of given stage .
        /// </summary>
        /// <param name="itemName">Name of the element</param>
        /// <param name="stageName">Name of the stage</param>
        /// <returns></returns>
        public double GetStep(string itemName, string stageName)
        {
            double step = 1;
            var itemString = this.GetString(itemName , false);
            var characterTypeStageName = this.GetCreationSchemeRootClass().Split('#').Last();
            var characterTypeStageString = DependencyHelper.CurrentContext.CurrentCharacter.GetString(characterTypeStageName, true);
            var characterTypePropertyString = this.GetObjectPropertyAssociated(characterTypeStageString, null, true);
            var characterTypeProperty = this.Ontology.Model.PropertyModel.SelectProperty(characterTypePropertyString);
            var characterTypeFact = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(characterTypeProperty).Single().TaxonomyObject;
            var characterTypeAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(characterTypeFact);
            bool stepFound = false;

            while(!stepFound && GetParentClasses(itemString).Count() > 0)
            {
                foreach (var entry in characterTypeAssertions)
                {
                    var predicate = entry.TaxonomyPredicate;
                    var predicateName = predicate.ToString();
                    var itemNameWords = itemName.Split('_').ToList();
                    if (predicateName.Contains("Coste"))
                    {
                        var predicateNameWords = predicateName.Split('#').Last().Split('_').ToList();
                        if (itemNameWords.Any(itemWord => predicateNameWords.Any(predicateWord => itemWord == predicateWord)))
                        {
                            var stageWords = stageName.Split('_').ToList();
                            var itemWords = itemName.Split('_').ToList();
                            var filteredPredicateName = predicateName;
                            foreach (var word in stageWords)
                                filteredPredicateName = filteredPredicateName.Replace(word , "");
                            foreach (var word in itemWords)
                                filteredPredicateName = filteredPredicateName.Replace(word , "");

                            if (!string.IsNullOrEmpty(filteredPredicateName))
                            {
                                var predicateAssertionEntries = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate);
                                if (predicateAssertionEntries.Count() > 0)
                                {
                                    stepFound = true;
                                    var predicateAssertion = predicateAssertionEntries.Single();
                                    step = Convert.ToInt32(predicateAssertion.TaxonomyObject.ToString().Split('^').First());
                                }
                            }
                        }
                    }
                    else
                    {
                        var game = DependencyHelper.CurrentContext.CurrentGame;
                        var itemFact = game.Ontology.Data.SelectFact(itemString);
                        if (itemFact != null)
                        {
                            var itemClass = game.Ontology.Data.Relations.ClassType.SelectEntriesBySubject(itemFact).Single().TaxonomyObject;
                            var itemClassName = itemClass.ToString().Split('#').Last();
                            var itemClassWords = itemClassName.Split('_').ToList();
                            if (predicateName.Contains("Coste"))
                            {
                                if (itemClassWords.Any(word => predicateName.Contains(word)))
                                {
                                    var predicateAssertionEntries = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate);
                                    if (predicateAssertionEntries.Count() > 0)
                                    {
                                        stepFound = true;
                                        var predicateAssertion = predicateAssertionEntries.Single();
                                        step = Convert.ToInt32(predicateAssertion.TaxonomyObject.ToString().Split('^').First());
                                    }
                                }
                            }
                        }
                    }
                }
                if (!stepFound)
                {
                    var parents = string.Empty;
                    if (CheckClass(itemString))
                        parents = GetParentClasses(itemString);
                    else
                        parents = GetElementClass(itemString).ShortName;
                    if (!string.IsNullOrEmpty(parents))
                    {
                        var parentList = parents.Split('|').ToList();
                        foreach (string parent in parentList)
                            step = GetStep(parent , stageName);
                        stepFound = true;
                    }
                }
            }
            return step;
        }

        /// <summary>
        /// Returns value of the given limit
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public double GetLimitValue (string name)
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            var valueString = string.Empty;
            var characterAssertions = character.GetCharacterProperties();
            var propertyString = $"{character.Context}{name}";
            var propertyFound = characterAssertions.TryGetValue(propertyString, out var valueList);

            if (!propertyFound)
            {
                propertyString = character.GetString(name);
                var property = game.Ontology.Model.PropertyModel.SelectProperty(propertyString);
                var gamePropertyAnnotations = game.Ontology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(property);
                if (gamePropertyAnnotations.EntriesCount > 0)
                {
                    var definition = gamePropertyAnnotations.Single().TaxonomyObject.ToString().Split('^').First();
                    valueString = character.GetValue(definition).ToString();
                }
            }
            else
                valueString = valueList.Single();

            var LimitValue = Math.Round(Convert.ToDouble(valueString.Split('^').First()));
            return LimitValue;
        }

        /// <summary>
        /// Returns the URI of the object property associated to the stage given
        /// </summary>
        /// <param name="stage">Name of the stage</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetObjectPropertyAssociated (string stageString, Item currentItem = null, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            var currentStageString = stageString;
            var stageShortName = stageString.Split('#').Last();
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
                currentStageString = $"{this.Context}{stageShortName}";
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var propertyString= string.Empty;
            var propertyNameFound = false;
            var StageWords = stageShortName.Split('_').ToList();
            var wordCounter = StageWords.Count();
            while (propertyNameFound == false && wordCounter > 0)
            {
                var ObjectPropertyName = "tiene";
                for (int i = 0; i < wordCounter; ++i)
                    ObjectPropertyName += StageWords.ElementAt(i);

                var ObjectPropertyAssertions = CurrentOntology.Model.PropertyModel.Where(entry => entry.Range != null && entry.Range.ToString().Contains(stageShortName));
                if (ObjectPropertyAssertions.Count() > 1)
                {
                    ObjectPropertyAssertions = ObjectPropertyAssertions.Where(entry => entry.ToString().Contains(ObjectPropertyName));
                    if(ObjectPropertyAssertions.Count() >1)
                    {
                        //Quedarse con una única propiedad 
                        var currentClassName = currentItem.Class;
                        ObjectPropertyAssertions = ObjectPropertyAssertions.Where(entry => entry.Range.Value.ToString().Contains(currentClassName));
                        if (ObjectPropertyAssertions.Count() > 0)
                        {
                            propertyString = ObjectPropertyAssertions.Single().ToString();
                            propertyNameFound = true;
                            break;
                        }
                    }
                    else if (ObjectPropertyAssertions.Count() == 1)
                    {
                        propertyString = ObjectPropertyAssertions.Single().ToString();
                        propertyNameFound = true;
                        break;
                    }
                }
                else if (ObjectPropertyAssertions.Count() == 1)
                {
                    propertyString = ObjectPropertyAssertions.Single().ToString();
                    propertyNameFound = true;
                    break;
                }
                --wordCounter;
            }
            if (wordCounter == 0)
            {
                var parents = GetParentClasses(stageString,applyOnCharacter);
                if (parents != null)
                {
                    var parentList = parents.Split('|').ToList();
                    foreach (var parent in parentList)
                    {
                        if (propertyNameFound == false)
                        {
                            propertyString = GetObjectPropertyAssociated(parent, null, applyOnCharacter);
                            if (propertyString != null)
                                propertyNameFound = true;
                        }
                    }
                }
            }
            if (!propertyString.Contains('#'))
                propertyString = GetString(propertyString);
            return propertyString;
        }//MODIFIED

        /// <summary>
        /// Returns a SortedList with the order and the name of the substages given a stage
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public SortedList<int, string> GetOrderedSubstages (string stageString, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            string currentStageString = stageString;
            string stageShortName = stageString.Split('#').Last();

            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
                currentStageString = $"{this.Context}{stageShortName}";
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var Substages = GetSubClasses(currentStageString);
            var OrderedSubstages = new SortedList<int, string>();
            foreach (var substage in Substages)
            {
                var SubstageClass = CurrentOntology.Model.ClassModel.SelectClass(currentStageString);
                var Substage_OrderEntry = CurrentOntology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(SubstageClass).Where(entry => entry.TaxonomyPredicate.ToString().Contains("Substage_Order")).Single();
                if (Substage_OrderEntry != null)
                    OrderedSubstages.Add(Convert.ToInt32(Substage_OrderEntry.TaxonomyObject.ToString().Split('^').First()), currentStageString);
            }
            return new SortedList<int, string>(OrderedSubstages);
        }//MODIFIED //UNUSED

        /// <summary>
        /// Returns a string containing the names of the types of the given element
        /// </summary>
        /// <param name="elementName">Name of the element</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetParentClasses (string elementString, bool applyOnCharacter = false)
        {
            var parent = string.Empty;
            if(!string.IsNullOrEmpty(elementString))
            {
                RDFOntology CurrentOntology;
                string CurrentContext;
                string currentElementString = elementString;
                var elementShortName = elementString.Split('#').Last();
                if (applyOnCharacter)
                {
                    CurrentOntology = this.Ontology;
                    CurrentContext = this.Context;
                    currentElementString = $"{this.Context}{elementShortName}";
                }
                else
                {
                    CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                    CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
                }

                var elementClass = CurrentOntology.Model.ClassModel.SelectClass(currentElementString);
                if (elementClass != null)
                {
                    var elementClassEntries = CurrentOntology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(elementClass);
                    foreach (var entry in elementClassEntries)
                        parent += $"{entry.TaxonomyObject}{"|"}";
                    if (parent != null)
                        if (parent.EndsWith('|'))
                            parent = parent[0..^1];
                }
                else
                {
                    var elementFact = CurrentOntology.Data.SelectFact(currentElementString);
                    var ElementClassAssertions = CurrentOntology.Data.Relations.ClassType.SelectEntriesBySubject(elementFact);
                    foreach (var entry in ElementClassAssertions)
                        parent += $"{entry.TaxonomyObject.ToString()}{"|"}";
                    if (parent != null)
                        if (parent.EndsWith('|'))
                            parent = parent[0..^1];
                }

            }
            return parent;
        }//MODIFIED

        /// <summary>
        /// Returns the general cost of an element given its name and the current stage
        /// </summary>
        /// <param name="stage">Name of the current stage</param>
        /// <param name="ElementName">Name of the element</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetGeneralCost (string stageString, string elementString, out float generalCost, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            string currentStageString = stageString;
            string currentElementString = elementString;
            var currentStageShortName = stageString.Split('#').Last();
            var currentElementShortName = elementString.Split('#').Last();

            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
                currentStageString = $"{this.Context}{currentStageShortName}";
                currentElementString = $"{this.Context}{currentElementShortName}";
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var GeneralCostString = string.Empty;
            generalCost = 0;
            var CostWords = new List<string> { "Coste", "Cost", "Coût" };
            var ItemFact = CurrentOntology.Data.SelectFact(currentElementString);
            var ItemFactAssertions = CurrentOntology.Data.Relations.Assertions.SelectEntriesBySubject(ItemFact);
            var ItemCosts = ItemFactAssertions.Where(entry => CostWords.Any(word => entry.ToString().Contains(word)));
            var GeneralCostPredicateName = CheckGeneralCost(stageString);
            ItemCosts = ItemCosts.Where(entry => entry.TaxonomyPredicate.ToString().Contains(GeneralCostPredicateName));
            if (ItemCosts.Count() > 0)
            {
                var ItemCostEntry = ItemCosts.Single();
                generalCost = Convert.ToSingle(ItemCostEntry.TaxonomyObject.ToString().Split('^').First());
                GeneralCostString = ItemCostEntry.TaxonomyPredicate.ToString();
            }

            return GeneralCostString;
        }//MODIFIED //UNUSED

        /// <summary>
        /// Returns the partial cost of an element given its name and the current stage
        /// </summary>
        /// <param name="stage">Name of the current stage</param>
        /// <param name="ElementName">Name of the element</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetPartialCost (string stageString, string elementString, out float partialCost, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            string currentStageString = stageString;
            string currentElementString = elementString;
            var currentStageShortName = stageString.Split('#').Last();
            var currentElementShortName = elementString.Split('#').Last();

            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
                currentStageString = $"{this.Context}{currentStageShortName}";
                currentElementString = $"{this.Context}{currentElementShortName}";
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            partialCost = 0;
            var CostWords = new List<string> { "Coste", "Cost", "Coût" };
            var ItemFact = CurrentOntology.Data.SelectFact(currentElementString);
            var ItemFactAssertions = CurrentOntology.Data.Relations.Assertions.SelectEntriesBySubject(ItemFact);
            var ItemCosts = ItemFactAssertions.Where(entry => CostWords.Any(word => entry.ToString().Contains(word)));
            if (ItemCosts.Count() > 1)
            {
                var GeneralCostPredicateName = CheckGeneralCost(currentStageString);
                ItemCosts = ItemCosts.Where(entry => !entry.TaxonomyPredicate.ToString().Contains(GeneralCostPredicateName));
            }

            var ItemCostEntry = ItemCosts.Single();
            if (ItemCostEntry != null)
                if (ItemCostEntry != null)
                    partialCost = Convert.ToSingle(ItemCostEntry.TaxonomyObject.ToString().Split('^').First());

            return ItemCostEntry.TaxonomyPredicate.ToString();
        }//MODIFIED //UNUSED

        /// <summary>
        /// Returns a collection of groups if given class has subclasses.
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        public ObservableCollection<Group> GetSubClasses (string classString, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            string currentClassString = classString;
            var currentClassShortName = classString.Split('#').Last();
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
                currentClassString = $"{this.Context}{currentClassShortName}";
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            ObservableCollection<Group> subclasses = new ObservableCollection<Group>();
            var currentClass = CurrentOntology.Model.ClassModel.SelectClass(classString);
            var currentClassDescription = this.GetElementDescription(classString, applyOnCharacter);

            var currentClassModel = CurrentOntology.Model.ClassModel.GetSubClassesOf(currentClass);
            if (currentClassModel.ClassesCount > 0)
            {
                var enumerator = currentClassModel.ClassesEnumerator;
                enumerator.Reset();
                while(enumerator.MoveNext())
                {
                    var currentSubclass = enumerator.Current;
                    var currentSubclassString = enumerator.Current.ToString();
                    var itemList = this.GetIndividuals(currentSubclassString, applyOnCharacter);
                    var currentGroup = new Group(currentSubclassString, itemList);
                    subclasses.Add(currentGroup);
                }
            }
            else
                subclasses = null;
           return subclasses;
        }//MODIFIED

        /// <summary>
        /// Returns the correspondent URI to the datatype given
        /// </summary>
        /// <param name="datatypeName"></param>
        /// <returns></returns>
        public string GetDatatypeUri (string datatypeName)
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
        /// Returns a value given a string with the formula, the current item name and the user input
        /// </summary>
        /// <param name="valueDefinition">Formula definition</param>
        /// <param name="itemName">Name of the current item</param>
        /// <param name="User_Input">Value given by the user to the current item</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        /// 
        public string GetString(string elementName, bool applyToCharacter = false)
        {
            RDFOntology GameOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
            RDFOntology CharacterOntology = this.Ontology;
            string elementString = string.Empty;
            bool found = false;

            if(applyToCharacter == true)
            {
                var characterFactList = CharacterOntology.Data.Where(item => item.ToString().Contains(elementName));
                if (characterFactList.Count() > 0)
                {
                    foreach (var item in characterFactList)
                    {
                        if (item.ToString().Split('#').Last() == elementName)
                        {
                            elementString = item.ToString();
                            found = true;
                            break;
                        }
                    }
                }
                if (found == false)
                {
                    var characterClassList = CharacterOntology.Model.ClassModel.Where(item => item.ToString().Contains(elementName));
                    if (characterClassList.Count() > 0)
                    {
                        foreach (var item in characterClassList)
                        {
                            if (item.ToString().Split('#').Last() == elementName)
                            {
                                elementString = item.ToString();
                                found = true;
                                break;
                            }
                        }
                    }
                }
                if (found == false)
                {
                    var characterPropertyList = CharacterOntology.Model.PropertyModel.Where(item => item.ToString().Contains(elementName));
                    if (characterPropertyList.Count() > 0)
                    {
                        foreach (var item in characterPropertyList)
                        {
                            if (item.ToString().Split('#').Last() == elementName)
                            {
                                elementString = item.ToString();
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                var gameFactList = GameOntology.Data.Where(item => item.ToString().Contains(elementName));
                if (gameFactList.Count() > 0)
                {
                    foreach (var item in gameFactList)
                    {
                        if (item.ToString().Split('#').Last() == elementName)
                        {
                            elementString = item.ToString();
                            found = true;
                            break;
                        }
                    }
                }
                if (found == false)
                {
                    var gameClassList = GameOntology.Model.ClassModel.Where(item => item.ToString().Contains(elementName));
                    if (gameClassList.Count() > 0)
                    {
                        foreach (var item in gameClassList)
                        {
                            if (item.ToString().Split('#').Last() == elementName)
                            {
                                elementString = item.ToString();
                                found = true;
                                break;
                            }
                        }
                    }
                }
                if (found == false)
                {
                    var gamePropertyList = GameOntology.Model.PropertyModel.Where(item => item.ToString().Contains(elementName));
                    if (gamePropertyList.Count() > 0)
                    {
                        foreach (var item in gamePropertyList)
                        {
                            if (item.ToString().Split('#').Last() == elementName)
                            {
                                elementString = item.ToString();
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }
            return elementString;
        }

        /// <summary>
        /// Returns the value of item formula, given the item, the user imput (if needed) and its membership to the character.
        /// </summary>
        /// <param name="valueDefinition"></param>
        /// <param name="itemName"></param>
        /// <param name="User_Input"></param>
        /// <param name="applyOnCharacter"></param>
        /// <returns></returns>
        public float GetValue (string valueDefinition, string itemName = null, string User_Input = null, bool applyOnCharacter = false)
        {            
            // 0 - Select current ontology using applyOnCharacter (true: characterOntology, false: gameOntology)
            RDFOntology CurrentOntology;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            var CharacterOntology = this.Ontology;
            var GameOntology = game.Ontology;

            if (applyOnCharacter)
                CurrentOntology = this.Ontology;
            else
                CurrentOntology = GameOntology;

            // 1 - Initalization of variables
            var SubjectRef = string.Empty;
            var CurrentValue = string.Empty;
            var Operators = new List<string> { "+", "-", "*", "/", "%", "<", ">", "<=", ">=", "=", "!=" };
            var currentList = new List<dynamic>();

            // 2 - Get formula substituting word "Item" by current item name. Then, split formula in formulaElements, removing leftover spaces
            var formula = valueDefinition.Replace("Item", itemName).Replace("__", "_");
            var formulaElements = formula.Split(':').Select(innerItem => innerItem.Trim()).ToList();

            // 3 - Evaluate each element of the formula, using an index to know its position inside the formula
            for (int index = 0; index < formulaElements.Count(); ++index)
            {
                // Get element from current index value
                var element = formulaElements.ElementAt(index);

                // if element contains "Ref", replace it with SubjectRef value
                if(element.Contains("Ref"))
                    element = element.Replace("Ref", SubjectRef);

                // 3.1 Check if element is a numeric value
                if (Regex.IsMatch(element, @"\d") && (!Regex.IsMatch(element, @"\D")))
                {
                    //3.1.1 Check if element is in the first position. 
                    if (index == 0)
                        // Save element value in CurrentValue if true. Otherwise, evaluate next element.
                        CurrentValue = element.ToString();
                    else
                        continue;
                } // Ended element as numeric value
                // 3.2 Check if element is a datatype property in the current ontology
                else if (CheckDatatypeProperty(GetString(element, applyOnCharacter), applyOnCharacter))
                {
                    // 3.2.1 Check if element is in the first position. 
                    if (index == 0)
                    {
                        // 3.2.1.1 If element is a datatype property, check if the character has any entry with it
                        // 3.2.1.1 Get character fact
                        var CharacterFact = CharacterOntology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
                        // 3.2.1.2 Check if character has element datatype property defined
                        var characterElementPropertyString = GetString(element, true);
                        var characterElementProperty = CharacterOntology.Model.PropertyModel.SelectProperty(characterElementPropertyString) as RDFOntologyDatatypeProperty;

                        if (characterElementProperty == null)
                        {
                            /* 3.2.1.2.1 If character does not have element datatype property defined, it is time to define it. For doing it, 
                             * we will search inside the game ontology the same property, and we will assign its default value, which will be included as an
                             * "IsDefinedBy" annotation property.*/
                            var gameElementPropertyString = GetString(element);
                            var gameElementProperty = GameOntology.Model.PropertyModel.SelectProperty(gameElementPropertyString);
                            // 3.2.1.2.2 Check if gameElementProperty exists, to prevent unhandled exceptions
                            if (gameElementProperty != null)
                            {
                                var GameOntologyPropertyModelIsDefinedByAnnotations = GameOntology.Model.PropertyModel.Annotations.IsDefinedBy;
                                var gameElementPropertyIsDefinedByAnnotations = GameOntologyPropertyModelIsDefinedByAnnotations.SelectEntriesBySubject(gameElementProperty);
                                //3.2.1.2.3 Check if the annotation exists, to prevent unhandled exceptions
                                if (gameElementPropertyIsDefinedByAnnotations.EntriesCount > 0)
                                {
                                    //3.2.1.2.3A.1 The definition is the object of the taxonomy entry. We take it in two parts, its value and its type.
                                    var definition = gameElementPropertyIsDefinedByAnnotations.Single().TaxonomyObject.ToString().Split('^').First();
                                    var definitionType = gameElementPropertyIsDefinedByAnnotations.Single().TaxonomyObject.ToString().Split('#').Last();
                                    //3.2.1.2.3A.2 To get its value, we call the function GetValue with it, and convert it to string
                                    var gameElementPropertyValue = GetValue(definition).ToString();
                                    //3.2.1.2.3A.3 Now we can add the property with its default value inside the character (So we use the character context strings)
                                    AddDatatypeProperty($"{this.Context}{FileService.EscapedName(this.Name)}", gameElementPropertyString, gameElementPropertyValue, definitionType);
                                }
                                else
                                {
                                    // If the annotation does not exist, then we use the user input to add a value to the property
                                    // 3.2.1.2.3B.1 We access the range of the property, so we can know its value type
                                    var valuetype = gameElementProperty.Range.ToString().Split('#').Last();
                                    // 3.2.1.2.3B.2 Now we can add the property with the user input inside the character (So we use the character context strings)
                                    AddDatatypeProperty($"{this.Context}{FileService.EscapedName(this.Name)}", characterElementPropertyString, User_Input.ToString(), valuetype);
                                }
                                // After adding the property, now we can access it
                                // 3.2.1.2.4 Get the character property added from the character ontology assertions using the character fact and the propertyname as filters
                                var CharacterAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                                characterElementProperty = CharacterAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains(element)).Single().TaxonomyPredicate as RDFOntologyDatatypeProperty;
                            }
                        }
                        // Now its time to get the property value as the current value
                        // 3.2.1.3 Get property assertion in character
                        var CharacterPropertyAssertionEntries = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact).SelectEntriesByPredicate(characterElementProperty);
                        // Check if property has been asserted in character
                        if(CharacterPropertyAssertionEntries.Count() < 1)
                        {
                            var definition = CharacterOntology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(characterElementProperty).Single().TaxonomyObject.ToString().Split('^').First();
                            var propertyValue = this.GetValue(definition).ToString();
                            var propertyType = characterElementProperty.Range.ToString().Split('#').Last();
                            AddDatatypeProperty($"{this.Context}{FileService.EscapedName(this.Name)}" , characterElementPropertyString , propertyValue , propertyType);
                        }
                        var CharacterPropertyAssertion = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact).SelectEntriesByPredicate(characterElementProperty).Single();
                        // 3.2.1.4 Get property value
                        var propertyValueString = CharacterPropertyAssertion.TaxonomyObject.ToString();
                        // 3.2.1.5 If the value is float, convert it to string. Otherwise, assign the value to CurrentValue and SubjectRef
                        if (propertyValueString.Contains("float"))
                        {
                            propertyValueString = propertyValueString.Split('^').First();
                            // If the value contains decimals, then we only get the integer part of the value.
                            if (propertyValueString.Contains(','))
                                CurrentValue = propertyValueString.Split(',').ElementAtOrDefault(0);
                            else
                                CurrentValue = propertyValueString;
                            SubjectRef = CurrentValue;
                        }
                        else
                        {
                            CurrentValue = propertyValueString.Split('^').First();
                            SubjectRef = CurrentValue;
                        }
                    }
                    else
                    {
                        // If this is not the first element, then it is necessary to check which element is before this one
                        // 3.2.2.1 Get previous element 
                        var previousElement = formulaElements.ElementAt(index - 1);

                        // 3.2.2.2 Check if the element is an individual in the game ontology
                        if (CheckIndividual(GetString(previousElement)))
                        {
                            // 3.2.2.3 Check if the element is has a version inside the character ontology. If it does not, then we create one
                            var individualFact = CheckIndividual(GetString(previousElement, true)) ? CharacterOntology.Data.SelectFact(GetString(previousElement, applyOnCharacter)) : CreateIndividual(previousElement);
                            // 3.2.2.4 Now its time to find the current element inside the character ontology. As before, if it is not inside the character ontology, we create it.
                            var elementProperty = CheckDatatypeProperty(GetString(element, true)) ? CharacterOntology.Model.PropertyModel.SelectProperty(GetString(element, true)) : CreateDatatypeProperty(element);
                            // 3.2.2.5 Look if there is any assertion of the element property of the individual fact inside the character ontology
                            var individualFactElementPropertyAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(individualFact).SelectEntriesByPredicate(elementProperty);
                            // 3.2.2.6 Check if there is any assertion, to prevent unhandled exceptions
                            if (individualFactElementPropertyAssertions.EntriesCount > 0)
                            {
                                // 3.2.2.7 Get assertion object value
                                var propertyValueString = individualFactElementPropertyAssertions.Single().TaxonomyObject.ToString();
                                // 3.2.2.8 If the value is float, convert it to string. Otherwise, assign the value to CurrentValue and SubjectRef
                                if (propertyValueString.Contains("float"))
                                {
                                    propertyValueString = propertyValueString.Split('^').First();
                                    // If the value contains decimals, then we only get the integer part of the value.
                                    if (propertyValueString.Contains(','))
                                        CurrentValue = propertyValueString.Split(',').ElementAtOrDefault(0);
                                    else
                                        CurrentValue = propertyValueString;
                                    SubjectRef = CurrentValue;
                                }
                                else
                                {
                                    CurrentValue = propertyValueString.Split('^').First();
                                    SubjectRef = CurrentValue;
                                }
                            }
                            else
                            {
                                // In case that there is not any elementPropertyAssertion for individualFact, then our current value should stay as 0.
                                CurrentValue = 0.ToString();
                                SubjectRef = CurrentValue;
                            }
                        }
                    }
                } // Ended element as datatype property 
                // 3.3 Check if element is an object property in the current ontology
                else if (CheckObjectProperty(GetString(element, applyOnCharacter), applyOnCharacter))
                {
                    // 3.3.1 Get current element property and the previous element 
                    var elementPropertyName = formulaElements.ElementAt(index);
                    var previousElement = string.Empty;
                    if (index > 0)
                        previousElement = formulaElements.ElementAt(index - 1);

                    // Declare a variable to know which property are we referring at every moment
                    var currentPropertyName = elementPropertyName;
                    // 3.3.2 Check if the previous element is not an operator.
                    if (!Operators.Any(op => previousElement == op))
                    {
                        // 3.3.2.1 Get character fact
                        var characterString = $"{this.Context}{FileService.EscapedName(this.Name)}";
                        var characterFact = CharacterOntology.Data.SelectFact(characterString);
                        // 3.3.2.2 Get character class
                        var characterClass = CharacterOntology.Data.Relations.ClassType.SelectEntriesBySubject(characterFact).Single().TaxonomyObject;
                        // 3.3.2.3 Get characterClassName
                        var characterClassName = characterClass.ToString().Split('#').Last();
                        // 3.3.2.4 Get itemName class
                        var itemClassName = itemName != null ? GetElementClass(itemName).FullName.Split('#').Last() : string.Empty;
                        // 3.3.2.5 Get character assertions
                        var characterAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(characterFact);
                        /* Now we are trying to find the first word of any datatype property related to the character. 
                         * To do it, we look for any datatype property which has the characterClass as domain */
                        // 3.3.2.6 Declare some variables to get the first word of any datatype property.
                        var datatypePropertyFound = false;
                        var datatypePropertyFirstWord = string.Empty;
                        RDFOntologyFact SubjectFact = null, ObjectFact = null;
                        RDFOntologyObjectProperty PredicateProperty;

                        // 3.3.2.7 Process every property of the character
                        foreach (var entry in characterAssertions)
                        {
                            // 3.3.2.7.1 Get current entry property
                            var entryProperty = entry.TaxonomyPredicate;
                            //  3.3.2.7.2 Check if entry property is  a datatypeProperty
                            if (CheckDatatypeProperty(entryProperty.ToString()))
                            {
                                //  3.3.2.7.3 Check if the word has been found
                                if (!datatypePropertyFound)
                                {
                                    //  3.3.2.7.4 Cast entry property as a datatype property
                                    var entryDatatypeProperty = entryProperty as RDFOntologyDatatypeProperty;
                                    //  3.3.2.7.5 Check if entry datatype property has a domain declared
                                    if (entryDatatypeProperty.Domain != null)
                                    {
                                        //  3.3.2.7.6 Check if the domain of the entry datatype property contains characterClassName
                                        if (entryDatatypeProperty.ToString().Contains(characterClassName))
                                        {
                                            //  3.3.2.7.7 Get the first word of the datatype property, and check that it has been found
                                            datatypePropertyFound = true;
                                            datatypePropertyFirstWord = datatypePropertyFirstWord.ToString().Split('#').Last().Split('_').First();
                                        }
                                    }
                                }
                            }
                        }
                        /* Its time to build the triple that has the value we are loooking for. The first thing we need is the subject of the triple.
                         * To find the subject, we need to check if there is any item passed as parameter. If there is, then the item is the subject. 
                         * Otherwise, the subject is the character fact */
                        // 3.3.2.8 Check if there is any itemClass
                        if (itemClassName != string.Empty && itemClassName.Contains(datatypePropertyFirstWord))
                        {
                            // 3.3.2.8A.1 Check if the itemClassName contains the first word of any character datatype property
                            if (characterClassName.Split('_').Any(word => word.Contains(datatypePropertyFirstWord)))
                                // 3.3.2.8A.1A If it is contains the word, then the subject is the character fact
                                SubjectFact = characterFact;
                            else
                                // 3.3.2.8A.1B If it does not contain the word, then the subject is the item. If it is not declared inside the ontology, we create it.
                                SubjectFact = CheckIndividual(GetString(itemName, true)) ? CharacterOntology.Data.SelectFact(GetString(itemName, true)) : CreateIndividual(itemName);
                        }
                        else if (itemClassName == string.Empty)
                            // 3.3.2.8B If there is not any itemClass, the subject is the characterFact
                            SubjectFact = characterFact;
                        else
                        {
                            // 3.3.2.8C.1 In case there is an item class but it does not contain the first word of any datatype property, get the element property
                            PredicateProperty = CharacterOntology.Model.PropertyModel.SelectProperty(GetString(element, applyOnCharacter)) as RDFOntologyObjectProperty;
                            // 3.3.2.8C.2 The next step is check if the current property has characterFact as its domain
                            var PredicatePropertyDomain = PredicateProperty.Domain;
                            // Check if domain is null, to prevent unhandled exceptions
                            if (PredicatePropertyDomain != null)
                            {
                                // 3.3.2.8C.2 Check if element predicate has characterClass as domain
                                if (!PredicatePropertyDomain.ToString().Contains(characterClassName))
                                {
                                    // 3.3.2.8C.2A If it does not have it, then we have to check if the item passed by parameter is inside the character ontology. If it is not, we create it
                                    if (!CheckIndividual(GetString(itemName, true), applyOnCharacter))
                                        SubjectFact = CreateIndividual(itemName);
                                    else
                                        // 3.3.2.8C.2B If the element predicate domains is characterClass, then our subject is the characterFact.
                                        SubjectFact = characterFact;
                                }
                            }
                        }
                        /* Once we have the subject, then we need to get the predicate.*/
                        // 3.3.2.9 Check if the predicate exists inside the character ontology. If it does not exists, we create it
                        if (!CheckObjectProperty(GetString(elementPropertyName, true)))
                            PredicateProperty = CreateObjectProperty(GetString(elementPropertyName, true));
                        else
                            PredicateProperty = CharacterOntology.Model.PropertyModel.SelectProperty(GetString(elementPropertyName, true)) as RDFOntologyObjectProperty;

                        /* Once we have our subject and predicate, we can find the object we are looking for.*/
                        // 3.3.2.10 Get the triple assertions which have SubjectFact as subject.
                        var SubjectAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);
                        // Check if SubjectAssertions is not null, to prevent unhandled exceptions
                        if (SubjectAssertions.EntriesCount > 0)
                        {
                            // 3.3.2.11 Get SubjectAssertions which have PredicateProperty as predicate
                            var SubjectPredicateAssertions = SubjectAssertions.SelectEntriesByPredicate(PredicateProperty);
                            // Check if SubjectPredicateAssertions is not null, to prevent unhandled exceptions
                            if (SubjectPredicateAssertions.EntriesCount > 0)
                            {
                                // 3.3.2.12 Get triple object fact
                                ObjectFact = SubjectPredicateAssertions.Single().TaxonomyObject as RDFOntologyFact;
                                // 3.3.2.13 Get object fact name as reference
                                SubjectRef = ObjectFact.ToString().Split('#').Last();
                                // 3.3.2.14 Get next element of the formula
                                var nextElement = formulaElements.ElementAt(index + 1);
                                /* Replace "Item" and "Ref" strings by their values inside the nextElement name*/
                                nextElement = nextElement.Replace("Item", itemName).Replace("Ref", SubjectRef);
                                // 3.3.2.15 Create a counter for next elements
                                var nextElementCounter = 1;
                                // 3.3.2.16 Check if next property is a datatype property inside the character property
                                if (CheckDatatypeProperty(GetString(nextElement, true)))
                                {
                                    // 3.3.2.16A.1 If next property is a datatype property, get the property
                                    var nextDatatypeProperty = CharacterOntology.Model.PropertyModel.SelectProperty(GetString(nextElement, true)) as RDFOntologyDatatypeProperty;
                                    // 3.3.2.16A.2 Get next property first word
                                    var nextDatatypePropertyFirstWord = nextDatatypeProperty.ToString().Split('#').Last().Split('_').First();
                                    // 3.3.2.16A.3 Check if characterClassName contains next property first word. If it is true, the subject is the characterfact. If not, then the subject is the subject reference.
                                    SubjectFact = !characterClassName.Contains(nextDatatypePropertyFirstWord) ? CharacterOntology.Data.SelectFact(GetString(SubjectRef, true)) : characterFact;
                                    // 3.3.2.16A.4 Get subject fact assertions
                                    SubjectAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);
                                    // Check if SubjectAssertions is not null, to prevent unhandled exceptions
                                    if (SubjectAssertions.EntriesCount > 0)
                                    {
                                        // 3.3.2.16A.5 Get subject fact assertions which have nextDatatypeProperty as predicate
                                        SubjectPredicateAssertions = SubjectAssertions.SelectEntriesByPredicate(nextDatatypeProperty);
                                        // Check if SubjectPredicateAssertions is not null, to prevent unhandled exceptions
                                        if (SubjectPredicateAssertions.EntriesCount > 0)
                                        {
                                            // 3.3.2.16A.5A.1 If SubjectPredicateAsseritons is not null, then the object is our current value
                                            var nextPropertyValueString = SubjectPredicateAssertions.Single().TaxonomyObject.ToString();
                                            // 3.3.2.16A.5A.2 If the value is float, convert it to string. Otherwise, assign the value to CurrentValue and SubjectRef
                                            if (nextPropertyValueString.Contains("float"))
                                            {
                                                nextPropertyValueString = nextPropertyValueString.Split('^').First();
                                                // If the value contains decimals, then we only get the integer part of the value.
                                                if (nextPropertyValueString.Contains(','))
                                                    CurrentValue = nextPropertyValueString.Split(',').ElementAtOrDefault(0);
                                                else
                                                    CurrentValue = nextPropertyValueString;
                                                SubjectRef = CurrentValue;
                                            }
                                            else
                                            {
                                                CurrentValue = nextPropertyValueString.Split('^').First();
                                                SubjectRef = CurrentValue;
                                            }
                                        }
                                        else
                                        {
                                            // 3.3.2.16A.5B If SubjectPredicateAsseritons is null, then current value is 0
                                            CurrentValue = 0.ToString();
                                            SubjectRef = CurrentValue;
                                        }
                                    }
                                }
                                else
                                {
                                    // 3.3.2.16B.1 Declare variable to control if its the first tiem to check
                                    var firstTime = true;
                                    // 3.3.2.16B.2 if this is not the last property, get the next property
                                    while (CheckObjectProperty(GetString(nextElement, true)))
                                    {
                                        // 3.3.2.16B.2.1 Check if its the first time
                                        if (firstTime)
                                        {
                                            // 3.3.2.16B.2.1A Check if item exists inside the character ontology. If it does not exists, we create it.
                                            SubjectFact = CheckIndividual(GetString(itemName, true)) ? CharacterOntology.Data.SelectFact(GetString(itemName, true)) : CreateIndividual(GetString(itemName, true));
                                        }
                                        // 3.3.2.16B.2.2 Check if element property exists inside the character ontology. If it does not exists, we create it.
                                        var nextPredicateProperty = CheckObjectProperty(GetString(element, true)) ? CharacterOntology.Model.PropertyModel.SelectProperty(GetString(element, true)) as RDFOntologyObjectProperty : CreateObjectProperty(GetString(itemName, true));
                                        // 3.3.2.16B.2.3 Get subject fact assertions
                                        SubjectAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesByObject(SubjectFact);
                                        // Check if SubjectAssertions is not null, to prevent unhandled exceptions
                                        if (SubjectAssertions.EntriesCount > 0)
                                        {
                                            // 3.3.2.16B.2.4 Get subject fact assertions which have nextPredicateProperty as predicate
                                            SubjectPredicateAssertions = SubjectAssertions.SelectEntriesByPredicate(nextPredicateProperty);
                                            // Check if SubjectPredicateAssertions is not null, to prevent unhandled exceptions
                                            if (SubjectPredicateAssertions.EntriesCount > 0)
                                            {
                                                // 3.3.2.16B.2.5 Get object as SubjectFact
                                                SubjectFact = SubjectPredicateAssertions.Single().TaxonomyObject as RDFOntologyFact;
                                                // 3.3.2.16B.2.6 Increase property counter
                                                ++nextElementCounter;
                                                // 3.3.2.16B.2.7 Get next element
                                                nextElement = formulaElements.ElementAt(index + nextElementCounter);
                                            }
                                        }
                                    }
                                    currentPropertyName = nextElement;
                                }
                                // 3.3.2.17 Check if current element is datatype property inside the game ontology
                                if (CheckDatatypeProperty(GetString(currentPropertyName), false))
                                {
                                    // 3.3.2.17A.1 Get next element first word
                                    var nextElementFirstWord = currentPropertyName.Split('_').First();
                                    // 3.3.2.17A.2 Check if characterClassName contains next element first word. If it is true, then the subject is character fact. Otherwise, the subject is the subject reference.
                                    SubjectFact = !characterClassName.Contains(nextElementFirstWord) ? CharacterOntology.Data.SelectFact(GetString(SubjectRef, true)) : characterFact;
                                    // 3.3.2.17A.3 Check if currentProperty exists inside the character ontology. If it does not exist, we create it.
                                    var PredicateDatatypeProperty = CheckDatatypeProperty(GetString(currentPropertyName, true)) ? CharacterOntology.Model.PropertyModel.SelectProperty(GetString(currentPropertyName, true)) as RDFOntologyDatatypeProperty : CreateDatatypeProperty(GetString(currentPropertyName, true));
                                    // 3.3.2.17A.4 Get subject fact asssertions
                                    SubjectAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesByObject(SubjectFact);
                                    // Check if SubjectAssertions is not null, to prevent unhandled exceptions
                                    if (SubjectAssertions.EntriesCount > 0)
                                    {
                                        // 3.3.2.17A.5 Get subject fact assertions which have nextPredicateProperty as predicate
                                        SubjectPredicateAssertions = SubjectAssertions.SelectEntriesByPredicate(PredicateDatatypeProperty);
                                        // Check if SubjectPredicateAssertions is not null, to prevent unhandled exceptions
                                        if (SubjectPredicateAssertions.EntriesCount > 0)
                                        {
                                            // 3.3.2.17A.5A.1 Get entry object value
                                            var entryObjectString = SubjectPredicateAssertions.Single().TaxonomyObject.ToString();
                                            // 3.3.2.17A.5A.2 If the value is float, convert it to string. Otherwise, assign the value to CurrentValue and SubjectRef
                                            if (entryObjectString.Contains("float"))
                                            {
                                                entryObjectString = entryObjectString.Split('^').First();
                                                // If the value contains decimals, then we only get the integer part of the value.
                                                if (entryObjectString.Contains(','))
                                                    CurrentValue = entryObjectString.Split(',').ElementAtOrDefault(0);
                                                else
                                                    CurrentValue = entryObjectString;
                                                SubjectRef = CurrentValue;
                                            }
                                            else
                                            {
                                                CurrentValue = entryObjectString.Split('^').First();
                                                SubjectRef = CurrentValue;
                                            }
                                        }
                                        else
                                        {
                                            // 3.3.2.17A.5B If there is no entries, then the value is 0.
                                            CurrentValue = 0.ToString();
                                            SubjectRef = CurrentValue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }// Ended element as object property
                 // 3.4 Check if element is an class in the current ontology
                else if (CheckClass(GetString(element, applyOnCharacter), applyOnCharacter))
                {
                    // 3.4.1 Get next element
                    var nextElement = formulaElements.ElementAt(index + 1);
                    // 3.4.2 Get element as individual, adding itemName to the end of element string
                    var subjectFactName = $"{element}_{User_Input}";
                    var SubjectFact = CurrentOntology.Data.SelectFact(GetString(subjectFactName, applyOnCharacter));
                    // 3.4.3 Get next element as property
                    var PredicateProperty = CurrentOntology.Model.PropertyModel.SelectProperty(GetString(nextElement, applyOnCharacter));
                    // 3.4.4 Get assertion
                    var assertion = CurrentOntology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact).SelectEntriesByPredicate(PredicateProperty);
                    // Check if assertion is not null, to prevent unhandled exceptions
                    if (assertion.EntriesCount > 0)
                    {
                        // If the assertion is not null, then the current value is the assertion object
                        CurrentValue = assertion.Single().TaxonomyObject.ToString().Split('^').First();
                    }
                }// Ended element as class
                 // 3.5 Check if element is an individual in the current ontology
                else if (CheckIndividual(GetString(element, applyOnCharacter), applyOnCharacter))
                {
                    // 3.5.1 Get next element
                    var nextElement = formulaElements.ElementAt(index + 1);
                    // 3.5.2 Get current element as subject fact
                    var SubjectFact = CurrentOntology.Data.SelectFact(GetString(element, applyOnCharacter));
                    // 3.5.3 Get next element as a property
                    var PredicateProperty = CurrentOntology.Model.PropertyModel.SelectProperty(GetString(nextElement, applyOnCharacter));
                    // 3.5.4 Check if PredicateProperty is not null, to prevent unhandled exceptions
                    if (PredicateProperty == null)
                    {
                        /* If PredicateProperty is null, then it is necessary to find another predicate. To find it, we will use the words inside the current predicate name.*/
                        // 3.5.4A.1 Declarate some variables to find the new predicate
                        var PredicatePropertyWords = PredicateProperty.ToString().Split('#').Last().Split('_').ToList();
                        var wordCounter = PredicatePropertyWords.Count();
                        var propertyFound = false;
                        var propertyName = string.Empty;

                        // 3.5.4A.2 Search properties until we find one
                        while (propertyFound == false && wordCounter > 1)
                        {
                            propertyName = "";
                            // 3.5.4A.3 Add all words to propertyName except the last one
                            for (int i = 0; i < wordCounter - 1; ++i)
                                propertyName += PredicatePropertyWords.ElementAt(i) + "_";
                            propertyName += PredicatePropertyWords.Last();
                            // 3.5.4A.4 Check if propertyName exists as a datatype property inside the current
                            if (CheckDatatypeProperty(GetString(propertyName, applyOnCharacter), applyOnCharacter))
                            {
                                //3.5.4A.4A.1 If propertyName is a datatype property, get it and update propertyFound indicator
                                PredicateProperty = CurrentOntology.Model.PropertyModel.SelectProperty(GetString(propertyName, applyOnCharacter));
                                propertyFound = true;
                            }
                            // 3.5.4A.5 Substract one from wordCounter
                            --wordCounter;
                        }
                    }
                    // 3.5.5 Get the assertion that has SubjectFact as subject and PredicateProperty as predicate
                    var assertion = CurrentOntology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact).SelectEntriesByPredicate(PredicateProperty);
                    // Check if assertion is null, to prevent unhandled exceptions
                    if (assertion.EntriesCount > 0)
                    {
                        // 3.5.6 If assertion is not null, the current value is the assertion object
                        CurrentValue = assertion.Single().TaxonomyObject.ToString().Split('^').First();
                    }
                }// Ended element as individual
                 //3.6 Check element as operator
                else if (Operators.Any(op => element == op))
                {
                    var isFloat = false;
                    // 3.6.1 Get next element
                    var nextElement = formulaElements.ElementAt(index + 1);
                    // 3.6.2 Check if next element is a numeric value
                    var nextElementIsValue = float.TryParse(nextElement, out float nextElementValue);
                    if (nextElementIsValue == false)
                    {
                        // 3.6.2A.1 Get character fact
                        var CharacterFact = CharacterOntology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
                        var SubjectFact = CharacterFact;
                        // 3.6.2A.2 Check if nextElement is a datatypeProperty 
                        if (CheckDatatypeProperty(GetString(nextElement, true)))
                        {
                            // 3.6.2A.2A.1 Get character class name
                            var characterClassName = GetElementClass($"{this.Context}{FileService.EscapedName(this.Name)}", true).FullName.Split('#').Last();
                            // 3.6.2A.2A.2 Get nextElement first word
                            var nextElementFirstWord = nextElement.Split('_').First();
                            // 3.6.2A.2A.3 Check if character class name contains next element first word
                            if (!characterClassName.Contains(nextElementFirstWord))
                            {
                                // 3.6.2A.2A.3A.1 If character class name does not contain next element first word, find the class which name contains the first word.
                                var nextElementClass = CurrentOntology.Model.ClassModel.Where(item => item.ToString().Contains(nextElementFirstWord)).Single();
                                // 3.6.2A.2A.3A.2 Find the element whose class is nextElementClass
                                var nextElementFactName = CurrentOntology.Data.Relations.ClassType.SelectEntriesByObject(nextElementClass).Single().TaxonomySubject.ToString().Split('#').Last();
                                // 3.6.2A.2A.3A.3 Check if nextElementFact exists inside the character ontology. If it does not, we create it
                                CharacterFact = CheckIndividual(GetString(nextElementFactName, true)) ? CharacterOntology.Data.SelectFact(GetString(nextElementFactName, true)) : CreateIndividual(nextElementFactName);
                            }
                            // 3.6.2A.2A.4 Check if next element predicate exists inside the character ontology. If it does not, we create it
                            var PredicateProperty = CheckDatatypeProperty(GetString(nextElement, true)) ? CharacterOntology.Model.PropertyModel.SelectProperty(GetString(nextElement, true))
                                : CreateDatatypeProperty(GetString(nextElement, true));
                            // 3.6.2A.2A.5 Declare variable to get next element value string
                            var nextElementValueString = string.Empty;
                            // 3.6.2A.2A.6 Get character assertions
                            var characterAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                            // Check if characterAssertions is null, to prevent unhandled exceptions
                            if (characterAssertions.EntriesCount > 0)
                            {
                                // 3.6.2A.2A.7 Get characterAssertions which have PredicateProperty as predicate
                                var characterPredicateAssertions = characterAssertions.SelectEntriesByPredicate(PredicateProperty);
                                // Check if characterPredicateAssertions is null, to prevent unhandled exceptions
                                if (characterPredicateAssertions.EntriesCount == 0)
                                {
                                    // 3.6.2A.2A.7A.1 Get predicate name
                                    var predicateName = PredicateProperty.ToString().Split('#').Last();
                                    // 3.6.2A.2A.7A.2 Search predicate inside the current ontology
                                    var CurrentOntologyPredicate = CurrentOntology.Model.PropertyModel.SelectProperty(GetString(predicateName, applyOnCharacter));
                                    // 3.6.2A.2A.7A.3 Get CurrentOntologyPredicate definition annoation
                                    var propertyDefinition = CurrentOntology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(CurrentOntologyPredicate).Single().TaxonomyObject.ToString().Split('^').First();
                                    // 3.6.2A.2A.7A.4 Get CurrentOntologyPredicate type
                                    var propertyType = CurrentOntology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(CurrentOntologyPredicate).Single().TaxonomyObject.ToString().Split('^').Last();
                                    // 3.6.2A.2A.7A.5 Get value from CurrentOntologyPredicate
                                    nextElementValueString = $"{GetValue(propertyDefinition)}^^{propertyType}";
                                }
                                else
                                    // 3.6.2A.2A.7B.1 If exists
                                    nextElementValueString = characterPredicateAssertions.Single().TaxonomyObject.ToString();
                            }
                            // 3.6.2A.2A.8 Get next element value. If it is float, round it to integer
                            string nextValueDigits = nextElementValueString.Substring(0, nextElementValueString.IndexOf('^'));
                            if (!nextElementValueString.Contains("float"))
                            {
                                if (nextValueDigits.Contains(','))
                                    nextValueDigits = nextElementValueString.Split(',').ElementAtOrDefault(0);

                                nextElementValue = Convert.ToSingle(nextValueDigits);
                            }
                            else
                            {
                                isFloat = true;
                                nextElementValueString = nextElementValueString.Substring(0, nextElementValueString.IndexOf('^'));
                                nextElementValue = Convert.ToSingle(nextElementValueString, CultureInfo.InvariantCulture);
                            }
                        }
                        // 3.6.2A.3 Check if nextElement is an objectProperty
                        else if (CheckObjectProperty(GetString(nextElement, true)))
                        {
                            // 3.6.2A.4 It it is true, the first thing we will do is save the index in other variable
                            var currentIndex = index;
                            // 3.6.2A.5 Get next element property 
                            var nextElementProperty = CharacterOntology.Model.PropertyModel.SelectProperty(GetString(nextElement,true));
                            // 3.6.2A.6 Get character assertions
                            var characterAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                            // Check if characterAssertions is null, to prevent unhandled exceptions
                            if (characterAssertions.EntriesCount > 0)
                            {
                                // 3.6.2A.7 Get character assertions which have nextElementProperty as predicate
                                var characterPredicateAssertions = characterAssertions.SelectEntriesByPredicate(nextElementProperty);
                                // Check if characterPredicateAssertions is null, to prevent unhandled exceptions
                                if (characterPredicateAssertions.EntriesCount > 0)
                                {
                                    // 3.6.2A.8 Get assertion object fact
                                    var nextElementFact = characterPredicateAssertions.Single().TaxonomyObject as RDFOntologyFact;
                                    // 3.6.2A.9 Increase current index variable
                                    ++currentIndex;
                                    // 3.6.2A.10 Get next element
                                    nextElement = formulaElements.ElementAt(currentIndex + 1).Replace("Item", itemName).Replace("Ref", SubjectRef);
                                    // 3.6.2A.11 Check if next element exists as property inside the current ontology
                                    if (CheckDatatypeProperty(GetString(nextElement, applyOnCharacter), applyOnCharacter))
                                    {
                                        // 3.6.2A.11A.1 If next element is a datatype property, check if the property exists inside the character ontology. If it does not, we create it.
                                        var PredicateProperty = CheckDatatypeProperty(GetString(nextElement, true)) ? CharacterOntology.Model.PropertyModel.SelectProperty(GetString(nextElement, true)) : CreateDatatypeProperty(GetString(nextElement, true));
                                        //3.6.2A.11A.2 Get nextElementFact assertions
                                        var nextElementFactAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(nextElementFact);
                                        // Check if nextElementFactAssertions is null, to prevent unhandled exceptions
                                        if (nextElementFactAssertions.EntriesCount > 0)
                                        {
                                            //3.6.2A.11A.3 Get nextElementFact assertions which have PredicateProperty as predicate
                                            var nextElementFactPredicateAssertions = nextElementFactAssertions.SelectEntriesByPredicate(PredicateProperty);
                                            // Check if nextElementFactPredicateAssertions is null, to prevent unhandled exceptions
                                            if (nextElementFactPredicateAssertions.EntriesCount > 0)
                                            {
                                                //3.6.2A.11A.3 Get next value. If it is not float, then round the value to integer
                                                var nextElementValueString = nextElementFactPredicateAssertions.Single().TaxonomyObject.ToString();
                                                var nextElementValueDigits = nextElementValueString.Split('^').First();

                                                if (!nextElementValueString.Contains("float"))
                                                {
                                                    if (nextElementValueDigits.Contains(','))
                                                        nextElementValueDigits = nextElementValueString.Split(',').ElementAtOrDefault(0);

                                                    nextElementValue = Convert.ToSingle(nextElementValueDigits);
                                                }
                                                else
                                                {
                                                    isFloat = true;
                                                    nextElementValue = Convert.ToSingle(nextElementValueDigits, CultureInfo.InvariantCulture);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // 3.6.2A.11B.1 If it is not a datatype property, we will need to find a property using next element words
                                        var nextElementWords = nextElement.Split('_').ToList();
                                        // 3.6.2A.11B.2 Declare variable to count number of words in use currently
                                        var wordCounter = nextElementWords.Count() - 1;
                                        // 3.6.2A.11B.3 Mount new property name using next element words while we do not find a property
                                        while (!CheckDatatypeProperty(GetString(nextElement)) && wordCounter > 1)
                                        {
                                            nextElement = "";
                                            for (int i = 0; i < wordCounter - 1; ++i)
                                                nextElement += nextElementWords.ElementAtOrDefault(i) + "_";
                                            nextElement += nextElementWords.ElementAtOrDefault(wordCounter);
                                            --wordCounter;
                                        }
                                        // 3.6.2A.11B.4 Check if wordCounter has reached its limit
                                        if (wordCounter > 1)
                                        {
                                            // 3.6.2A.11B.4A.1 Check if new nextElement is a datatype property inside the characterOntology. If it does not exists, we create it
                                            var PredicateProperty = CheckDatatypeProperty(GetString(nextElement, true)) ? CharacterOntology.Model.PropertyModel.SelectProperty(GetString(nextElement, true)) : CreateDatatypeProperty(GetString(nextElement, true));
                                            // 3.6.2A.11B.4A.2 Get next element fact assertions
                                            var nextElementFactAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(nextElementFact);
                                            // Check if nextElementFactAssertions is null, to prevent unhandled exceptions
                                            if (nextElementFactAssertions.EntriesCount > 0)
                                            {
                                                // 3.6.2A.11B.4A.3 Get nextElementFact assertions which have PredicateProperty as predicate
                                                var nextElementFactPredicateAssertions = nextElementFactAssertions.SelectEntriesByPredicate(PredicateProperty);
                                                // Check if nextElementFactPredicateAssertions is null, to prevent unhandled exceptions
                                                if (nextElementFactPredicateAssertions.EntriesCount > 0)
                                                {
                                                    // 3.6.2A.11B.4A.14 Get next value. If it is not float, then round the value to integer
                                                    var nextElementValueString = nextElementFactPredicateAssertions.Single().TaxonomyObject.ToString();
                                                    var nextElementValueDigits = nextElementValueString.Split('^').First();

                                                    if (!nextElementValueString.Contains("float"))
                                                    {
                                                        if (nextElementValueDigits.Contains(','))
                                                            nextElementValueDigits = nextElementValueString.Split(',').ElementAtOrDefault(0);

                                                        nextElementValue = Convert.ToSingle(nextElementValueDigits);
                                                    }
                                                    else
                                                    {
                                                        isFloat = true;
                                                        nextElementValue = Convert.ToSingle(nextElementValueDigits, CultureInfo.InvariantCulture);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            /* If there is no property, then we need to find it searching upper in the property hierarchy.*/
                                            // 3.6.2A.11B.4B.1 Get item parents
                                            var itemParents = GetParentClasses(GetString(itemName, applyOnCharacter), applyOnCharacter);
                                            // Check if item parents is null, to prevent unhandled exceptions
                                            if (itemParents != null)
                                            {
                                                // 3.6.2A.11B.4B.2 Declare some variables to get a new item with its definition
                                                var newItem = string.Empty;
                                                var newItemDefinition = string.Empty;
                                                var parentList = itemParents.Split("|").ToList();
                                                // 3.6.2A.11B.4B.3 Look for each parent of item
                                                foreach (var parent in parentList)
                                                {
                                                    // 3.6.2A.11B.4B.4 Check if new item is not defined
                                                    if (string.IsNullOrEmpty(newItem))
                                                    {
                                                        // 3.6.2A.11B.4B.5 Search while there are not items found
                                                        for (int i = index; i < formulaElements.Count() - 1; ++i)
                                                        {
                                                            // 3.6.2A.11B.4B.6 Get next element and replace item with parent
                                                            var newNextElement = formulaElements.ElementAt(i + 1).Replace(itemName, parent);
                                                            // 3.6.2A.11B.4B.7 Check if newNextElement is a datatype property inside the current ontology
                                                            if (CheckDatatypeProperty(GetString(newNextElement, applyOnCharacter), applyOnCharacter))
                                                            {
                                                                // 3.6.2A.11B.4B.8 Create a new list using the current definition 
                                                                var newValueList = valueDefinition.Split(':').ToList();
                                                                // 3.6.2A.11B.4B.9 declare some variables to get the new item definition
                                                                var basePointsWord = string.Empty;
                                                                var itemClassItem = GetElementClass(GetString(itemName, applyOnCharacter), applyOnCharacter);
                                                                var definitionFound = false;
                                                                var itemClassDefinition = string.Empty;
                                                                // 3.6.2A.11B.4B.10 Search while no definition has been found 
                                                                while (!definitionFound)
                                                                {
                                                                    // 3.6.2A.11B.4B.10.1 Search for valued list info annotations for item class
                                                                    var itemClass = CurrentOntology.Model.ClassModel.SelectClass(GetString(itemClassItem.FullName.Split('#').Last(), applyOnCharacter));
                                                                    var itemClassAnnotations = CurrentOntology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(itemClass);
                                                                    // Check if definitionAnnotations is null, to prevent unhandled exceptions
                                                                    if (itemClassAnnotations.EntriesCount > 0)
                                                                    {
                                                                        // 3.6.2A.11B.4B.10.2 Get itemClassAnnotations which have "Valued List Info" as predicate
                                                                        var itemClassDefinitionAnnotations = itemClassAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("ValuedListInfo"));
                                                                        // Check if itemClassDefinitionAnnotations is null, to prevent unhandled exceptions
                                                                        if (itemClassDefinitionAnnotations.Count() > 0)
                                                                        {
                                                                            // 3.6.2A.11B.4B.10.3 Get definition and update definitionFound indicator
                                                                            itemClassDefinition = itemClassDefinitionAnnotations.Single().TaxonomyObject.ToString().Split('^').First();
                                                                            definitionFound = true;
                                                                        }
                                                                    }
                                                                }
                                                                // 3.6.2A.11B.4B.11 Get new list of definition rows
                                                                var definitionRows = itemClassDefinition.Split('\n').ToList();
                                                                // 3.6.2A.11B.4B.12 Search for base points word in every row of the new definition
                                                                foreach (var row in definitionRows)
                                                                {
                                                                    // 3.6.2A.11B.4B.13 Get row elements
                                                                    var rowElements = row.Split(',').ToList();
                                                                    // 3.6.2A.11B.4B.14 Check if current row is defined as user editable
                                                                    var userEditValue = Convert.ToBoolean(rowElements.Where(item => item.Contains("User_Edit")).Single().Split(':').Last());
                                                                    if (userEditValue == true)
                                                                    {
                                                                        basePointsWord = rowElements.First().Split('_').Last();
                                                                        break;
                                                                    }
                                                                }
                                                                // 3.6.2A.11B.4B.15 Count newValueList elements
                                                                var listCount = newValueList.Count();
                                                                // 3.6.2A.11B.4B.16 Add user input in the position of base points word
                                                                for (int listIndex = 0; listIndex < listCount; ++listIndex)
                                                                {
                                                                    if (newValueList.ElementAtOrDefault(listIndex).Contains(basePointsWord))
                                                                    {
                                                                        newValueList.RemoveAt(listIndex);
                                                                        newValueList.Insert(listIndex, User_Input);
                                                                    }
                                                                }
                                                                // 3.6.2A.11B.4B.17 Create new value definition using the elements of newValueList
                                                                foreach (string item in newValueList)
                                                                {
                                                                    int listIndex = newValueList.IndexOf(item);
                                                                    if (listIndex == i + 1)
                                                                        newItemDefinition += newNextElement + ':';
                                                                    else
                                                                        newItemDefinition += item + ':';
                                                                }
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    else
                                                        break;
                                                }
                                                if (newItemDefinition.EndsWith(':'))
                                                    newItemDefinition = newItemDefinition[0..^1];

                                                nextElementValue = GetValue(newItemDefinition.ToString(), itemName, User_Input);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var itemString = GetString(itemName);
                            var itemClass = GetElementClass(itemString);
                            var itemClassName = itemClass.ToString().Split('#').Last();
                            var definition = valueDefinition.Replace(itemName, itemClassName);
                            nextElementValue = GetValue(definition);                
                        }
                    }

                    // 3.6.3 If current element matches with the division operator, and denominator is 0, then next value is the neutral value of division (1).
                    if (element == "/" && nextElementValue == 0)
                    {
                        nextElementValue = 1;
                    }
                    // 3.6.4 Get result of the operator
                    dynamic operatorResult = ConvertToOperator(element, Convert.ToSingle(CurrentValue), nextElementValue);
                    // 3.6.5 Check if result is boolean
                    if (operatorResult.GetType().ToString().Contains("boolean"))
                    {
                        // 3.6.5A.1 If result is boolean, add all individuals in itemName class to currentList
                        foreach (Item individual in GetIndividuals(itemName))
                            if (operatorResult == true)
                                currentList.Add(individual);
                    }
                    else
                    {
                        //3.6.5B.1 If result is not value, check if value is float. If it is not, then round result to integer
                        if (isFloat == false)
                        {
                            var resultString = operatorResult.ToString();
                            if (resultString.Contains(','))
                                CurrentValue = resultString.Split(',').ElementAt(0);
                            else
                                CurrentValue = resultString;
                        }
                        else
                            CurrentValue = operatorResult.ToString();
                    }
                }
                else
                {
                    var itemString = GetString(itemName);
                    var itemClass = GetElementClass(itemString);
                    var itemClassName = itemClass.FullName.Split('#').Last();
                    var definition = valueDefinition.Replace("Item", itemClassName);
                    CurrentValue = GetValue(definition).ToString();
                }
            }
            
            // Check if currentValue is null, remove type and return value
            if (string.IsNullOrEmpty(CurrentValue))
                CurrentValue = "0";
            CurrentValue = CurrentValue.Split('^').First();
            return Convert.ToSingle(CurrentValue);
        }
    }
}
