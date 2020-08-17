
namespace ARPEGOS.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RDFSharp.Semantics;

    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Returns a list of elements available given the current stage, true if the stage has a general limit, the general limit and the partial limit
        /// </summary>
        /// <param name="stage">Name of the stage</param>
        /// <param name="hasGeneralLimitValue">Boolean which tells if the stage has general limit</param>
        /// <param name="GeneralLimitValue">Value of the general limit</param>
        /// <param name="PartialLimitValue">Value of the partial limit</param>
        /// <returns></returns>
        public List<string> CheckAvailableOptions(string stage, bool hasGeneralLimitValue, float? generalLimitValue, float? partialLimitValue)
        {
            var costWords = new List<string> { "Coste", "Cost", "Coût" };
            var requirementWords = new List<string> {"Requisito", "Requirement", "Requisite", "Prérequis" };
            var availableOptions = new List<string>();
            var stageGroup = new Group(stage);
            foreach (var item in stageGroup.GroupList)
            {
                if (hasGeneralLimitValue == false || generalLimitValue > 0)
                {
                    if (partialLimitValue > 0)
                    {
                        var itemFact = this.Game.Ontology.Data.SelectFact($"{this.Game.Context}{item.Name}");
                        var itemFactAssertions = this.Game.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(itemFact);

                        //Check it has requisites
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
                                        var predicate = requirementEntry.TaxonomyPredicate.ToString().Substring(requirementEntry.TaxonomyPredicate.ToString().LastIndexOf('#') + 1);
                                        var name = requirementEntry.TaxonomyObject.ToString().Substring(requirementEntry.TaxonomyObject.ToString().LastIndexOf('#') + 1);
                                        objectRequirementNameList.Add(name);
                                        requirementsChecked.Add(predicate);
                                    }
                                }
                                else
                                    objectRequirementNameList.Add(entry.TaxonomyObject.ToString().Split('#').Last());

                                foreach (var name in objectRequirementNameList)
                                {
                                    var objectFactName = name;
                                    RDFOntologyFact objectFact = null;
                                    if (!this.CheckIndividual(objectFactName))
                                    {
                                        allRequirementsFulfilled = false;
                                        continue;
                                    }
                                    else
                                        objectFact = this.Ontology.Data.SelectFact($"{this.Context}{objectFactName}");

                                    var characterFact = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");
                                    var characterAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(characterFact);
                                    var characterRequirementAssertions = characterAssertions.SelectEntriesByObject(objectFact);
                                    if (characterRequirementAssertions.Any())
                                    {
                                        if (isDatatype)
                                        {
                                            var requirementAssertion = characterAssertions.SingleOrDefault(entry => entry.TaxonomyPredicate.ToString().Contains(objectFactName));
                                            var requirementValue = Convert.ToSingle(entry.TaxonomyObject.ToString().Substring(0, entry.TaxonomyObject.ToString().IndexOf('^')));
                                            var characterValue = Convert.ToSingle(requirementAssertion.TaxonomyObject.ToString().Split('#').First());
                                            var result = ConvertToOperator("<", characterValue, requirementValue);
                                            objectRequirementNameDictionary.Add(name, result != true);
                                        }
                                    }
                                    else
                                    {
                                        var requirementAssertions = characterAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains(objectFactName));
                                        if (requirementAssertions.Any())
                                        {
                                            if (requirementAssertions.Count() > 1)
                                                requirementAssertions = requirementAssertions.Where(entry => entry.TaxonomyPredicate.ToString().Contains("Total"));

                                            if (isDatatype)
                                            {
                                                var requirementAssertion = requirementAssertions.SingleOrDefault();
                                                var RequirementValue = Convert.ToSingle(entry.TaxonomyObject.ToString().Substring(0, entry.TaxonomyObject.ToString().IndexOf('^')));
                                                var CharacterValue = Convert.ToSingle(requirementAssertion.TaxonomyObject.ToString().Split('#').First());
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
                            //Check it has costs
                            var itemCosts = itemFactAssertions.Where(entry => costWords.Any(word => entry.ToString().Contains(word)));
                            RDFOntologyTaxonomyEntry itemCostEntry = null;

                            if (itemCosts.Count() > 1)
                            {
                                var generalCostPredicateName = this.CheckGeneralCost(stage);
                                itemCosts = itemCosts.Where(entry => entry.TaxonomyPredicate.ToString().Contains(generalCostPredicateName));
                                itemCostEntry = itemCosts.SingleOrDefault();
                            }
                            else if (itemCosts.Count() == 1)
                                itemCostEntry = itemCosts.SingleOrDefault();

                            if (itemCostEntry != null)
                            {
                                var costValue = Convert.ToSingle(itemCostEntry.TaxonomyObject.ToString().Split('#').First());
                                var itemCostEntryPredicate = itemCostEntry.TaxonomyPredicate.ToString().Split('#').Last();
                                var generalLimitName = this.GetLimitByValue(stage, generalLimitValue.ToString());
                                var firstWord = generalLimitName.Split('_').ToList().FirstOrDefault() + "_";
                                generalLimitName = generalLimitName.Replace("Per_", "");

                                var costMatchGeneralLimit = CheckCostAndLimit(itemCostEntryPredicate, generalLimitName);
                                if (costMatchGeneralLimit == false)
                                {
                                    var partialLimitName = this.GetLimitByValue(stage, partialLimitValue.ToString());
                                    var costMatchPartialLimit = CheckCostAndLimit(itemCostEntryPredicate, partialLimitName);
                                    if (itemCostEntryPredicate != partialLimitName)
                                    {
                                        // Buscar límite K
                                        var itemCostWords = itemCostEntryPredicate.Split('_').ToList();
                                        var characterClass = new List<string> { "Personaje", "Character", "Personnage" };
                                        var characterDatatatypePropertyFound = false;
                                        var index = 0;
                                        itemCostWords.Remove(itemCostWords.FirstOrDefault());

                                        while (!characterDatatatypePropertyFound)
                                        {
                                            var property = this.Ontology.Model.PropertyModel.ElementAtOrDefault(index);
                                            if (this.CheckDatatypeProperty(property.ToString().Split('#').Last()))
                                            {
                                                var datatypeProperty = property as RDFOntologyDatatypeProperty;
                                                if (datatypeProperty.Domain != null)
                                                {
                                                    var domainClass = datatypeProperty.Domain.ToString().Split('#').Last();
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
                                            itemCostWords.Remove(itemCostWords.LastOrDefault());
                                            itemCostWords.Add("Total");
                                        }

                                        var requirementCostName = string.Join('_', itemCostWords);
                                        var characterFact = this.Ontology.Data.SelectFact($"{this.Context}{FileService.EscapedName(this.Name)}");

                                        if (!(this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{requirementCostName}") is RDFOntologyDatatypeProperty requirementCostProperty))
                                        {
                                            IEnumerable<RDFOntologyTaxonomyEntry> requirementCostAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(characterFact);
                                            requirementCostAssertions = requirementCostAssertions.Where(entry => itemCostWords.All(word => entry.TaxonomyPredicate.ToString().Contains(word)));
                                            requirementCostName = requirementCostAssertions.SingleOrDefault().TaxonomyPredicate.ToString().Split('#').Last();
                                            requirementCostProperty = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{requirementCostName}") as RDFOntologyDatatypeProperty;
                                        }

                                        var entry = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(characterFact).SelectEntriesByPredicate(requirementCostProperty).SingleOrDefault();
                                        var entryValue = Convert.ToSingle(entry.TaxonomyObject.ToString().Substring(0, entry.TaxonomyObject.ToString().IndexOf('^')));
                                        if (entryValue >= costValue)
                                            availableOptions.Add(item.Name);
                                    }
                                    else
                                    {
                                        if (generalLimitValue >= costValue && partialLimitValue >= costValue) 
                                            availableOptions.Add(item.Name);
                                    }
                                }
                                else
                                {
                                    if (generalLimitValue >= costValue)
                                        availableOptions.Add(item.Name);
                                }                              
                            }
                        }                        
                    }
                }
            }

            return availableOptions;
        }
    }
}
