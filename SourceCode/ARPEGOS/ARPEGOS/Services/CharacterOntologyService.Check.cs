
namespace ARPEGOS.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
                CurrentOntology = this.Game.Ontology;

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
        public bool CheckFact (string name, bool applyOnCharacter = true)
        {
            RDFOntology CurrentOntology;
            if (applyOnCharacter)
                CurrentOntology = this.Ontology;
            else
                CurrentOntology = this.Game.Ontology;

            var escapedName = FileService.EscapedName(name);
            var dataModel = CurrentOntology.Data;
            return dataModel.SelectFact($"{this.Context}{escapedName}") != null;
        }

        /// <summary>
        /// Check if class exists inside the current ontology
        /// </summary>
        /// <param name="name">Name of the class</param>
        /// <returns></returns>
        public bool CheckClass (string name, bool applyOnCharacter = true)
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
                CurrentOntology = this.Game.Ontology;
                CurrentContext = this.Game.Context;
            }

            var escapedName = FileService.EscapedName(name);
            var classModel = CurrentOntology.Model.ClassModel;
            return classModel.SelectClass($"{CurrentContext}{escapedName}") != null;
        }

        /// <summary>
        /// Check if object property exists inside the current ontology
        /// </summary>
        /// <param name="name">Name of the object property</param>
        /// <returns></returns>
        public bool CheckObjectProperty (string name, bool applyOnCharacter = true)
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
                CurrentOntology = this.Game.Ontology;
                CurrentContext = this.Game.Context;
            }

            var escapedName = FileService.EscapedName(name);
            var exists = false;
            var propertyModel = CurrentOntology.Model.PropertyModel;
            var property = propertyModel.SelectProperty($"{CurrentContext}{escapedName}");

            if (property != null)
            {
                var objectEnumerator = propertyModel.ObjectPropertiesEnumerator;
                while (objectEnumerator.MoveNext())
                {
                    // performance doesn't change drastically from lastindexof + substring, and with split is more readable
                    if (objectEnumerator.Current?.ToString().Split('#').Last() == escapedName)
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
        public bool CheckDatatypeProperty (string name, bool applyOnCharacter = true)
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
                CurrentOntology = this.Game.Ontology;
                CurrentContext = this.Game.Context;
            }

            var escapedName = FileService.EscapedName(name);
            var exists = false;
            var propertyModel = CurrentOntology.Model.PropertyModel;
            var property = propertyModel.SelectProperty($"{CurrentContext}{escapedName}");

            if (property != null)
            {
                var datatypeEnumerator = propertyModel.DatatypePropertiesEnumerator;
                while (datatypeEnumerator.MoveNext())
                {
                    // performance doesn't change drastically from lastindexof + substring, and with split is more readable
                    if (datatypeEnumerator.Current?.ToString().Split('#').Last() == escapedName)
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
        public bool CheckIndividual (string name, bool applyOnCharacter = true)
        {
            return this.CheckFact(name, applyOnCharacter);
        }

        /// <summary>
        /// Returns the general cost associated to the given stage
        /// </summary>
        /// <param name="stage">Name of the stage</param>
        /// <returns></returns>
        public string CheckGeneralCost (string stage, bool applyOnCharacter = false)
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
                CurrentOntology = this.Game.Ontology;
                CurrentContext = this.Game.Context;
            }

            var generalCostPredicateName = string.Empty;
            var stageWords = stage.Split('_');
            var filterCounter = 2;
            var wordCounter = stageWords.Length;

            while (filterCounter > 1)
            {
                if (wordCounter > 0)
                {
                    var subjectFactName = string.Join('_', stageWords.Take(wordCounter));
                    var subjectFact = CurrentOntology.Model.ClassModel.SelectClass($"{CurrentContext}{subjectFactName}");
                    if (subjectFact != null)
                    {
                        var subjectFactCostAnnotations = CurrentOntology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(subjectFact).Where(entry => entry.ToString().Contains("GeneralCostDefinedBy"));
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
                var parents = this.GetParentClasses(stage);
                if (parents != null)
                {
                    foreach (var parent in parents.Split(':'))
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
        public string CheckValueListInfo (string stage, bool applyOnCharacter = false)
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
                CurrentOntology = this.Game.Ontology;
                CurrentContext = this.Game.Context;
            }

            string info = null;

            var stageClass = CurrentOntology.Model.ClassModel.SelectClass($"{CurrentContext}{stage}");
            var stageAnnotations = CurrentOntology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(stageClass);
            var stageDefinitionAnnotation = stageAnnotations.Single(entry => entry.TaxonomyPredicate.ToString().Contains("ValuedListInfo"));
            if (stageDefinitionAnnotation != null)
                info = stageDefinitionAnnotation.TaxonomyObject.ToString().Split('^').First();
            else
            {
                var parents = this.GetParentClasses(stage, applyOnCharacter);
                if (parents != null)
                {
                    var parentList = parents.Split(':').ToList();
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
                                        var predicate = requirementEntry.TaxonomyPredicate.ToString().Split('#').Last();
                                        var name = requirementEntry.TaxonomyObject.ToString().Split('#').Last();
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
                                            var requirementValue = Convert.ToSingle(entry.TaxonomyObject.ToString().Split('^').First());
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
                                        var entryValue = Convert.ToSingle(entry.TaxonomyObject.ToString().Split('^').First());
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
