using ARPEGOS.Helpers;
using RDFSharp.Model;
using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace ARPEGOS.Services
{
    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Returns the hierarchy of the property given
        /// </summary>
        /// <param name="propertyName"> Name of the property given </param>
        /// <returns> String with the hierarchy of the property given </returns>
        public string GetPropertyVisualizationPosition (string propertyName)
        {
            var CurrentProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{propertyName}");
            var AnnotationProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}Visualization");
            var CharacterVisualizationAnnotations = this.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesByPredicate(AnnotationProperty);
            var CharacterVisualizationAnnotation = CharacterVisualizationAnnotations.SelectEntriesBySubject(CurrentProperty).Single();
            return CharacterVisualizationAnnotation.TaxonomyObject.ToString().Split('^').First();
        }

        /// <summary>
        /// Returns all the properties of the current character 
        /// </summary>
        /// <returns> Dictionary with the properties of the current character and its values</returns>
        public Dictionary<string, string> GetCharacterProperties ()
        {
            var CharacterProperties = new Dictionary<string, string>();
            var characterName = FileService.EscapedName(this.Name);
            var CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{characterName}");
            var CharacterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
            foreach (var assertion in CharacterAssertions)
            {
                var property = assertion.TaxonomyPredicate.ToString().Split('#').Last();
                var value = assertion.TaxonomyObject.ToString().Split('#').Last();
                if (!CharacterProperties.ContainsKey(property))
                    CharacterProperties.Add(property, value);
            }

            return CharacterProperties;
        }

        /// <summary>
        /// Returns the properties that represent the skills of the character
        /// </summary>
        /// <returns> Set of string which contains the names of the skills </returns>
        public IEnumerable<Item> GetCharacterSkills ()
        {
            var characterProperties = GetCharacterProperties();
            var skills = new ObservableCollection<Item>();
            var AnnotationType = "SkillProperty";
            foreach (var item in characterProperties)
            {
                var skillName = string.Empty;
                var skillDescription = string.Empty;
                var property = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{item.Key}");
                var annotationProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{AnnotationType}");
                var propertyTaxonomy = this.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesByPredicate(annotationProperty);
                var propertySkillTaxonomy = propertyTaxonomy.SelectEntriesBySubject(property);
                if (propertySkillTaxonomy.EntriesCount > 0)
                {
                    if (this.CheckObjectProperty(item.Key) == true)
                    {
                        skillName = item.Value;
                        var skillObjectFact = this.Ontology.Data.SelectFact($"{this.Context}{skillName}");
                        skillDescription = this.Ontology.Data.Annotations.Comment.SelectEntriesBySubject(skillObjectFact).Single().TaxonomyObject.ToString().Split('^').First();
                        skills.Add(new Item(skillName, skillDescription));
                    }
                    else
                    {
                        skillName = item.Key.Replace("Per_","").Replace("_Total","");
                        var skillObjectFact = this.Ontology.Data.SelectFact($"{this.Context}{skillName}");
                        
                        skillDescription = this.Ontology.Data.Annotations.Comment.SelectEntriesBySubject(skillObjectFact).Single().TaxonomyObject.ToString().Split('^').First();
                        skills.Add(new Item(skillName, skillDescription));
                    }
                }
            }
            return skills;
        }


        public int GetSkillValue (string skillName)
        {
            var skillPropertyName = $"Per_{skillName}_Total";
            if (this.CheckDatatypeProperty(skillPropertyName) == true)
                skillName = skillPropertyName;

            int skillValue = 0;
            var character = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
            var currentProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{skillName}");
            if (this.CheckIndividual(skillName) == true)
            {
                var annotationPropertyName = "SkillValue";
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
                }
            }
            else if (this.CheckDatatypeProperty(skillName) == true)
            {
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
        public Item GetElementClass (string elementName, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var DataModel = CurrentOntology.Data;
            var elementClassType = DataModel.Relations.ClassType.Where(entry => entry.TaxonomySubject.ToString().Contains(elementName)).First();
            var elementClassName = elementClassType.TaxonomyObject.ToString().Split('#').Last();
            var elementClassResource = this.Ontology.Model.ClassModel.SelectClass($"{this.Context}{elementClassName}");
            var elementClassDescription = this.Ontology.Model.ClassModel.Annotations.Comment.SelectEntriesBySubject(elementClassResource).Single().TaxonomyObject.ToString().Split('^').First();
            var elementClass = new Item(elementClassName, elementClassDescription);
            return elementClass;
        }

        /// <summary>
        /// Returns description of the element
        /// </summary>
        /// <param name="elementName">Name of the element</param>
        /// <returns></returns>
        public string GetElementDescription (string elementName, bool applyOnCharacter = false)
        {
            var elementDesc = string.Empty;
            RDFOntology CurrentOntology;
            var CurrentContext = string.Empty;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var DataModel = CurrentOntology.Data;
            var elementCommentAnnotation = DataModel.Annotations.Comment.Where(entry => entry.TaxonomySubject.ToString().Contains(elementName)).Single();
            if (elementCommentAnnotation != null)
                elementDesc = elementCommentAnnotation.TaxonomyObject.ToString().Split('^').First();
            return elementDesc;
        }

        /// <summary>
        /// Returns a list of individuals given the name of their class
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public IEnumerable<Item> GetIndividuals (string className, bool applyOnCharacter = false)
        {
            var individuals = new ObservableCollection<Item>();
            RDFOntology CurrentOntology;
            string CurrentContext;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var ClassModel = CurrentOntology.Model.ClassModel;
            var DataModel = CurrentOntology.Data;
            var currentClass = ClassModel.SelectClass($"{CurrentContext}{className}");
            var classAssertions = DataModel.Relations.ClassType.SelectEntriesByObject(currentClass);
            foreach (var assertion in classAssertions)
            {
                var individual = assertion.TaxonomySubject;
                var individualName = individual.ToString().Substring(individual.ToString().LastIndexOf('#') + 1);
                var individualDescription = CurrentOntology.Data.Annotations.Comment.SelectEntriesBySubject(individual).Single().TaxonomyObject.ToString().Split('^').First();
                individuals.Add(new Item(individualName, individualDescription));
            }

            return individuals;
        }

        /// <summary>
        /// Returns a list of groups of individuals given the name of the root class
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public dynamic GetIndividualsGrouped (string className, bool applyOnCharacter = false)
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

        /// <summary>
        /// Returns true if the given element has available points, and the available points left.
        /// </summary>
        /// <param name="ElementName">Name of the element</param>
        /// <param name="AvailablePoints">Available points obtained</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetAvailablePoints (string ElementName, out float? AvailablePoints, bool applyOnCharacter = false)
        {
            AvailablePoints = null;
            var LimitPropertyName = string.Empty;
            var AvailableWords = new List<string>()
            {
                "Disponible",
                "Available"
            };

            RDFOntology CurrentOntology;
            string CurrentContext;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var PropertyModel = CurrentOntology.Model.PropertyModel;
            var FilterResultsCounter = 0;
            var index = 0;
            var ElementWords = ElementName.Split('_').ToList();
            var CompareList = new List<string>();
            var ResultProperties = new List<RDFOntologyProperty>();

            var ElementClass = CurrentOntology.Model.ClassModel.SelectClass($"{CurrentContext}{ElementName}");
            var ElementClassAnnotations = CurrentOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(ElementClass);
            var ElementGeneralCostAnnotation = ElementClassAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("General_Cost_Defined_By")).Single();
            var generalCostAnnotationFound = (ElementGeneralCostAnnotation != null);

            if (generalCostAnnotationFound)
            {
                var AnnotationValue = ElementGeneralCostAnnotation.TaxonomyObject.ToString();
                if (AnnotationValue.Contains('^'))
                    AnnotationValue = AnnotationValue.Split('^').First();
                if (string.IsNullOrEmpty(AnnotationValue))
                    return null;

                var GeneralCostProperty = PropertyModel.SelectProperty($"{CurrentContext}{AnnotationValue}");
                ResultProperties.Add(GeneralCostProperty);
                FilterResultsCounter = ResultProperties.Count();
            }
            else
            {
                index = 0;
                ResultProperties = PropertyModel.Where(entry => AvailableWords.Any(word => entry.ToString().Contains(word))).ToList();
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
                        var propertyName = property.ToString().Split('#').Last();
                        var propertyWords = propertyName.Split('_').ToList();
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
                    LimitPropertyName = LimitProperty.ToString().Split('#').Last();
                    RDFOntologyDatatypeProperty CharacterLimitProperty;
                    if (CheckDatatypeProperty(LimitPropertyName) == false)
                        CharacterLimitProperty = CreateDatatypeProperty(LimitPropertyName);
                    else
                        CharacterLimitProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{LimitPropertyName}") as RDFOntologyDatatypeProperty;
                    var LimitPropertyAssertion = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(CharacterLimitProperty).Single();
                    if (LimitPropertyAssertion == null)
                    {
                        IEnumerable<RDFOntologyTaxonomyEntry> ResultAnnotations = CurrentOntology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(LimitProperty);
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

            if ((FilterResultsCounter == 0) || LimitPropertyName == null)
            {
                var parents = GetParentClasses(ElementName);
                if (parents != null)
                {
                    var parentsList = parents.Split(':').ToList();
                    foreach (string parent in parentsList)
                    {
                        if (AvailablePoints == null)
                        {
                            var parentHasAvailablePoints = GetAvailablePoints(parent, out float? parentAvailablePoints);
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
        /// Returns true if the given element has a limit and the limit.
        /// </summary>
        /// <param name="ElementName">Name of the element</param>
        /// <param name="LimitValue">Limit value obtained</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetLimit (string ElementName, out float? LimitValue, bool applyOnCharacter = false)
        {
            var LimitPropertyName = string.Empty;
            var hasLimit = false;
            LimitValue = null;
            var LimitWords = new List<string>()
            {
                "Límite",
                "Limit",
                "Limite"
            };

            RDFOntology CurrentOntology;
            string CurrentContext;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var PropertyModel = CurrentOntology.Model.PropertyModel;
            var ResultProperties = PropertyModel.Where(entry => LimitWords.Any(word => entry.ToString().Contains(word)));
            var ElementWords = ElementName.Split('_').ToList();
            var CompareList = new List<string>();
            var index = 0;
            var FilterResultsCounter = ResultProperties.Count();

            while (FilterResultsCounter > 1)
            {
                CompareList.Add(ElementWords.ElementAt(index));
                ResultProperties = ResultProperties.Where(entry => CompareList.All(word => entry.ToString().Contains(word)));
                FilterResultsCounter = ResultProperties.Count();
                if (ElementWords.Count() - 1 == index && ResultProperties.Count() > 1)
                    break;
                ++index;
            }

            if (FilterResultsCounter > 0)
            {
                hasLimit = true;
                RDFOntologyDatatypeProperty LimitProperty;
                if (FilterResultsCounter > 1)
                    LimitProperty = ResultProperties.First() as RDFOntologyDatatypeProperty;
                else
                    LimitProperty = ResultProperties.Single() as RDFOntologyDatatypeProperty;
                LimitPropertyName = LimitProperty.ToString().Split('#').Last();
                RDFOntologyDatatypeProperty CharacterLimitProperty;
                if (!CheckDatatypeProperty(LimitPropertyName))
                    CharacterLimitProperty = CreateDatatypeProperty(LimitPropertyName);
                else
                    CharacterLimitProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{LimitPropertyName}") as RDFOntologyDatatypeProperty;
                var CharacterPropertyAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(CharacterLimitProperty);
                if (CharacterPropertyAssertions.Count() == 0)
                {
                    IEnumerable<RDFOntologyTaxonomyEntry> ResultAnnotations = CurrentOntology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(LimitProperty);
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
                            LimitValue = GetValue(LimitPropertyDefinition);
                        else
                            LimitValue = Convert.ToSingle(LimitPropertyDefinition);
                    }
                }
                else
                {
                    var entry = CharacterPropertyAssertions.Single();
                    var value = entry.TaxonomyObject.ToString().Split('^').First();
                    LimitValue = Convert.ToSingle(value);
                }
            }
            else
            {
                var parents = GetParentClasses(ElementName);
                if (parents != null)
                {
                    var parentList = parents.Split(':').ToList();
                    foreach (var parent in parentList)
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
        public string GetLimitByValue (string stage, string value)
        {
            var Limit = string.Empty;
            var CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
            var CharacterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
            var CharacterAssertionsValueEntries = CharacterAssertions.Where(entry => entry.TaxonomyObject.ToString().Contains(value));
            if (CharacterAssertionsValueEntries.Count() == 1)
                Limit = CharacterAssertionsValueEntries.Single().TaxonomyPredicate.ToString().Split('#').Last();
            else
            {
                if (CharacterAssertionsValueEntries.Count() > 1)
                {
                    var StageGeneralLimit = GetAvailablePoints(stage, out float? LimitValue);
                    if (LimitValue.ToString() != value)
                    {
                        var StagePartialLimit = GetLimit(stage, out LimitValue);
                        if (LimitValue.ToString() != value)
                        {
                            var parents = GetParentClasses(stage);
                            if (parents != null)
                            {
                                var parentList = parents.Split(':').ToList();
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
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetObjectPropertyAssociated (string stage, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var propertyName = string.Empty;
            var propertyNameFound = false;
            var StageWords = stage.Split('_').ToList();
            var wordCounter = StageWords.Count();
            while (propertyNameFound == false && wordCounter > 0)
            {
                var ObjectPropertyName = "tiene";
                for (int i = 0; i < wordCounter; ++i)
                    ObjectPropertyName += StageWords.ElementAt(i);

                var ObjectPropertyAssertions = CurrentOntology.Model.PropertyModel.Where(entry => entry.Range != null && entry.Range.ToString().Contains(stage));
                if (ObjectPropertyAssertions.Count() > 1)
                {
                    ObjectPropertyAssertions = ObjectPropertyAssertions.Where(entry => entry.ToString().Contains(ObjectPropertyName));
                    propertyName = ObjectPropertyAssertions.Single().ToString().Split('#').Last();
                    propertyNameFound = true;
                }
                else if (ObjectPropertyAssertions.Count() == 1)
                {
                    propertyName = ObjectPropertyAssertions.Single().ToString().Split('#').Last();
                    propertyNameFound = true;
                }
                --wordCounter;
            }
            if (wordCounter == 0)
            {
                var parents = GetParentClasses(stage);
                if (parents != null)
                {
                    var parentList = parents.Split(':').ToList();
                    foreach (var parent in parentList)
                    {
                        if (propertyNameFound == false)
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
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public SortedList<int, string> GetOrderedSubstages (string stage, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }
            var Substages = GetSubClasses(stage);
            var SubstagesAndOrder = new Dictionary<int, string>();
            foreach (var substage in Substages)
            {
                var SubstageClass = CurrentOntology.Model.ClassModel.SelectClass($"{CurrentContext}{substage.Title}");
                var Substage_OrderEntry = CurrentOntology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(SubstageClass).Where(entry => entry.TaxonomyPredicate.ToString().Contains("Substage_Order")).Single();
                if (Substage_OrderEntry != null)
                    SubstagesAndOrder.Add(Convert.ToInt32(Substage_OrderEntry.TaxonomyObject.ToString().Split('^').First()), substage.Title);
            }

            return new SortedList<int, string>(SubstagesAndOrder);
        }

        /// <summary>
        /// Returns a string containing the names of the types of the given element
        /// </summary>
        /// <param name="elementName">Name of the element</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetParentClasses (string elementName, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var parent = string.Empty;
            var elementClass = CurrentOntology.Model.ClassModel.SelectClass($"{CurrentContext}{elementName}");
            if (elementClass != null)
            {
                var elementClassEntries = CurrentOntology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(elementClass);
                foreach (var entry in elementClassEntries)
                    parent += $"{entry.TaxonomyObject.ToString().Split('#').Last()}{":"}";
                if (parent != null)
                    if (parent.EndsWith(':'))
                        parent = parent[0..^1];
            }
            else
            {
                var elementFact = CurrentOntology.Data.SelectFact($"{CurrentContext}{elementName}");
                var ElementClassAssertions = CurrentOntology.Data.Relations.ClassType.SelectEntriesBySubject(elementFact);
                foreach (var entry in ElementClassAssertions)
                    parent += $"{entry.TaxonomyObject.ToString().Split('#').Last()}{":"}";
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
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetGeneralCost (string stage, string ElementName, out float generalCost, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var GeneralCostName = string.Empty;
            generalCost = 0;
            var CostWords = new List<string> { "Coste", "Cost", "Coût" };
            var ItemFact = CurrentOntology.Data.SelectFact($"{CurrentContext}{ElementName}");
            var ItemFactAssertions = CurrentOntology.Data.Relations.Assertions.SelectEntriesBySubject(ItemFact);
            var ItemCosts = ItemFactAssertions.Where(entry => CostWords.Any(word => entry.ToString().Contains(word)));
            var GeneralCostPredicateName = CheckGeneralCost(stage);
            ItemCosts = ItemCosts.Where(entry => entry.TaxonomyPredicate.ToString().Contains(GeneralCostPredicateName));
            if (ItemCosts.Count() > 0)
            {
                var ItemCostEntry = ItemCosts.Single();
                generalCost = Convert.ToSingle(ItemCostEntry.TaxonomyObject.ToString().Split('^').First());
                GeneralCostName = ItemCostEntry.TaxonomyPredicate.ToString().Split('#').Last();
            }

            return GeneralCostName;
        }

        /// <summary>
        /// Returns the partial cost of an element given its name and the current stage
        /// </summary>
        /// <param name="stage">Name of the current stage</param>
        /// <param name="ElementName">Name of the element</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetPartialCost (string stage, string ElementName, out float partialCost, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            partialCost = 0;
            var CostWords = new List<string> { "Coste", "Cost", "Coût" };
            var ItemFact = CurrentOntology.Data.SelectFact($"{CurrentContext}{ElementName}");
            var ItemFactAssertions = CurrentOntology.Data.Relations.Assertions.SelectEntriesBySubject(ItemFact);
            var ItemCosts = ItemFactAssertions.Where(entry => CostWords.Any(word => entry.ToString().Contains(word)));
            if (ItemCosts.Count() > 1)
            {
                var GeneralCostPredicateName = CheckGeneralCost(stage);
                ItemCosts = ItemCosts.Where(entry => !entry.TaxonomyPredicate.ToString().Contains(GeneralCostPredicateName));
            }

            var ItemCostEntry = ItemCosts.Single();
            if (ItemCostEntry != null)
                if (ItemCostEntry != null)
                    partialCost = Convert.ToSingle(ItemCostEntry.TaxonomyObject.ToString().Split('^').First());

            return ItemCostEntry.TaxonomyPredicate.ToString().Split('#').Last();
        }

        /// <summary>
        /// Returns a collection of groups if given class has subclasses.
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="applyOnCharacter">Check in character file</param>
        /// <returns></returns>
        public ObservableCollection<Group> GetSubClasses (string className, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            ObservableCollection<Group> subclasses = new ObservableCollection<Group>();
            var CharacterClassModel = CurrentOntology.Model.ClassModel;
            var RootClass = CharacterClassModel.SelectClass($"{CurrentContext}{className}");
            var SubClassesOfRoot = CharacterClassModel.Relations.SubClassOf.SelectEntriesByObject(RootClass);
            if (SubClassesOfRoot.EntriesCount != 0)
                foreach (var currentEntry in SubClassesOfRoot)
                {
                    var groupName = currentEntry.TaxonomySubject.ToString().Split('#').Last();
                    var currentClass = CharacterClassModel.SelectClass($"{CurrentContext}{groupName}");
                    var UpperClassesOfCurrent = CharacterClassModel.Relations.SubClassOf.SelectEntriesBySubject(currentClass);

                    foreach (var currentUpperClassEntry in UpperClassesOfCurrent)
                    {
                        var upperClass = currentUpperClassEntry.TaxonomyObject.ToString().Split('#').Last();
                        if (CharacterClassModel.SelectClass($"{CurrentContext}{upperClass}") == RootClass)
                            subclasses.Add(new Group(groupName));
                    }
                }
            if (subclasses.Count < 1)
                return null;
            else
                return subclasses;
        }

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
        public float GetValue (string valueDefinition, string itemName = null, string User_Input = null, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var SubjectRef = string.Empty;
            var CurrentValue = string.Empty;
            var operators = new string[] { "+", "-", "*", "/", "%", "<", ">", "<=", ">=", "=", "!=" }.ToList();
            List<dynamic> currentList = null;
            var hasUpperLimit = false;
            var UpperLimit = 0.0f;
            valueDefinition = valueDefinition.Replace("Item", itemName).Replace("__", "_");
            var expression = valueDefinition.Split(':').Select(innerItem => innerItem.Trim()).ToList();

            for (var index = 0; index < expression.Count(); ++index)
            {
                var element = expression.ElementAt(index);
                if (element.Contains("Ref"))
                    element = element.Replace("Ref", SubjectRef);

                if (element.EndsWith("()"))
                {
                    var method = element.Split("(").First();
                    if (method.Contains("Math."))
                    {
                        var methodFunction = method.Split('.').Last();
                        var mi = typeof(Math).GetMethod(methodFunction, new[] { typeof(float) });
                        var methodParameters = mi.GetParameters().ToList();
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
                        var mi = typeof(Math).GetMethod(method, new[] { typeof(float) });
                        if (mi != null)
                        {
                            var methodParameters = mi.GetParameters().ToList();
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
                    var NextElement = expression.ElementAt(index + 1);
                    var isValue = float.TryParse(NextElement, out float nextValue);
                    var isFloat = false;
                    if (isValue == false)
                    {
                        var CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
                        if (CheckObjectProperty(NextElement))
                        {
                            var row_index = index;
                            var nextElementProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{NextElement}") as RDFOntologyObjectProperty;
                            var CharacterFactAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                            var nextElementEntry = CharacterFactAssertions.Where(item => item.TaxonomyPredicate == nextElementProperty).Single();
                            var nextElementFact = nextElementEntry.TaxonomyObject as RDFOntologyFact;

                            ++row_index;
                            NextElement = expression.ElementAt(row_index + 1).Replace("Item", itemName);
                            if (CheckDatatypeProperty(NextElement, false))
                            {
                                RDFOntologyDatatypeProperty nextElementDatatypeProperty;
                                if (!CheckDatatypeProperty(NextElement))
                                    nextElementDatatypeProperty = CreateDatatypeProperty(NextElement);
                                else
                                    nextElementDatatypeProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context }{NextElement}") as RDFOntologyDatatypeProperty;
                                var ObjectFactAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(nextElementFact);
                                nextElementEntry = ObjectFactAssertions.Where(item => item.TaxonomyPredicate == nextElementDatatypeProperty).Single();
                                var nextValueString = nextElementEntry.TaxonomyObject.ToString();
                                if (!nextValueString.Contains("float"))
                                {
                                    nextValueString = nextValueString.Split('^').First().Split(',').First();
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
                                var NextElementWords = NextElement.Split('_').ToList();
                                var wordCounter = NextElementWords.Count() - 1;
                                //Poner bucle descomponer en palabras y comprobar por conjuntos reducidos de palabras
                                while (!CheckDatatypeProperty(NextElement) && wordCounter > 0)
                                {
                                    NextElement = "";
                                    for (var i = 0; i < wordCounter - 1; ++i)
                                        NextElement += $"{NextElementWords.ElementAt(i)}{"_"}";
                                    NextElement += $"{NextElementWords.ElementAt(wordCounter)}";
                                    --wordCounter;
                                }
                                if (wordCounter > 1)
                                {
                                    RDFOntologyDatatypeProperty nextElementDatatypeProperty;
                                    if (!CheckDatatypeProperty(NextElement))
                                        nextElementDatatypeProperty = CreateDatatypeProperty(NextElement);
                                    else
                                        nextElementDatatypeProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{NextElement}") as RDFOntologyDatatypeProperty;

                                    var ObjectFactAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(nextElementFact);
                                    nextElementEntry = ObjectFactAssertions.Where(item => item.TaxonomyPredicate == nextElementDatatypeProperty).Single();
                                    var nextValueString = nextElementEntry.TaxonomyObject.ToString();
                                    if (!nextValueString.Contains("float"))
                                    {
                                        nextValueString = nextValueString.Split('^').First().Split(',').First();
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
                                    var itemParents = GetParentClasses(itemName);
                                    if (itemParents != null)
                                    {
                                        var newItem = string.Empty;
                                        var newValueDefinition = " ";
                                        var ParentList = itemParents.Split(':').ToList();

                                        foreach (var parent in ParentList)
                                        {
                                            if (newItem == null)
                                            {
                                                for (var i = index; i < expression.Count() - 1; ++i)
                                                {
                                                    var newNextElement = expression.ElementAt(i + 1).Replace(itemName, parent);
                                                    if (CheckDatatypeProperty(newNextElement, false) == true)
                                                    {
                                                        var newValueList = valueDefinition.Split(':').ToList();
                                                        var basePointsWord = string.Empty;
                                                        var descriptionFound = false;
                                                        var itemClassName = GetParentClasses(itemName);
                                                        var itemClassDescription = string.Empty;
                                                        while (descriptionFound == false)
                                                        {
                                                            var ItemClass = CurrentOntology.Model.ClassModel.SelectClass($"{CurrentContext}{itemClassName}");
                                                            if (ItemClass != null)
                                                            {
                                                                var classAnnotations = CurrentOntology.Data.Annotations.CustomAnnotations.Where(entry => entry.TaxonomySubject == ItemClass);
                                                                if (classAnnotations.Count() > 0)
                                                                {
                                                                    var Valued_List_InfoAnnotations = classAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("Valued_List_Info"));
                                                                    if (Valued_List_InfoAnnotations.Count() > 0)
                                                                    {
                                                                        itemClassDescription = Valued_List_InfoAnnotations.Single().TaxonomyObject.ToString().Split('^').First();
                                                                        descriptionFound = true;
                                                                    }
                                                                }
                                                            }
                                                            itemClassName = GetParentClasses(itemClassName);
                                                        }

                                                        var DescriptionRows = itemClassDescription.Split('\n').ToList();
                                                        foreach (var row in DescriptionRows)
                                                        {
                                                            var rowElements = row.Split(',').ToList();
                                                            var userEditValue = Convert.ToBoolean(rowElements.Where(element => element.Contains("user_edit")).Single().Split(':').Last());
                                                            if (userEditValue == true)
                                                            {
                                                                basePointsWord = rowElements.First().Split('_').Last();
                                                                break;
                                                            }
                                                        }

                                                        var listCount = newValueList.Count();
                                                        for (var listIndex = 0; listIndex < listCount; ++listIndex)
                                                        {
                                                            if (newValueList.ElementAt(listIndex).Contains(basePointsWord))
                                                            {
                                                                newValueList.RemoveAt(listIndex);
                                                                newValueList.Insert(listIndex, User_Input);
                                                            }
                                                        }

                                                        foreach (var item in newValueList)
                                                        {
                                                            var listIndex = newValueList.IndexOf(item);
                                                            if (listIndex == i + 1)
                                                                newValueDefinition += $"{newNextElement}{":"}";
                                                            else
                                                                newValueDefinition += $"{item}{":"}";
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
                            var characterClassName = GetElementClass(this.Name, true).Name;
                            var nextElementFirstWord = NextElement.Split('_').ToList().First();
                            if (!characterClassName.Contains(nextElementFirstWord))
                            {
                                var nextElementClass = CurrentOntology.Model.ClassModel.Where(item => item.ToString().Contains(nextElementFirstWord)).Single();
                                var nextElementClassName = nextElementClass.ToString().Split('#').Last();
                                var CharacterNextElementEntry = this.Ontology.Data.Relations.ClassType.Where(entry => entry.TaxonomyObject.ToString().Contains(nextElementClassName)).Single();
                                var CharacterNextElementFactName = CharacterNextElementEntry.TaxonomySubject.ToString().Split('#').Last();
                                if (!CheckIndividual(CharacterNextElementFactName))
                                    CharacterFact = CreateIndividual(CharacterNextElementFactName);
                                else
                                    CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{CharacterNextElementFactName}");
                            }
                            else
                                CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");

                            if (!CheckDatatypeProperty(NextElement))
                                predicate = CreateDatatypeProperty(NextElement);
                            else
                                predicate = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{NextElement}") as RDFOntologyDatatypeProperty;

                            var nextValueString = string.Empty;
                            var CharacterPredicateAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                            var CharacterPredicateEntry = CharacterPredicateAssertions.Where(entry => entry.TaxonomyPredicate == predicate).Single();
                            if (CharacterPredicateEntry == null)
                            {
                                var predicateName = predicate.ToString().Split('#').Last();
                                var GamePredicate = CurrentOntology.Model.PropertyModel.SelectProperty($"{CurrentContext}{predicateName}") as RDFOntologyDatatypeProperty;
                                CharacterPredicateEntry = CurrentOntology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(GamePredicate).Single();
                                var predicateDefinition = CharacterPredicateEntry.TaxonomyObject.ToString().Split('^').First();
                                var predicateType = CharacterPredicateEntry.TaxonomyObject.ToString().Split('^').Last();
                                nextValueString = $"{GetValue(predicateDefinition)}{predicateType}";
                            }
                            else
                                nextValueString = CharacterPredicateEntry.TaxonomyObject.ToString();

                            var nextValueDigits = nextValueString.Split('^').First();
                            if (!nextValueString.Contains("float"))
                            {
                                nextValueDigits = nextValueString.Split(',').First();
                                nextValue = Convert.ToSingle(nextValueDigits);
                            }
                            else
                            {
                                isFloat = true;
                                nextValueString = nextValueString.Split('^').First();
                                nextValue = Convert.ToSingle(nextValueString, FileService.Culture());
                            }
                        }
                    }

                    if (element == "/" && nextValue == 0)
                        nextValue = 1;
                    var operatorResult = ConvertToOperator(element, Convert.ToSingle(CurrentValue), nextValue);
                    if (operatorResult.GetType().ToString().Contains("boolean"))
                    {
                        foreach (var individual in GetIndividuals(itemName))
                            if (operatorResult == true)
                                currentList.Add(individual.Name);
                    }
                    else
                    {
                        if (isFloat == false)
                            CurrentValue = operatorResult.ToString().Split(',').First();
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
                    var property = expression.ElementAt(index + 1);

                    var currentFact = CurrentOntology.Data.SelectFact($"{CurrentContext}{element}{"_"}{User_Input}");
                    var currentProperty = CurrentOntology.Model.PropertyModel.SelectProperty($"{CurrentContext}{property}");
                    var elementAssertion = CurrentOntology.Data.Relations.Assertions.SelectEntriesBySubject(currentFact).Where(entry => entry.TaxonomyPredicate == currentProperty).Single();
                    CurrentValue = elementAssertion.TaxonomyObject.ToString().Split('^').First();
                }
                else if (CheckIndividual(element, false))
                {
                    var property = expression.ElementAt(index + 1);
                    var currentFact = CurrentOntology.Data.SelectFact($"{CurrentContext}{element}");
                    var currentProperty = CurrentOntology.Model.PropertyModel.SelectProperty($"{CurrentContext}{property}");
                    if (currentProperty == null)
                    {
                        var propertyWords = property.Split('_').ToList();
                        var wordCounter = propertyWords.Count();
                        var elementFound = false;
                        var propertyName = string.Empty;

                        while (elementFound == false && wordCounter > 1)
                        {
                            propertyName = string.Empty;
                            for (var i = 0; i < wordCounter - 1; ++i)
                                propertyName += $"{propertyWords.ElementAt(i)}{"_"}";
                            propertyName += $"{propertyWords.Last()}";

                            if (CheckDatatypeProperty(propertyName, false) == true)
                            {
                                elementFound = true;
                                currentProperty = CurrentOntology.Model.PropertyModel.SelectProperty($"{CurrentContext}{propertyName}");
                            }
                            --wordCounter;
                        }
                    }

                    var elementPropertyEntry = CurrentOntology.Data.Relations.Assertions.SelectEntriesBySubject(currentFact).Where(entry => entry.TaxonomyPredicate == currentProperty).Single();
                    CurrentValue = elementPropertyEntry.TaxonomyObject.ToString().Split('^').First();
                }
                else if (CheckObjectProperty(element, false))
                {
                    var currentPropertyName = expression.ElementAt(index);
                    var previousProperty = expression.ElementAt(index - 1);
                    if (!operators.Any(op => previousProperty == op))
                    {
                        RDFOntologyFact SubjectFact;
                        RDFOntologyObjectProperty objectPredicate;
                        RDFOntologyTaxonomy SubjectFactAssertions;
                        RDFOntologyTaxonomyEntry SubjectFactPredicateEntry;
                        var CharacterClass = new List<string> { "Personaje", "Character", "Personnage" };

                        var ItemClassName = string.Empty;
                        if (itemName != null)
                            ItemClassName = GetElementClass(itemName, applyOnCharacter).Name;
                        var elementFirstWord = element.Split('_').ToList().First();

                        var CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
                        var CharacterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                        var datatypeAssertionFound = false;
                        var characterDatatypePropertyFirstWord = string.Empty;
                        foreach (var entry in CharacterAssertions)
                        {
                            var propertyName = entry.TaxonomyPredicate.ToString().Split('#').Last();
                            if (CheckDatatypeProperty(propertyName) == true)
                            {
                                var DatatypeProperty = entry.TaxonomyPredicate as RDFOntologyDatatypeProperty;
                                if (DatatypeProperty.Domain != null)
                                {
                                    if (CharacterClass.Any(word => DatatypeProperty.Domain.ToString().Contains(word)))
                                    {
                                        characterDatatypePropertyFirstWord = DatatypeProperty.ToString().Split('#').Last().Split('_').First();
                                        datatypeAssertionFound = true;
                                    }
                                }
                            }

                            if (datatypeAssertionFound == true)
                                break;
                        }

                        if (ItemClassName != null && ItemClassName.Contains(characterDatatypePropertyFirstWord))
                        {
                            if (CharacterClass.Any(word => word.Contains(characterDatatypePropertyFirstWord)))
                                SubjectFact = CharacterFact;
                            else
                            {
                                if (!CheckIndividual(itemName))
                                    CreateIndividual(itemName);
                                SubjectFact = this.Ontology.Data.SelectFact($"{this.Context}{itemName}");
                            }
                        }
                        else if (ItemClassName == null)
                            SubjectFact = CharacterFact;
                        else
                        {
                            var useCharacterContext = CheckObjectProperty(element);
                            if (useCharacterContext == true)
                                objectPredicate = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context }{element}") as RDFOntologyObjectProperty;
                            else
                                objectPredicate = CurrentOntology.Model.PropertyModel.SelectProperty($"{CurrentContext}{element}") as RDFOntologyObjectProperty;

                            RDFOntologyObjectProperty ParentProperty = objectPredicate;
                            var elementHasParentProperty = true;
                            while (elementHasParentProperty == true)
                            {
                                if (useCharacterContext == true)
                                {
                                    var predicateParentAssertion = this.Ontology.Model.PropertyModel.Relations.SubPropertyOf.SelectEntriesBySubject(objectPredicate);
                                    if (predicateParentAssertion.Count() == 0)
                                    {
                                        ParentProperty = objectPredicate;
                                        elementHasParentProperty = false;
                                    }
                                    else
                                        objectPredicate = predicateParentAssertion.Single().TaxonomyObject as RDFOntologyObjectProperty;
                                }
                                else
                                {
                                    var predicateParentAssertion = CurrentOntology.Model.PropertyModel.Relations.SubPropertyOf.SelectEntriesBySubject(objectPredicate);
                                    if (predicateParentAssertion.Count() == 0)
                                    {
                                        ParentProperty = objectPredicate;
                                        elementHasParentProperty = false;
                                    }
                                    else
                                        objectPredicate = predicateParentAssertion.Single().TaxonomyObject as RDFOntologyObjectProperty;
                                }
                            }

                            if (CharacterClass.Any(word => ParentProperty.ToString().Contains(word)))
                                SubjectFact = CharacterFact;
                            else
                            {
                                if (!CheckIndividual(itemName))
                                    CreateIndividual(itemName);
                                SubjectFact = this.Ontology.Data.SelectFact($"{this.Context}{itemName}");
                            }

                        }

                        if (!CheckObjectProperty(element))
                            CreateObjectProperty(element);

                        var SubjectContext = SubjectFact.ToString().Split('#').Last();
                        if (SubjectContext == this.Context)
                            objectPredicate = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Ontology}{element}") as RDFOntologyObjectProperty;
                        else
                            objectPredicate = CurrentOntology.Model.PropertyModel.SelectProperty($"{CurrentOntology}{element}") as RDFOntologyObjectProperty;

                        if (SubjectContext == this.Context)
                            SubjectFactAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);
                        else
                            SubjectFactAssertions = CurrentOntology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);

                        if (SubjectFactAssertions.Count() > 1)
                            SubjectFactPredicateEntry = SubjectFactAssertions.SelectEntriesByPredicate(objectPredicate).Single();
                        else
                            SubjectFactPredicateEntry = SubjectFactAssertions.Single();

                        if (SubjectFactPredicateEntry != null)
                            SubjectRef = SubjectFactPredicateEntry.TaxonomyObject.ToString().Split('#').Last();

                        var nextProperty = expression.ElementAt(index + 1);
                        if (nextProperty.Contains("Item"))
                            nextProperty = nextProperty.Replace("Item", itemName);
                        if (nextProperty.Contains("Ref"))
                            nextProperty = nextProperty.Replace("Ref", SubjectRef);

                        int nextPropertyCounter = 1;
                        if (CheckDatatypeProperty(nextProperty))
                        {
                            currentPropertyName = nextProperty;
                            var DatatypePredicate = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{nextProperty}") as RDFOntologyDatatypeProperty;

                            var characterClassName = GetElementClass(this.Name, true).Name;
                            var nextPropertyFirstWord = nextProperty.Split('_').ToList().First();
                            if (characterClassName.Contains(nextPropertyFirstWord))
                                CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
                            else
                                CharacterFact = this.Ontology.Data.SelectFact(this.Context + SubjectRef);

                            var CharacterFactAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                            var CharacterFactPredicateEntry = CharacterFactAssertions.SelectEntriesByPredicate(DatatypePredicate).Single();
                            if (CharacterFactPredicateEntry == null)
                            {
                                CurrentValue = 0.ToString();
                                SubjectRef = CurrentValue;
                            }
                            else
                            {
                                var row_index = index;

                                var GameDatatypePredicate = CurrentOntology.Model.PropertyModel.SelectProperty($"{CurrentContext}{nextProperty}") as RDFOntologyDatatypeProperty;
                                var PropertyAnnotations = CurrentOntology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(GameDatatypePredicate);
                                var PropertyUpperLimitAnnotation = PropertyAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("hasUpperLimit"));
                                if (PropertyUpperLimitAnnotation.Count() > 0)
                                {
                                    hasUpperLimit = true;
                                    UpperLimit = Convert.ToSingle(PropertyUpperLimitAnnotation.Single().TaxonomyObject.ToString().Split('^').First());
                                }
                                var nextPropertyObject = CharacterFactPredicateEntry.TaxonomyObject.ToString();
                                if (!nextPropertyObject.Contains("float"))
                                {
                                    CurrentValue = nextPropertyObject.Split('^').First().Split(',').First();
                                    SubjectRef = CurrentValue;
                                    continue;
                                }
                                else
                                {
                                    CurrentValue = nextPropertyObject.Split('^').First();
                                    SubjectRef = CurrentValue;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            var firstTime = true;
                            while (CheckObjectProperty(nextProperty))
                            {
                                if (firstTime)
                                {
                                    if (!CheckIndividual(itemName))
                                        SubjectFact = CreateIndividual(itemName);
                                    else
                                        SubjectFact = this.Ontology.Data.SelectFact($"{this.Context}{itemName}");
                                    firstTime = false;
                                }

                                if (!CheckObjectProperty(element))
                                    objectPredicate = CreateObjectProperty(element);
                                else
                                    objectPredicate = this.Ontology.Model.PropertyModel.SelectProperty($"{CurrentContext}{element}") as RDFOntologyObjectProperty;

                                var ItemFactAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);
                                var ItemFactPredicateEntry = ItemFactAssertions.Where(assertion => assertion.TaxonomyPredicate == objectPredicate).Single();
                                SubjectFact = ItemFactPredicateEntry.TaxonomyObject as RDFOntologyFact;
                                ++nextPropertyCounter;
                                nextProperty = expression.ElementAt(index + nextPropertyCounter);
                            }
                            currentPropertyName = nextProperty;
                        }

                        if (CheckDatatypeProperty(currentPropertyName, false))
                        {
                            RDFOntologyDatatypeProperty currentProperty;
                            var characterClassName = GetElementClass(this.Name, true).Name;
                            var nextElementFirstWord = currentPropertyName.Split('_').ToList().First();
                            if (characterClassName.Contains(nextElementFirstWord))
                                SubjectFact = CharacterFact;
                            else
                                SubjectFact = this.Ontology.Data.SelectFact($"{this.Context}{SubjectRef}");

                            if (!CheckDatatypeProperty(currentPropertyName))
                                currentProperty = CreateDatatypeProperty(currentPropertyName);
                            else
                                currentProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{currentPropertyName}") as RDFOntologyDatatypeProperty;
                            var subjectFactAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);
                            var subjectFactPropertyEntry = subjectFactAssertions.Where(entry => entry.TaxonomyPredicate == currentProperty).Single();
                            if (subjectFactPropertyEntry == null)
                            {
                                CurrentValue = 0.ToString();
                                SubjectRef = CurrentValue;
                            }
                            else
                            {
                                var nextPropertyObject = subjectFactPropertyEntry.TaxonomyObject.ToString();
                                if (!nextPropertyObject.Contains("float"))
                                {
                                    CurrentValue = nextPropertyObject.Split('^').First().Split(',').First();
                                    SubjectRef = CurrentValue;
                                }

                                else
                                {
                                    CurrentValue = nextPropertyObject.Split('^').First();
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
                        var CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
                        RDFOntologyDatatypeProperty predicate;
                        if (!CheckDatatypeProperty(element))
                            predicate = CreateDatatypeProperty(element);
                        else
                            predicate = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{element}") as RDFOntologyDatatypeProperty;
                        var CharacterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                        var CharacterPredicateEntry = CharacterAssertions.Where(entry => entry.TaxonomyPredicate == predicate).Single();
                        if (CharacterPredicateEntry == null)
                        {
                            var GamePredicate = CurrentOntology.Model.PropertyModel.SelectProperty($"{CurrentContext}{element}") as RDFOntologyDatatypeProperty;
                            var PredicateDefaultValueEntry = CurrentOntology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(GamePredicate).Single();
                            if (PredicateDefaultValueEntry != null)
                            {
                                var PredicateDefaultValueDefinition = PredicateDefaultValueEntry.TaxonomyObject.ToString();
                                var valuetype = PredicateDefaultValueDefinition.Split('#').Last();
                                PredicateDefaultValueDefinition = PredicateDefaultValueDefinition.Split('^').First();
                                var predicateValue = GetValue(PredicateDefaultValueDefinition).ToString();
                                AddDatatypeProperty(this.Context + this.Name, predicate.ToString(), predicateValue, valuetype);
                                CharacterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                                CharacterPredicateEntry = CharacterAssertions.Where(entry => entry.TaxonomyPredicate == predicate).Single();
                            }
                            else
                            {
                                var valuetype = predicate.Range.ToString().Split('#').Last();
                                AddDatatypeProperty(this.Context + this.Name, predicate.ToString(), User_Input.ToString(), valuetype);
                                CharacterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                                CharacterPredicateEntry = CharacterAssertions.Where(entry => entry.TaxonomyPredicate == predicate).Single();
                            }

                        }

                        var PropertyAnnotations = CurrentOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(predicate);
                        var PropertyUpperLimitAnnotation = PropertyAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("hasUpperLimit"));
                        if (PropertyUpperLimitAnnotation.Count() > 0)
                        {
                            hasUpperLimit = true;
                            UpperLimit = Convert.ToSingle(PropertyUpperLimitAnnotation.Single().TaxonomyObject.ToString().Split('^').First());
                        }
                        else
                        {
                            var CurrentValueString = CharacterPredicateEntry.TaxonomyObject.ToString();
                            if (!CurrentValueString.Contains("float"))
                            {
                                CurrentValue = CurrentValueString.Split('^').First().Split(',').First();
                                SubjectRef = CurrentValue;
                            }
                            else
                            {
                                CurrentValue = CurrentValueString.Split('^').First();
                                SubjectRef = CurrentValue;
                            }
                        }
                    }
                    else
                    {
                        var previousElement = expression.ElementAt(index - 1);
                        if (CheckIndividual(element, false))
                        {
                            RDFOntologyFact subjectFact;
                            if (!CheckIndividual(element))
                                subjectFact = CreateIndividual(element);
                            else
                                subjectFact = this.Ontology.Data.SelectFact($"{this.Context}{element}");

                            RDFOntologyDatatypeProperty predicate;
                            if (!CheckDatatypeProperty(element))
                                predicate = CreateDatatypeProperty(element);
                            else
                                predicate = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{element}") as RDFOntologyDatatypeProperty;

                            var SubjectFactPredicateAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(subjectFact);
                            SubjectFactPredicateAssertions = SubjectFactPredicateAssertions.SelectEntriesByPredicate(predicate);

                            if (SubjectFactPredicateAssertions.Count() > 0)
                            {
                                var entry = SubjectFactPredicateAssertions.Single();
                                var PropertyAnnotations = CurrentOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(predicate);
                                var PropertyUpperLimitAnnotation = PropertyAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("hasUpperLimit"));
                                if (PropertyUpperLimitAnnotation.Count() > 0)
                                {
                                    var UpperLimitValue = PropertyUpperLimitAnnotation.Single().TaxonomyObject.ToString().Split('^').First();
                                    var PropertyObjectValue = entry.TaxonomyObject.ToString().Split('^').First();
                                    var limitResult = ConvertToOperator("<", Convert.ToSingle(PropertyObjectValue), Convert.ToSingle(UpperLimitValue));
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
                                        CurrentValue = CurrentValueString.Split('^').First().Split(',').First();
                                        SubjectRef = CurrentValue;
                                    }
                                    else
                                    {
                                        CurrentValue = CurrentValueString.Split('^').First();
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
            else if (CurrentValue.Contains("^"))
                CurrentValue = CurrentValue.Substring(0, CurrentValue.IndexOf('^'));

            if (hasUpperLimit == true)
            {
                var result = ConvertToOperator("<", Convert.ToSingle(CurrentValue), UpperLimit);
                if (result == false)
                    CurrentValue = UpperLimit.ToString();
            }

            return Convert.ToSingle(CurrentValue);
        }

    }
}
