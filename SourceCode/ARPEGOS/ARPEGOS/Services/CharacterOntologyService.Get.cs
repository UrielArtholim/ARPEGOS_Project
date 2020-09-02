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
        public Dictionary<string, string> GetCharacterProperties ()
        {
            var CharacterProperties = new Dictionary<string, string>();
            var characterName = FileService.EscapedName(this.Name);
            var CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{characterName}");
            var CharacterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
            foreach (var assertion in CharacterAssertions)
            {
                var property = assertion.TaxonomyPredicate.ToString();
                var value = assertion.TaxonomyObject.ToString();
                if (!CharacterProperties.ContainsKey(property))
                    CharacterProperties.Add(property, value);
            }

            return CharacterProperties;
        }//MODIFIED

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
                var property = this.Ontology.Model.PropertyModel.SelectProperty(item.Key);
                var annotationProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{AnnotationType}");
                var propertyTaxonomy = this.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(property);
                var propertySkillTaxonomy = propertyTaxonomy.Where(property => property.TaxonomyPredicate.ToString().Contains(AnnotationType));
                if (propertySkillTaxonomy.Count() > 0)
                {
                    if (this.CheckObjectProperty(item.Key) == true)
                    {
                        skillName = item.Value;
                        var skillObjectFact = this.Ontology.Data.SelectFact(skillName);
                        skillDescription = this.Ontology.Data.Annotations.Comment.SelectEntriesBySubject(skillObjectFact).Single().TaxonomyObject.ToString();
                        skills.Add(new Item(skillName, skillDescription));
                    }
                    else
                    {
                        skills.Add(new Item(item.Key, skillDescription)); //DataItem
                    }
                }
            }
            return skills;
        }//MODIFIED


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
        } //DOESN'T NEED MODIFICATION

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

            var DataModel = CurrentOntology.Data;
            RDFOntologyResource currentElementResource;
            if (CheckClass(currentElementString))
                currentElementResource = CurrentOntology.Model.ClassModel.SelectClass(currentElementString);
            else if (CheckFact(currentElementString))
                currentElementResource = CurrentOntology.Data.SelectFact(currentElementString);
            else
                currentElementResource = CurrentOntology.Model.PropertyModel.SelectProperty(currentElementString);


            var elementCommentAnnotation = DataModel.Annotations.Comment.SelectEntriesBySubject(currentElementResource).Single(); 
            if (elementCommentAnnotation != null)
                elementDesc = elementCommentAnnotation.TaxonomyObject.ToString().Split('^').First();
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

            var ClassModel = CurrentOntology.Model.ClassModel;
            var DataModel = CurrentOntology.Data;
            var currentClass = ClassModel.SelectClass(currentClassString);
            var classAssertions = DataModel.Relations.ClassType.SelectEntriesByObject(currentClass);
            foreach (var assertion in classAssertions)
            {
                var individualString = assertion.TaxonomySubject.ToString();
                var individualFact = DataModel.SelectFact(individualString);
                var individualDescription = CurrentOntology.Data.Annotations.Comment.SelectEntriesBySubject(individualFact).Single().TaxonomyObject.ToString().Split('^').First();
                individuals.Add(new Item(individualString, individualDescription));
            }

            return individuals;
        }//MODIFIED

        /// <summary>
        /// Returns a list of groups of individuals given the name of the root class
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public dynamic GetIndividualsGrouped (string classString, bool applyOnCharacter = false)
        {
            dynamic groups;
            ObservableCollection<Group> subclasses = GetSubClasses(classString, applyOnCharacter);
            if (subclasses != null)
            {
                groups = subclasses;
                foreach (Group groupItem in groups)
                    groupItem.GroupList = GetIndividualsGrouped(groupItem.GroupString, applyOnCharacter);
            }
            else
            {
                groups = GetIndividuals(classString, applyOnCharacter);
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
        public string GetAvailablePoints (string ElementString, out float? AvailablePoints, bool applyOnCharacter = false)
        {
            AvailablePoints = null;
            var LimitPropertyString = string.Empty;
            var AvailableWords = new List<string>()
            {
                "Disponible",
                "Available"
            };

            RDFOntology CurrentOntology;
            string CurrentContext;
            string currentElementString = ElementString;
            var shortName = ElementString.Split('#').Last();

            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
                currentElementString = $"{this.Context}{shortName}";
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var PropertyModel = CurrentOntology.Model.PropertyModel;
            var FilterResultsCounter = 0;
            var index = 0;
            var ElementWords = shortName.Split('_').ToList();
            var CompareList = new List<string>();
            var ResultProperties = new List<RDFOntologyProperty>();

            var ElementClass = CurrentOntology.Model.ClassModel.SelectClass(currentElementString);
            var ElementClassAnnotations = CurrentOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(ElementClass);
            var ElementGeneralCostAnnotation = ElementClassAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("GeneralCostDefinedBy")).Single();
            var generalCostAnnotationFound = (ElementGeneralCostAnnotation != null);

            if (generalCostAnnotationFound)
            {
                var AnnotationValue = ElementGeneralCostAnnotation.TaxonomyObject.ToString();
                if (string.IsNullOrEmpty(AnnotationValue))
                    return null;

                var GeneralCostProperty = PropertyModel.SelectProperty(AnnotationValue);
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
                    LimitPropertyString = LimitProperty.ToString().Split('#').Last();
                    var LimitPropertyShortName = LimitPropertyString.Split('#').Last();
                    RDFOntologyDatatypeProperty CharacterLimitProperty;
                    if (CheckDatatypeProperty(LimitPropertyString) == false)
                        CharacterLimitProperty = CreateDatatypeProperty(LimitPropertyString);
                    else
                        CharacterLimitProperty = this.Ontology.Model.PropertyModel.SelectProperty(LimitPropertyString) as RDFOntologyDatatypeProperty;
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
                            var parentHasAvailablePoints = GetAvailablePoints(parent, out float? parentAvailablePoints);
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
        /// Returns true if the given element has a limit and the limit.
        /// </summary>
        /// <param name="ElementName">Name of the element</param>
        /// <param name="LimitValue">Limit value obtained</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetLimit (string ElementString, out float? LimitValue, bool applyOnCharacter = false)
        {
            var LimitPropertyString = string.Empty;
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
            string currentElementString = ElementString;
            var shortName = ElementString.Split('#').Last();
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
                
                currentElementString = $"{this.Context}{shortName}";
            }
            else
            {
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
                CurrentContext = DependencyHelper.CurrentContext.CurrentGame.Context;
            }

            var PropertyModel = CurrentOntology.Model.PropertyModel;
            var ResultProperties = PropertyModel.Where(entry => LimitWords.Any(word => entry.ToString().Contains(word)));
            var ElementWords = shortName.Split('_').ToList();
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
                LimitPropertyString = LimitProperty.ToString();
                var LimitPropertyShortName = LimitPropertyString.Split('#').Last();
                RDFOntologyDatatypeProperty CharacterLimitProperty;
                if (!CheckDatatypeProperty(LimitPropertyString))
                    CharacterLimitProperty = CreateDatatypeProperty(LimitPropertyString);
                else
                    CharacterLimitProperty = this.Ontology.Model.PropertyModel.SelectProperty(LimitPropertyString) as RDFOntologyDatatypeProperty;
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
                var parents = GetParentClasses(LimitPropertyString);
                if (parents != null)
                {
                    var parentList = parents.Split('|').ToList();
                    foreach (var parent in parentList)
                    {
                        if (hasLimit == false)
                        {
                            LimitPropertyString = GetLimit(parent, out LimitValue);
                            hasLimit = LimitPropertyString != null;
                        }
                    }
                }
            }
            return LimitPropertyString;
        }//MODIFIED

        /// <summary>
        /// Returns the name of a limit given its value
        /// </summary>
        /// <param name="value">Value of the limit</param>
        /// <returns></returns>
        public string GetLimitByValue (string stageString, string value)
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
                    var StageGeneralLimit = GetAvailablePoints(stageString, out float? LimitValue);
                    if (LimitValue.ToString() != value)
                    {
                        var StagePartialLimit = GetLimit(stageString, out LimitValue);
                        if (LimitValue.ToString() != value)
                        {
                            var parents = GetParentClasses(stageString);
                            if (parents != null)
                            {
                                var parentList = parents.Split('|').ToList();
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
        }//MODIFIED

        /// <summary>
        /// Returns the name of the object property associated to the stage given
        /// </summary>
        /// <param name="stage">Name of the stage</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetObjectPropertyAssociated (string stageString, bool applyOnCharacter = false)
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
                    propertyString = ObjectPropertyAssertions.Single().ToString();
                    propertyNameFound = true;
                }
                else if (ObjectPropertyAssertions.Count() == 1)
                {
                    propertyString = ObjectPropertyAssertions.Single().ToString();
                    propertyNameFound = true;
                }
                --wordCounter;
            }
            if (wordCounter == 0)
            {
                var parents = GetParentClasses(stageString);
                if (parents != null)
                {
                    var parentList = parents.Split('|').ToList();
                    foreach (var parent in parentList)
                    {
                        if (propertyNameFound == false)
                        {
                            propertyString = GetObjectPropertyAssociated(parent);
                            if (propertyString != null)
                                propertyNameFound = true;
                        }
                    }
                }
            }
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
        }//MODIFIED

        /// <summary>
        /// Returns a string containing the names of the types of the given element
        /// </summary>
        /// <param name="elementName">Name of the element</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetParentClasses (string elementString, bool applyOnCharacter = false)
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

            var parent = string.Empty;
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
        }//MODIFIED

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
        }//MODIFIED

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
            var CharacterClassModel = CurrentOntology.Model.ClassModel;
            var RootClass = CharacterClassModel.SelectClass(currentClassString);
            var SubClassesOfRoot = CharacterClassModel.Relations.SubClassOf.SelectEntriesByObject(RootClass);
            if (SubClassesOfRoot.EntriesCount != 0)
                foreach (var currentEntry in SubClassesOfRoot)
                {
                    var groupString = currentEntry.TaxonomySubject.ToString();
                    var currentClass = CharacterClassModel.SelectClass(groupString);
                    var UpperClassesOfCurrent = CharacterClassModel.Relations.SubClassOf.SelectEntriesBySubject(currentClass);

                    foreach (var currentUpperClassEntry in UpperClassesOfCurrent)
                    {
                        var upperClassString = currentUpperClassEntry.TaxonomyObject.ToString();
                        if (CharacterClassModel.SelectClass(upperClassString) == RootClass)
                            subclasses.Add(new Group(groupString));
                    }
                }
            if (subclasses.Count < 1)
                return null;
            else
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
                            }
                        }
                    }
                }
            }
            else
            {
                if (found == false)
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
                            }
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
                            }
                        }
                    }
                }
            }
            
                

            

            return elementString;
        }

        public float GetValue (string valueDefinition, string itemName = null, string User_Input = null, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            RDFOntology GameOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
            string CurrentContext;
            
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = GameOntology;
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
                        var NextElementString = GetString(NextElement, applyOnCharacter);
                        var CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
                        if (CheckObjectProperty(NextElementString)) //overload this
                        {
                            var row_index = index;
                            var nextElementProperty = CurrentOntology.Model.PropertyModel.SelectProperty(NextElementString) as RDFOntologyObjectProperty;

                            var CharacterFactAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                            var nextElementEntry = CharacterFactAssertions.Where(item => item.TaxonomyPredicate == nextElementProperty).Single();
                            var nextElementFact = nextElementEntry.TaxonomyObject as RDFOntologyFact;

                            ++row_index;
                            NextElement = expression.ElementAt(row_index + 1).Replace("Item", itemName);
                            NextElementString = GetString(NextElement, applyOnCharacter);

                            if (CheckDatatypeProperty(NextElementString, false))
                            {
                                RDFOntologyDatatypeProperty nextElementDatatypeProperty;
                                if (!CheckDatatypeProperty(NextElementString))
                                    nextElementDatatypeProperty = CreateDatatypeProperty(NextElement);
                                else
                                    nextElementDatatypeProperty = this.Ontology.Model.PropertyModel.SelectProperty(GetString(NextElement, true)) as RDFOntologyDatatypeProperty;
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
                                    NextElementString = GetString(NextElement, applyOnCharacter);
                                    RDFOntologyDatatypeProperty nextElementDatatypeProperty;
                                    if (!CheckDatatypeProperty(NextElementString))
                                        nextElementDatatypeProperty = CreateDatatypeProperty(NextElement);
                                    else
                                        nextElementDatatypeProperty = this.Ontology.Model.PropertyModel.SelectProperty(NextElementString) as RDFOntologyDatatypeProperty;

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
                                    var itemParents = GetParentClasses(GetString(itemName, applyOnCharacter), applyOnCharacter);
                                    if (itemParents != null)
                                    {
                                        var newItem = string.Empty;
                                        var newValueDefinition = " ";
                                        var ParentList = itemParents.Split('|').ToList();

                                        foreach (var parent in ParentList)
                                        {
                                            if (newItem == null)
                                            {
                                                for (var i = index; i < expression.Count() - 1; ++i)
                                                {
                                                    var newNextElement = expression.ElementAt(i + 1).Replace(itemName, parent);
                                                    var newNextElementString = GetString(newNextElement, applyOnCharacter);
                                                    if (CheckDatatypeProperty(newNextElement, false) == true)
                                                    {
                                                        var newValueList = valueDefinition.Split(':').ToList();
                                                        var basePointsWord = string.Empty;
                                                        var descriptionFound = false;
                                                        var itemClassFullName = GetElementClass(GetString(itemName,applyOnCharacter),applyOnCharacter).FullName;
                                                        var itemClassDescription = string.Empty;
                                                        while (descriptionFound == false)
                                                        {
                                                            var itemClass = CurrentOntology.Model.ClassModel.SelectClass(itemClassFullName);
                                                            if (itemClass != null)
                                                            {
                                                                var classAnnotations = CurrentOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(itemClass);
                                                                if (classAnnotations.Count() > 0)
                                                                {
                                                                    var Valued_List_InfoAnnotations = classAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("ValuedListInfo"));
                                                                    if (Valued_List_InfoAnnotations.Count() > 0)
                                                                    {
                                                                        itemClassDescription = Valued_List_InfoAnnotations.Single().TaxonomyObject.ToString().Split('^').First();
                                                                        descriptionFound = true;
                                                                    }
                                                                }
                                                            }
                                                            itemClassFullName = GetParentClasses(itemClassFullName, applyOnCharacter);
                                                        }

                                                        var DescriptionRows = itemClassDescription.Split('\n').ToList();
                                                        foreach (var row in DescriptionRows)
                                                        {
                                                            var rowElements = row.Split(',').ToList();
                                                            var userEditValue = Convert.ToBoolean(rowElements.Where(element => element.Contains("User_Edit")).Single().Split('|').Last());
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
                                                                newValueDefinition += $"{newNextElement}{':'}";
                                                            else
                                                                newValueDefinition += $"{item}{':'}";
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
                            var characterClass = GetElementClass($"{this.Context}{FileService.EscapedName(this.Name)}");
                            var characterClassShortName = characterClass.ShortName;
                            var nextElementFirstWord = NextElement.Split('_').ToList().First();
                            var nextElementString = GetString(NextElement);
                            if (!characterClassShortName.Contains(nextElementFirstWord))
                            {
                                var nextElementClass = CurrentOntology.Model.ClassModel.Where(item => item.ToString().Contains(nextElementFirstWord)).Single();
                                var nextElementClassString = nextElementClass.ToString();
                                var CharacterNextElementEntry = this.Ontology.Data.Relations.ClassType.SelectEntriesByObject(nextElementClass).Single();
                                var CharacterNextElementFactString = CharacterNextElementEntry.TaxonomySubject.ToString();
                                if (!CheckIndividual(CharacterNextElementFactString))
                                    CharacterFact = CreateIndividual(CharacterNextElementFactString.Split('#').Last());
                                else
                                    CharacterFact = this.Ontology.Data.SelectFact(CharacterNextElementFactString);
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
                                var predicateString = GetString(predicateName, applyOnCharacter);
                                var GamePredicate = CurrentOntology.Model.PropertyModel.SelectProperty(predicateString) as RDFOntologyDatatypeProperty;
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
                        foreach (var individual in GetIndividuals(GetString(itemName)))
                            if (operatorResult == true)
                                currentList.Add(individual.ShortName);
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
                else if (CheckClass(GetString(element), false))
                {
                    var property = expression.ElementAt(index + 1);
                    var propertyString = GetString(property, applyOnCharacter);
                    var elementInputShortName = $"{element}_{User_Input}";
                    var elementInputString = GetString(elementInputShortName, applyOnCharacter);
                    var currentFact = CurrentOntology.Data.SelectFact(elementInputString);
                    var currentProperty = CurrentOntology.Model.PropertyModel.SelectProperty(propertyString);
                    var elementAssertion = CurrentOntology.Data.Relations.Assertions.SelectEntriesBySubject(currentFact).SelectEntriesByPredicate(currentProperty).Single();
                    CurrentValue = elementAssertion.TaxonomyObject.ToString().Split('^').First();
                }
                else if (CheckIndividual(GetString(element), false))
                {
                    var property = expression.ElementAt(index + 1);
                    var propertyString = GetString(property, applyOnCharacter);
                    var elementString = GetString(element, applyOnCharacter);
                    var currentFact = CurrentOntology.Data.SelectFact(elementString);
                    var currentProperty = CurrentOntology.Model.PropertyModel.SelectProperty(propertyString);
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
                                propertyString = GetString(propertyName, applyOnCharacter);
                                currentProperty = CurrentOntology.Model.PropertyModel.SelectProperty(propertyString);
                            }
                            --wordCounter;
                        }
                    }

                    var elementPropertyEntry = CurrentOntology.Data.Relations.Assertions.SelectEntriesBySubject(currentFact).SelectEntriesByPredicate(currentProperty).Single();
                    CurrentValue = elementPropertyEntry.TaxonomyObject.ToString().Split('^').First();
                }
                else if (CheckObjectProperty(GetString(element), false))
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
                        var itemString = GetString(itemName, applyOnCharacter);
                        if(itemString == string.Empty)
                            itemString = GetString(itemName, !applyOnCharacter);
                        if (itemName != null)
                            ItemClassName = GetElementClass(itemName, applyOnCharacter).ShortName;
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
                                if (!CheckIndividual(itemString))
                                    CreateIndividual(itemName);
                                SubjectFact = this.Ontology.Data.SelectFact(itemString);
                            }
                        }
                        else if (ItemClassName == null)
                            SubjectFact = CharacterFact;
                        else
                        {
                            var elementString = GetString(element, true);
                            var useCharacterContext = CheckObjectProperty(elementString);
                            if (useCharacterContext == true)
                                objectPredicate = this.Ontology.Model.PropertyModel.SelectProperty(elementString) as RDFOntologyObjectProperty;
                            else
                                objectPredicate = CurrentOntology.Model.PropertyModel.SelectProperty(GetString(element)) as RDFOntologyObjectProperty;

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
                                itemString = GetString(itemName, true);
                                if (!CheckIndividual(itemString))
                                    CreateIndividual(itemName);
                                SubjectFact = this.Ontology.Data.SelectFact(itemString);
                            }

                        }

                        if (!CheckObjectProperty(GetString(itemName,true)))
                            CreateObjectProperty(element);

                        var SubjectContext = SubjectFact.ToString().Split('#').First();
                        if (SubjectContext == this.Context)
                            objectPredicate = this.Ontology.Model.PropertyModel.SelectProperty(GetString(element,true)) as RDFOntologyObjectProperty;
                        else
                            objectPredicate = GameOntology.Model.PropertyModel.SelectProperty(GetString(element)) as RDFOntologyObjectProperty;

                        if (SubjectContext == this.Context)
                            SubjectFactAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);
                        else
                            SubjectFactAssertions = GameOntology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);

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

                        var nextPropertyString = GetString(nextProperty, true);

                        int nextPropertyCounter = 1;
                        if (CheckDatatypeProperty(nextProperty))
                        {
                            currentPropertyName = nextProperty;
                            var DatatypePredicate = this.Ontology.Model.PropertyModel.SelectProperty(nextPropertyString) as RDFOntologyDatatypeProperty;

                            var characterClassShortName = GetElementClass(FileService.EscapedName(this.Name), true).ShortName;
                            var nextPropertyFirstWord = nextProperty.Split('_').ToList().First();
                            if (characterClassShortName.Contains(nextPropertyFirstWord))
                                CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
                            else
                                CharacterFact = this.Ontology.Data.SelectFact($"{this.Context}{SubjectRef}");

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

                                var CurrentDatatypePredicate = CurrentOntology.Model.PropertyModel.SelectProperty(GetString(nextProperty, applyOnCharacter)) as RDFOntologyDatatypeProperty;
                                var PropertyAnnotations = CurrentOntology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(CurrentDatatypePredicate);
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
                            while (CheckObjectProperty(GetString(nextProperty,true)))
                            {
                                if (firstTime)
                                {
                                    itemString = GetString(itemName, true);
                                    if (!CheckIndividual(itemString))
                                        SubjectFact = CreateIndividual(itemName);
                                    else
                                        SubjectFact = this.Ontology.Data.SelectFact(itemString);
                                    firstTime = false;
                                }

                                var elementString = GetString(element, true);
                                if (!CheckObjectProperty(elementString))
                                    objectPredicate = CreateObjectProperty(element);
                                else
                                    objectPredicate = this.Ontology.Model.PropertyModel.SelectProperty(elementString) as RDFOntologyObjectProperty;

                                var ItemFactAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);
                                var ItemFactPredicateEntry = ItemFactAssertions.SelectEntriesByPredicate(objectPredicate).Single();
                                SubjectFact = ItemFactPredicateEntry.TaxonomyObject as RDFOntologyFact;
                                ++nextPropertyCounter;
                                nextProperty = expression.ElementAt(index + nextPropertyCounter);
                            }
                            currentPropertyName = nextProperty;
                        }

                        if (CheckDatatypeProperty(currentPropertyName, false))
                        {
                            RDFOntologyDatatypeProperty currentProperty;
                            var characterClassName = GetElementClass(FileService.EscapedName(this.Name), true).ShortName;
                            var nextElementFirstWord = currentPropertyName.Split('_').ToList().First();
                            var SubjectRefString = GetString(SubjectRef, true);
                            if (characterClassName.Contains(nextElementFirstWord))
                                SubjectFact = CharacterFact;
                            else
                                SubjectFact = this.Ontology.Data.SelectFact(SubjectRefString);

                            var currentPropertyString = GetString(currentPropertyName, true);
                            if (!CheckDatatypeProperty(currentPropertyString))
                                currentProperty = CreateDatatypeProperty(currentPropertyName);
                            else
                                currentProperty = this.Ontology.Model.PropertyModel.SelectProperty(currentPropertyString) as RDFOntologyDatatypeProperty;
                            var subjectFactAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact);
                            var subjectFactPropertyEntry = subjectFactAssertions.SelectEntriesByPredicate(currentProperty).Single();
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
                        var elementString = GetString(element, true);
                        if (!CheckDatatypeProperty(elementString))
                            predicate = CreateDatatypeProperty(element);
                        else
                            predicate = this.Ontology.Model.PropertyModel.SelectProperty(elementString) as RDFOntologyDatatypeProperty;
                        var CharacterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                        var CharacterPredicateEntry = CharacterAssertions.SelectEntriesByPredicate(predicate).Single();
                        if (CharacterPredicateEntry == null)
                        {
                            var GamePredicate = CurrentOntology.Model.PropertyModel.SelectProperty(elementString) as RDFOntologyDatatypeProperty;
                            var PredicateDefaultValueEntry = CurrentOntology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(GamePredicate).Single();
                            if (PredicateDefaultValueEntry != null)
                            {
                                var PredicateDefaultValueDefinition = PredicateDefaultValueEntry.TaxonomyObject.ToString();
                                var valuetype = PredicateDefaultValueDefinition.Split('#').Last();
                                PredicateDefaultValueDefinition = PredicateDefaultValueDefinition.Split('^').First();
                                var predicateValue = GetValue(PredicateDefaultValueDefinition).ToString();
                                AddDatatypeProperty($"{this.Context}{FileService.EscapedName(this.Name)}", elementString, predicateValue, valuetype);
                                CharacterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                                CharacterPredicateEntry = CharacterAssertions.SelectEntriesByPredicate(predicate).Single();
                            }
                            else
                            {
                                var valuetype = predicate.Range.ToString().Split('#').Last();
                                AddDatatypeProperty($"{this.Context}{FileService.EscapedName(this.Name)}", elementString, User_Input.ToString(), valuetype);
                                CharacterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                                CharacterPredicateEntry = CharacterAssertions.SelectEntriesByPredicate(predicate).Single();
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
                        var characterString = $"{this.Context}{FileService.EscapedName(this.Name)}";
                        RDFOntologyFact subjectFact;
                        if (!CheckIndividual(characterString))
                            subjectFact = CreateIndividual(FileService.EscapedName(this.Name));
                        else
                            subjectFact = this.Ontology.Data.SelectFact(characterString);

                        RDFOntologyDatatypeProperty predicate;
                        var elementString = GetString(element, true);
                        if (!CheckDatatypeProperty(elementString))
                            predicate = CreateDatatypeProperty(element);
                        else
                            predicate = this.Ontology.Model.PropertyModel.SelectProperty(elementString) as RDFOntologyDatatypeProperty;

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
