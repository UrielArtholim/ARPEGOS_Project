
namespace ARPEGOS.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using ARPEGOS.Helpers;
    using RDFSharp.Model;
    using RDFSharp.Semantics.OWL;

    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Check if literal exists inside the current ontology
        /// </summary>
        /// <param name="value">Value of the literal</param>
        /// <param name="type">Semantic datatype of the literal</param>
        /// <returns></returns>
        public bool CheckLiteral (string value, string type, bool applyOnCharacter = true)
        {
            RDFOntology CurrentOntology;
            if (applyOnCharacter)
                CurrentOntology = this.Ontology;
            else
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;

            var dataModel = CurrentOntology.Data;
            var literalType = CheckDatatypeFromString(type);
            var literal = new RDFTypedLiteral(value, literalType);
            return dataModel.SelectLiteral(literal.ToString()) != null;
        }

        /// <summary>
        /// Check if fact exists inside the current ontology
        /// </summary>
        /// <param name="name">Name of the fact</param>
        /// <returns></returns>
        public bool CheckFact (string elementString, bool applyOnCharacter = true)
        {
            RDFOntology CurrentOntology;
            if (applyOnCharacter)
                CurrentOntology = this.Ontology;
            else
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;

            var dataModel = CurrentOntology.Data;
            return dataModel.SelectFact(elementString) != null;
        }

        /// <summary>
        /// Check if class exists inside the current ontology
        /// </summary>
        /// <param name="name">Name of the class</param>
        /// <returns></returns>
        public bool CheckClass (string elementString, bool applyOnCharacter = true)
        {
            RDFOntology CurrentOntology;
            if (applyOnCharacter)
                CurrentOntology = this.Ontology;
            else
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
            
            var classModel = CurrentOntology.Model.ClassModel;
            return classModel.SelectClass(elementString) != null;
        }

        /// <summary>
        /// Check if object property exists inside the current ontology
        /// </summary>
        /// <param name="name">Name of the object property</param>
        /// <returns></returns>
        public bool CheckObjectProperty (string elementString, bool applyOnCharacter = true)
        {
            RDFOntology CurrentOntology;

            if (applyOnCharacter)
                CurrentOntology = this.Ontology;
            else
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;

            var exists = false;
            var propertyModel = CurrentOntology.Model.PropertyModel;
            var property = propertyModel.SelectProperty(elementString);

            if (property != null)
            {
                var objectEnumerator = propertyModel.ObjectPropertiesEnumerator;
                var shortName = elementString.Split('#').Last();
                while (objectEnumerator.MoveNext())
                {
                    // performance doesn't change drastically from lastindexof + substring, and with split is more readable
                    if (objectEnumerator.Current?.ToString().Split('#').Last() == shortName)
                    {
                        exists = true;
                        break;
                    }
                }
            }

            return exists;
        }

        /// <summary>
        /// Check if datatype property exists inside the current ontology
        /// </summary>
        /// <param name="name">Name of the datatype property</param>
        /// <returns></returns>
        public bool CheckDatatypeProperty (string elementString, bool applyOnCharacter = true)
        {
            RDFOntology CurrentOntology;
            if (applyOnCharacter)

                CurrentOntology = this.Ontology;
            else
                CurrentOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;

            var exists = false;
            var propertyModel = CurrentOntology.Model.PropertyModel;
            var property = propertyModel.SelectProperty(elementString);

            if (property != null)
            {
                var datatypeEnumerator = propertyModel.DatatypePropertiesEnumerator;
                var shortName = elementString.Split('#').Last();
                while (datatypeEnumerator.MoveNext())
                {
                    var propertyName = datatypeEnumerator.Current?.ToString().Split('#').Last();
                    // performance doesn't change drastically from lastindexof + substring, and with split is more readable
                    if (datatypeEnumerator.Current?.ToString().Split('#').Last() == shortName)
                    {
                        exists = true;
                        break;
                    }
                }
            }

            return exists;
        }

        /// <summary>
        /// Check if individual exists inside the current ontology
        /// </summary>
        /// <param name="name">Name of the individual</param>
        /// <returns></returns>
        public bool CheckIndividual (string elementString, bool applyOnCharacter = true)
        {
            return this.CheckFact(elementString, applyOnCharacter);
        }

        /// <summary>
        /// Returns the general cost associated to the given stage
        /// </summary>
        /// <param name="stage">Name of the stage</param>
        /// <returns></returns>
        public string CheckGeneralCost (string stageString, bool applyOnCharacter = false)
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

            var generalCostPredicateName = string.Empty;
            var stageWords = stageString.Split('#').Last().Split('_');
            var filterCounter = 2;
            var wordCounter = stageWords.Length;

            while (filterCounter > 1)
            {
                if (wordCounter > 0)
                {
                    var subjectFactName = string.Join('_', stageWords.Take(wordCounter));
                    var subjectFactString = GetString(subjectFactName, applyOnCharacter);
                    var subjectFact = CurrentOntology.Model.ClassModel.SelectClass(subjectFactString);
                    if (subjectFact != null)
                    {
                        var subjectFactCostAnnotations = CurrentOntology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(subjectFact).Where(entry => entry.ToString().Contains("GeneralCost"));
                        if (subjectFactCostAnnotations.Count() == 1)
                        {
                            filterCounter = 1;
                            generalCostPredicateName = subjectFactCostAnnotations.Single().TaxonomyObject.ToString().Split('^').First();
                        }
                    }
                    --wordCounter;
                }
                else
                    filterCounter = 0;
            }

            if (filterCounter != 1)
            {
                var parents = this.GetParentClasses(stageString);
                if (parents != null)
                {
                    foreach (var parent in parents.Split('|'))
                    {
                        generalCostPredicateName = this.CheckGeneralCost(parent);
                        if (generalCostPredicateName != null)
                            break;
                    }
                }
            }
            return generalCostPredicateName;
        }

        /// <summary>
        /// Returns the description of a valued list view given the stage
        /// </summary>
        /// <param name="stage">Name of the stage stage</param>
        /// <returns></returns>
        public string CheckValueListInfo (string stageString, bool applyOnCharacter = false)
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

            string info = null;

            var stageClass = CurrentOntology.Model.ClassModel.SelectClass(stageString);
            var stageAnnotations = CurrentOntology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(stageClass);
            var stageDefinitionAnnotation = stageAnnotations.Single(entry => entry.TaxonomyPredicate.ToString().Contains("ValuedListInfo"));
            if (stageDefinitionAnnotation != null)
                info = stageDefinitionAnnotation.TaxonomyObject.ToString().Split('^').First();
            else
            {
                var parents = this.GetParentClasses(stageString, applyOnCharacter);
                if (parents != null)
                {
                    var parentList = parents.Split('|').ToList();
                    foreach (var parent in parentList)
                    {
                        info = this.CheckValueListInfo(parent, applyOnCharacter);
                        if (info != null)
                            break;
                    }
                }
            }

            return info;
        }

        /// <summary>
        /// Returns a list of elements available given the current stage, true if the stage has a general limit, the general limit and the partial limit
        /// </summary>
        /// <param name="stage">Name of the stage</param>
        /// <param name="hasGeneralLimitValue">Boolean which tells if the stage has general limit</param>
        /// <param name="GeneralLimitValue">Value of the general limit</param>
        /// <param name="PartialLimitValue">Value of the partial limit</param>
        /// <returns></returns>
        public List<Item> CheckAvailableOptions (string stageString, bool hasGeneralLimitValue, string GeneralLimitName, double generalLimitValue, string StageLimitName, double partialLimitValue)
        {
            RDFOntology GameOntology = DependencyHelper.CurrentContext.CurrentGame.Ontology;
            var costWords = new List<string> { "Coste", "Cost", "Coût" };
            var requirementWords = new List<string> { "Requisito", "Requirement", "Requisite", "Prérequis" };
            var stageOptions = new List<Item>();
            var availableOptions = new List<Item>();
            // Check if stage has subclasses
            var stageClass = GameOntology.Model.ClassModel.SelectClass(stageString);
            var stageSubclassesEntries = GameOntology.Model.ClassModel.Relations.SubClassOf.SelectEntriesByObject(stageClass);
            bool stageHasGroups = false;
            var stageGroups = this.GetIndividualsGrouped(stageString);

            if (stageSubclassesEntries.EntriesCount > 0)
            {
                stageHasGroups = true;
                foreach (var group in stageGroups)
                    foreach (var item in group)
                        stageOptions.Add(item);
            }
            else
            {
                var stageElements = this.GetIndividuals(stageString);
                foreach (var item in stageElements)
                    stageOptions.Add(item);
            }

            if(stageHasGroups == true)
            {
                Debug.WriteLine($"Stage Groups |-| ");
                var groupIndex = 1;
                foreach(var group in stageGroups)
                {
                    var itemIndex = 1;
                    Debug.Write($"Group {groupIndex} - ");
                    foreach (var item in group.Elements)
                    {
                        Debug.WriteLine($"Item {itemIndex} - {item.ShortName}");
                        stageOptions.Add(item);
                        Debug.WriteLine($"--------------------------");
                        ++itemIndex;
                    }
                    Debug.WriteLine($"==========================");
                    ++groupIndex;
                    itemIndex = 1;
                }
            }
            else
            {
                Debug.WriteLine($"Stage Items |-| ");
                var stageItems = this.GetIndividuals(stageString);
                var itemIndex = 1;
                foreach (var item in stageItems)
                {
                    Debug.WriteLine($"Item {itemIndex} - {item.ShortName}");
                    stageOptions.Add(item);
                    ++itemIndex;
                }
                Debug.WriteLine($"==========================");
            }

            Debug.WriteLine($"Item Available Checks |-| ");
            var itemNumber = 1;
            foreach (Item item in stageOptions)
            {
                Debug.WriteLine($"Item {itemNumber} - {item.ShortName}");

                if (hasGeneralLimitValue == false || generalLimitValue > 0)
                {
                    if (partialLimitValue > 0)
                    {
                        var itemFact = GameOntology.Data.SelectFact(item.FullName);//IBAS POR AQUI
                        var itemFactAssertions = GameOntology.Data.Relations.Assertions.SelectEntriesBySubject(itemFact);
                        var itemRequirements = itemFactAssertions.Where(entry => requirementWords.Any(word => entry.ToString().Contains(word)));
                        string datatypeRequirementName;
                        var allRequirementsFulfilled = true;
                        var requirementsChecked = new List<string>();
                        foreach (var entry in itemRequirements)
                        {
                            if (allRequirementsFulfilled)
                            {
                                var objectRequirementNameList = new List<string>();
                                var objectRequirementNameDictionary = new Dictionary<string, bool>();
                                datatypeRequirementName = entry.TaxonomyPredicate.ToString().Split('#').Last();
                                if (requirementsChecked.Any(req => req == datatypeRequirementName))
                                    continue;
                                requirementsChecked.Add(datatypeRequirementName);
                                var isDatatype = this.CheckDatatypeProperty(datatypeRequirementName);
                                if (isDatatype)
                                {
                                    var requirementNumber = datatypeRequirementName.Split('_').ToList().LastOrDefault();
                                    var itemObjectRequirements = itemFactAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains(datatypeRequirementName) == false);
                                    itemObjectRequirements = itemObjectRequirements.Where(entry => entry.ToString().Contains("_" + requirementNumber));
                                    foreach (var requirementEntry in itemObjectRequirements)
                                    {
                                        var predicateString = requirementEntry.TaxonomyPredicate.ToString();
                                        var requirementShortName = requirementEntry.TaxonomyObject.ToString();
                                        objectRequirementNameList.Add(requirementShortName);
                                        requirementsChecked.Add(predicateString);
                                    }
                                }
                                else
                                    objectRequirementNameList.Add(entry.TaxonomyObject.ToString());

                                foreach (var name in objectRequirementNameList)
                                {
                                    var objectFactString = GetString(name, true);
                                    RDFOntologyFact objectFact = null;
                                    if (!this.CheckIndividual(objectFactString))
                                    {
                                        allRequirementsFulfilled = false;
                                        continue;
                                    }
                                    else
                                        objectFact = this.Ontology.Data.SelectFact(objectFactString);

                                    var characterFact = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
                                    var characterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(characterFact);
                                    var characterRequirementAssertions = characterAssertions.SelectEntriesByObject(objectFact);
                                    if (characterRequirementAssertions.Any())
                                    {
                                        if (isDatatype)
                                        {
                                            var requirementAssertion = characterAssertions.Single(entry => entry.TaxonomyPredicate.ToString() == objectFactString);
                                            var requirementValue = Convert.ToSingle(entry.TaxonomyObject.ToString().Split('^').First());
                                            var characterValue = Convert.ToSingle(requirementAssertion.TaxonomyObject.ToString().Split('^').First());
                                            var result = ConvertToOperator("<", characterValue, requirementValue);
                                            objectRequirementNameDictionary.Add(name, result != true);
                                        }
                                    }
                                    else
                                    {
                                        var requirementAssertions = characterAssertions.Where(entry => entry.TaxonomyPredicate.ToString()==objectFactString);
                                        if (requirementAssertions.Any())
                                        {
                                            if (requirementAssertions.Count() > 1)
                                                requirementAssertions = requirementAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains("Total"));

                                            if (isDatatype)
                                            {
                                                var requirementAssertion = requirementAssertions.SingleOrDefault();
                                                var RequirementValue = Convert.ToSingle(entry.TaxonomyObject.ToString().Split('^').First());
                                                var CharacterValue = Convert.ToSingle(requirementAssertion.TaxonomyObject.ToString().Split('^').First());
                                                var result = ConvertToOperator("<", CharacterValue, RequirementValue);
                                                objectRequirementNameDictionary.Add(name, result != true);
                                            }
                                        }
                                        else
                                            allRequirementsFulfilled = false;
                                    }
                                }
                                if (objectRequirementNameDictionary.Values.All(value => value == false))
                                    allRequirementsFulfilled = false;
                            }
                        }

                        if (allRequirementsFulfilled)
                        {
                            var itemCosts = itemFactAssertions.Where(entry => costWords.Any(word => entry.ToString().Contains(word)));
                            RDFOntologyTaxonomyEntry itemCostEntry = null;

                            if (itemCosts.Count() > 1)
                            {
                                var generalCostPredicateName = this.CheckGeneralCost(stageString);
                                itemCostEntry = itemCosts.Where(entry => entry.TaxonomyPredicate.ToString().Contains(generalCostPredicateName)).Single();
                            }
                            else if (itemCosts.Count() == 1)
                                itemCostEntry = itemCosts.SingleOrDefault();

                            if (itemCostEntry != null)
                            {
                                var costValue = Convert.ToSingle(itemCostEntry.TaxonomyObject.ToString().Split('^').First());
                                var itemCostEntryPredicate = itemCostEntry.TaxonomyPredicate.ToString();

                                var matchPartialLimit = CheckCostAndLimit(itemCostEntryPredicate, StageLimitName);
                                var matchGeneralLimit = CheckCostAndLimit(itemCostEntryPredicate, GeneralLimitName);
                                if (!matchPartialLimit && !matchGeneralLimit)
                                {
                                    var itemCostWords = itemCostEntryPredicate.Split('#').Last().Split('_').ToList();
                                    var characterClass = new List<string> { "Personaje", "Character", "Personnage" };
                                    var characterDatatatypePropertyFound = false;
                                    var index = 0;
                                    itemCostWords.Remove(itemCostWords.First());

                                    while (!characterDatatatypePropertyFound)
                                    {
                                        var property = this.Ontology.Model.PropertyModel.ElementAtOrDefault(index);
                                        if (this.CheckDatatypeProperty(property.ToString()))
                                        {
                                            var datatypeProperty = property as RDFOntologyDatatypeProperty;
                                            if (datatypeProperty.Domain != null)
                                            {
                                                var domainClass = datatypeProperty.Domain.ToString();
                                                if (characterClass.Any(word => domainClass.Contains(word)))
                                                {
                                                    if (!characterClass.Any(word => datatypeProperty.ToString().Contains(word)))
                                                    {
                                                        var datatypePropertyFirstWord = datatypeProperty.ToString().Split('#').Last().Split('_').FirstOrDefault();
                                                        itemCostWords.Insert(0, datatypePropertyFirstWord);
                                                        characterDatatatypePropertyFound = true;
                                                    }
                                                }
                                            }
                                        }
                                        ++index;
                                    }

                                    if (costWords.Any(word => itemCostEntryPredicate.Contains(word)))
                                    {
                                        itemCostWords.Remove(itemCostWords.Last());
                                        itemCostWords.Add("Total");
                                        var itemCostName = string.Join("_", itemCostWords);
                                        if (string.IsNullOrEmpty(this.GetString(itemCostName)))
                                            itemCostWords.Remove(itemCostWords.Last());
                                    }

                                    var requirementCostName = string.Join('_', itemCostWords);
                                    var requirementCostString = GetString(requirementCostName, true);
                                    if (string.IsNullOrEmpty(requirementCostString))
                                        requirementCostString = itemCostEntryPredicate;
                                    var characterFact = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");

                                    if (!(this.Ontology.Model.PropertyModel.SelectProperty(requirementCostString) is RDFOntologyDatatypeProperty requirementCostProperty))
                                    {
                                        IEnumerable<RDFOntologyTaxonomyEntry> requirementCostAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(characterFact);
                                        requirementCostAssertions = requirementCostAssertions.Where(entry => itemCostWords.All(word => entry.TaxonomyPredicate.ToString().Contains(word)));
                                        requirementCostString = requirementCostAssertions.Single().TaxonomyPredicate.ToString();
                                        requirementCostProperty = this.Ontology.Model.PropertyModel.SelectProperty(requirementCostString) as RDFOntologyDatatypeProperty;
                                    }

                                    var entry = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(characterFact).SelectEntriesByPredicate(requirementCostProperty).SingleOrDefault();
                                    var entryValue = Convert.ToSingle(entry.TaxonomyObject.ToString().Split('^').First());
                                    if (entryValue >= costValue)
                                    {
                                        if (hasGeneralLimitValue)
                                            if (generalLimitValue >= costValue)
                                                availableOptions.Add(new Item(item.FullName));
                                            else
                                                availableOptions.Add(new Item(item.FullName));
                                    }
                                }
                                else
                                {
                                    if (matchGeneralLimit || matchPartialLimit)
                                    {
                                        if (matchGeneralLimit && generalLimitValue >= costValue && partialLimitValue >= costValue)
                                            availableOptions.Add(new Item(item.FullName));
                                        else if (matchPartialLimit && partialLimitValue >= costValue)
                                            availableOptions.Add(new Item(item.FullName));
                                    }
                                }
                            }
                        }
                    }
                }
                ++itemNumber;
            }
            return availableOptions;
        }
    }
}
