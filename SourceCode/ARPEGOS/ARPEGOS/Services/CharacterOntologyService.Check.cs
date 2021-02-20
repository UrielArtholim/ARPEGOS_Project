
namespace ARPEGOS.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
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
                    var subjectFactString = GetFullString(subjectFactName, applyOnCharacter);
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
        public List<Item> CheckAvailableOptions (string stageString, bool hasGeneralLimitValue, string GeneralLimitName, double generalLimitValue, string StageLimitName, double partialLimitValue, string groupName = null)
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            var GameOntology = game.Ontology;
            var costWords = new List<string> { "Coste", "Cost", "Coût" };
            var requirementWords = new List<string> { "Requisito", "Requirement", "Requisite", "Prérequis" };
            var stageOptions = new List<Item>();
            var availableOptions = new List<Item>();
            // Check if stage has subclasses
            var stageClass = GameOntology.Model.ClassModel.SelectClass(stageString);
            var stageSubclassesEntries = GameOntology.Model.ClassModel.Relations.SubClassOf.SelectEntriesByObject(stageClass);
            bool stageHasGroups = false;
            var stageGroups = this.GetIndividualsGrouped(stageString);

            if(groupName != null)
            {
                var groupString = character.GetFullString(groupName);
                var groupIsClass = character.CheckClass(groupString, false);
                if(groupIsClass == true)
                {
                    var groupElements = this.GetIndividuals(groupString);
                    foreach (var item in groupElements)
                        stageOptions.Add(item);
                }
                else
                {
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
                }
            }
            else
            {
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
            foreach(var item in stageOptions)
            {
                Debug.WriteLine($"Item {itemNumber} - {item.ShortName}");

                if (hasGeneralLimitValue == false || generalLimitValue > 0)
                {
                    if (partialLimitValue > 0)
                    {
                        var itemFact = GameOntology.Data.SelectFact(item.FullName);
                        var itemFactAssertions = GameOntology.Data.Relations.Assertions.SelectEntriesBySubject(itemFact);
                        var itemRequirements = itemFactAssertions.Where(entry => requirementWords.Any(word => entry.TaxonomyPredicate.ToString().Contains(word)));
                        string datatypeRequirementName;
                        var allRequirementsFulfilled = true;
                        var requirementsChecked = new List<string>();
                        var requirementsDictionary = new Dictionary<string, string>();
                        var requirementPropertyDictionary = new Dictionary<string, string>();

                        foreach (var entry in itemRequirements)
                        {
                            var currentPredicate = entry.TaxonomyPredicate.ToString().Split('#').Last();

                            var currentObject = entry.TaxonomyObject.ToString();
                            if (currentObject.Contains('^'))
                                currentObject = currentObject.Split('^').First();
                            else
                                currentObject = currentObject.Split('#').Last();
                            var predicateNumber = currentPredicate.Split('_').Last();

                            if (!requirementPropertyDictionary.ContainsKey(currentObject))
                                requirementPropertyDictionary.Add(currentObject, currentPredicate);


                            var coincidences = itemRequirements.Where(req => req != entry && req.TaxonomyPredicate.ToString().Split('#').Last() == currentPredicate);
                            if (coincidences.Count() > 1)
                                if (!requirementsDictionary.ContainsKey(currentPredicate))
                                    requirementsDictionary.Add(currentPredicate, "Multiple");

                            coincidences = itemRequirements.Where(req => req != entry && req.TaxonomyPredicate.ToString().Split('#').Last().Contains(predicateNumber));
                            if (coincidences.Count() > 0)
                            {
                                var elements = coincidences.Where(req => req.TaxonomyPredicate.ToString().Split('#').Last() != currentPredicate);
                                if (elements.Count() == 1)
                                    if (!requirementsDictionary.ContainsKey(currentPredicate))
                                        requirementsDictionary.Add(currentPredicate, "Datatype");
                            }
                            else
                            {
                                if (!requirementsDictionary.ContainsKey(currentPredicate))
                                    requirementsDictionary.Add(currentPredicate, "Object");
                            }
                        }

                        foreach (var entry in itemRequirements)
                        {
                            if (allRequirementsFulfilled)
                            {
                                var objectRequirementNameList = new List<string>();
                                var objectRequirementNameDictionary = new Dictionary<string, bool>();
                                datatypeRequirementName = entry.TaxonomyPredicate.ToString().Split('#').Last();
                                requirementsChecked.Add(datatypeRequirementName);
                                var isDatatype = this.CheckDatatypeProperty($"{game.Context}{datatypeRequirementName}", false);
                                if (isDatatype)
                                {
                                    var requirementNumber = datatypeRequirementName.Split('_').ToList().LastOrDefault();
                                    var itemObjectRequirements = itemFactAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains(datatypeRequirementName) == false);
                                    if (itemObjectRequirements.Count() > 0)
                                    {
                                        itemObjectRequirements = itemObjectRequirements.Where(entry => entry.ToString().Contains("_" + requirementNumber));
                                        if (itemObjectRequirements.Count() > 0)
                                        {
                                            foreach (var requirementEntry in itemObjectRequirements)
                                            {
                                                var predicateString = requirementEntry.TaxonomyPredicate.ToString();
                                                var requirementShortName = requirementEntry.TaxonomyObject.ToString();
                                                objectRequirementNameList.Add(requirementShortName);
                                                requirementsChecked.Add(predicateString);
                                            }
                                        }
                                    }
                                }
                                else
                                    objectRequirementNameList.Add(entry.TaxonomyObject.ToString());

                                foreach (var name in objectRequirementNameList)
                                {
                                    var elementName = name.Split('#').Last();
                                    var elementString = $"{character.Context}{elementName}";
                                    RDFOntologyFact elementFact = null;
                                    RDFOntologyDatatypeProperty elementProperty = null;
                                    if (this.CheckIndividual(elementString))
                                        elementFact = this.Ontology.Data.SelectFact(elementString);
                                    else
                                    {
                                        var elementStringEntries = this.Ontology.Model.PropertyModel.Where(property => property.ToString().Contains(elementName));
                                        if (elementStringEntries.Count() > 0)
                                        {
                                            if (elementStringEntries.Count() > 1)
                                                elementString = elementStringEntries.Where(property => property.ToString().Contains("Total")).Single().ToString();
                                            else
                                                elementString = elementStringEntries.Single().ToString();
                                            elementProperty = this.Ontology.Model.PropertyModel.SelectProperty(elementString) as RDFOntologyDatatypeProperty;
                                        }
                                        else
                                            allRequirementsFulfilled = false;
                                    }

                                    if (allRequirementsFulfilled == true)
                                    {
                                        var characterFact = this.Ontology.Data.SelectFact($"{character.Context}{FileService.EscapedName(this.Name)}");
                                        var characterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(characterFact);
                                        RDFOntologyTaxonomy characterRequirementAssertions = null;
                                        if (isDatatype)
                                        {
                                            characterRequirementAssertions = characterAssertions.SelectEntriesByPredicate(elementProperty);
                                            if (characterRequirementAssertions.Count() > 0)
                                            {
                                                var requirementAssertion = characterRequirementAssertions.Single();
                                                var requirementValue = Convert.ToSingle(entry.TaxonomyObject.ToString().Split('^').First());
                                                var characterValue = Convert.ToSingle(requirementAssertion.TaxonomyObject.ToString().Split('^').First());
                                                var result = ConvertToOperator("<", characterValue, requirementValue);
                                                if (!objectRequirementNameDictionary.ContainsKey(name))
                                                    objectRequirementNameDictionary.Add(name, result != true);
                                            }
                                        }
                                        else
                                        {
                                            requirementPropertyDictionary.TryGetValue(elementName, out var requirementProperty);
                                            requirementsDictionary.TryGetValue(requirementProperty, out var requirementType);

                                            if (requirementType == "Multiple")
                                            {
                                                var requirementNumber = requirementProperty.Split('#').Last().Split('_').Last();
                                                var valueRequirementEntries = itemRequirements.Where(assertion => assertion.TaxonomyPredicate.ToString().Split('#').Last().Contains(requirementNumber) && assertion.TaxonomyPredicate.ToString().Split('#').Last() != requirementProperty);
                                                if (valueRequirementEntries.Count() > 0)
                                                {
                                                    var requirementCheckValue = Convert.ToDouble(valueRequirementEntries.Single().TaxonomyObject.ToString().Split('^').First());
                                                    var requirementEntries = itemRequirements.Where(assertion => assertion.TaxonomyPredicate.ToString().Split('#').Last() == requirementProperty);
                                                    var similarRequirementsDictionary = new Dictionary<string, bool>();
                                                    foreach (var assertion in requirementEntries)
                                                    {
                                                        var currentObjectString = assertion.TaxonomyObject.ToString();
                                                        var currentObjectName = currentObjectString.Split('#').Last();

                                                        if (character.CheckIndividual(currentObjectString, false))
                                                        {
                                                            var currentObject = game.Ontology.Data.SelectFact(currentObjectString);
                                                            var characterCurrentRequirementAssertions = characterAssertions.SelectEntriesByObject(currentObject);
                                                            if (characterCurrentRequirementAssertions.Count() > 0)
                                                            {
                                                                if (!similarRequirementsDictionary.ContainsKey(elementName))
                                                                    similarRequirementsDictionary.Add(elementName, true);
                                                            }
                                                            else
                                                                if (!similarRequirementsDictionary.ContainsKey(elementName))
                                                                similarRequirementsDictionary.Add(elementName, false);
                                                        }
                                                        else
                                                        {
                                                            var characterCurrentRequirementAssertions = characterAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains(currentObjectName));
                                                            if (characterCurrentRequirementAssertions.Count() > 0)
                                                            {
                                                                characterCurrentRequirementAssertions = characterAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains("Total"));
                                                                if (characterCurrentRequirementAssertions.Count() > 0)
                                                                {
                                                                    var currentRequirementValue = Convert.ToDouble(characterCurrentRequirementAssertions.Single().TaxonomyObject.ToString().Split('^').First());
                                                                    var result = ConvertToOperator("<", currentRequirementValue, requirementCheckValue);
                                                                    if (result == false)
                                                                    {
                                                                        if (!similarRequirementsDictionary.ContainsKey(elementName))
                                                                            similarRequirementsDictionary.Add(elementName, true);
                                                                    }
                                                                    else
                                                                        if (!similarRequirementsDictionary.ContainsKey(elementName))
                                                                        similarRequirementsDictionary.Add(elementName, false);
                                                                }
                                                            }
                                                            else
                                                                if (!similarRequirementsDictionary.ContainsKey(elementName))
                                                                similarRequirementsDictionary.Add(elementName, false);
                                                        }
                                                    }
                                                    if (!similarRequirementsDictionary.Values.Any(check => check == true))
                                                        allRequirementsFulfilled = false;
                                                }
                                            }
                                            else if (requirementType == "Object")
                                            {
                                                characterRequirementAssertions = characterAssertions.SelectEntriesByObject(elementFact);
                                                if (characterRequirementAssertions.Any())
                                                {
                                                    if (!objectRequirementNameDictionary.ContainsKey(elementName))
                                                        objectRequirementNameDictionary.Add(elementName, true);

                                                }
                                                else
                                                    if (!objectRequirementNameDictionary.ContainsKey(elementName))
                                                    objectRequirementNameDictionary.Add(elementName, false);
                                            }
                                        }
                                    }
                                }
                                if (objectRequirementNameDictionary.Values.Count() > 0)
                                    if (objectRequirementNameDictionary.Values.All(value => value == false))
                                        allRequirementsFulfilled = false;
                            }
                            else
                                break;
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
                                        if (string.IsNullOrEmpty(this.GetFullString(itemCostName)))
                                            itemCostWords.Remove(itemCostWords.Last());
                                    }

                                    var requirementCostName = string.Join('_', itemCostWords);
                                    var requirementCostString = GetFullString(requirementCostName, true);
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
            };

            var availableOptionsSet = new HashSet<Item>();
            foreach (var item in availableOptions)
            {
                var itemName = item.FullName;
                if (!availableOptionsSet.Any(option => option.FullName == itemName))
                    availableOptionsSet.Add(item);
            }
            availableOptions = availableOptionsSet.ToList();
            return availableOptions;
        }
    }
}
