using RDFSharp.Semantics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ARPEGOS.Services
{
    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Returns class of the given element, choosing if the element is from the character or not
        /// </summary>
        /// <param name="elementName">Name of the element given</param>
        /// <param name="applyOnCharacter">Search inside character</param>
        /// <returns></returns>
        public string GetElementClass (string elementName, bool applyOnCharacter = false)
        {
            RDFOntology CurrentOntology;
            string CurrentContext;
            if(applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = this.Game.Ontology;
                CurrentContext = this.Game.Context;
            }

            var DataModel = CurrentOntology.Data;
            var elementClassType = DataModel.Relations.ClassType.Where(entry => entry.TaxonomySubject.ToString().Contains(elementName)).First();
            var elementClass = elementClassType.TaxonomyObject.ToString().Split('#').Last();
            return elementClass;
        }

        /// <summary>
        /// Returns description of the element
        /// </summary>
        /// <param name="elementName">Name of the element</param>
        /// <returns></returns>
        public string GetElementDescription (string elementName, bool applyOnCharacter = false)
        {
            string elementDesc = null;
            RDFOntology CurrentOntology;
            string CurrentContext;
            if (applyOnCharacter)
            {
                CurrentOntology = this.Ontology;
                CurrentContext = this.Context;
            }
            else
            {
                CurrentOntology = this.Game.Ontology;
                CurrentContext = this.Game.Context;
            }

            var DataModel = CurrentOntology.Data;
            var elementCommentAnnotation = DataModel.Annotations.Comment.Where(entry => entry.TaxonomySubject.ToString().Contains(elementName)).First();
            if (elementCommentAnnotation != null)
                elementDesc = elementCommentAnnotation.TaxonomyObject.ToString().Replace("^^http://www.w3.org/2001/XMLSchema#string", "");
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
                CurrentOntology = this.Game.Ontology;
                CurrentContext = this.Game.Context;
            }

            var ClassModel = CurrentOntology.Model.ClassModel;
            var DataModel = CurrentOntology.Data;
            var currentClass = ClassModel.SelectClass($"{CurrentContext}{className}");
            var classAssertions = DataModel.Relations.ClassType.SelectEntriesByObject(currentClass);
            foreach (var assertion in classAssertions)
            {
                    var individual = assertion.TaxonomySubject;
                    var individualName = individual.ToString().Substring(individual.ToString().LastIndexOf('#') + 1);
                    individuals.Add(new Item(individualName));
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
            string LimitPropertyName = null;
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
                CurrentOntology = this.Game.Ontology;
                CurrentContext = this.Game.Context;
            }

            var PropertyModel = CurrentOntology.Model.PropertyModel;
            var FilterResultsCounter = 0;
            var index = 0;
            var ElementWords = ElementName.Split('_').ToList();
            var CompareList = new List<string>();
            var ResultProperties = new List<RDFOntologyProperty>();

            var ElementClass = CurrentOntology.Model.ClassModel.SelectClass($"{CurrentContext}{ElementName}");
            var ElementClassAnnotations = CurrentOntology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(ElementClass);
            var ElementGeneralCostAnnotation = ElementClassAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("GeneralCostDefinedBy")).Single();
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
            string LimitPropertyName = null;
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
                CurrentOntology = this.Game.Ontology;
                CurrentContext = this.Game.Context;
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

        º
    }
}
