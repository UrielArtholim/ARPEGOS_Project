
namespace ARPEGOS.Services
{
    using System.Collections.Generic;
    using System.Linq;

    using RDFSharp.Semantics;

    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Update available points limit given the element name
        /// </summary>
        /// <param name="stage">Element which contains the limit</param>
        /// <param name="update">Updated value of the limit</param>
        internal bool UpdateAvailablePoints(string stage, float? update)
        {
            bool hasUpdated = false;
            List<string> AvailableWords = new List<string>()
            {
                "Disponible",
                "Available"
            };

            RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;

            IEnumerable<RDFOntologyProperty> ResultProperties = GamePropertyModel.Where(entry => AvailableWords.Any(word => entry.ToString().Contains(word)));
            List<string> ElementWords = stage.Split('_').ToList();
            List<string> CompareList = new List<string>();
            int index = 0;
            int FilterResultsCounter = ResultProperties.Count();

            while (FilterResultsCounter > 1)
            {
                CompareList.Add(ElementWords.ElementAtOrDefault(index));
                ResultProperties = ResultProperties.Where(entry => CompareList.All(word => entry.ToString().Contains(word)));
                FilterResultsCounter = ResultProperties.Count();
                ++index;
            }

            if (FilterResultsCounter > 0)
            {
                RDFOntologyProperty LimitProperty = ResultProperties.SingleOrDefault();
                string propertyName = LimitProperty.ToString().Substring(LimitProperty.ToString().LastIndexOf('#') + 1);
                UpdateDatatypeAssertion(propertyName, update.ToString());
            }
            else
            {
                string parents = GetParentClasses(stage);
                if(parents != null)
                {
                    List<string> parentList = parents.Split(':').ToList();
                    foreach (string parent in parentList)
                    {
                        if (hasUpdated == false)
                            hasUpdated = UpdateAvailablePoints(parent, update);
                    }
                }                
            }
            SaveCharacter();
            return hasUpdated;
        }

        /// <summary>
        /// Updates a datatype assertion in character given the predicate and the new value.
        /// </summary>
        /// <param name="predicateName">Name of the predicate</param>
        /// <param name="value">New value of the assertion</param>
        internal void UpdateDatatypeAssertion(string predicateName, string value)
        {
            bool hasPredicate = CheckDatatypeProperty(predicateName);
            RDFOntologyDatatypeProperty predicate;
            if (hasPredicate == false)
                predicate = CreateDatatypeProperty(predicateName);
            else
                predicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + predicateName) as RDFOntologyDatatypeProperty;

            string subject, valuetype;

            if (hasPredicate == true)
            {
                RDFOntologyTaxonomyEntry CharacterPredicateAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SingleOrDefault();
                if(CharacterPredicateAssertions != null)
                {
                    valuetype = CharacterPredicateAssertions.TaxonomyObject.ToString().Substring(CharacterPredicateAssertions.TaxonomyObject.ToString().LastIndexOf('#') + 1);
                    subject = CharacterPredicateAssertions.TaxonomySubject.ToString();
                    RemoveDatatypeProperty(predicateName);
                    SaveCharacter();
                }
                else
                {
                    subject = CurrentCharacterContext + CurrentCharacterName;
                    valuetype = predicate.Range.Value.ToString().Substring(predicate.Range.Value.ToString().LastIndexOf('#') + 1);
                }
            }
            else
            {
                subject = CurrentCharacterContext + CurrentCharacterName;
                valuetype = predicate.Range.Value.ToString().Substring(predicate.Range.Value.ToString().LastIndexOf('#') + 1);
            }
            AddDatatypeProperty(subject, CurrentCharacterContext + predicateName, value, valuetype);
            SaveCharacter();
        }

        /// <summary>
        /// Updates an object assertion in character given the predicate and the new object.
        /// </summary>
        /// <param name="predicateName">>Name of the predicate</param>
        /// <param name="objectName">New object of the assertion</param>
        internal void UpdateObjectAssertion(string predicateName, string objectName)
        {
            bool hasPredicate = CheckObjectProperty(predicateName);
            RDFOntologyObjectProperty predicate;
            if (hasPredicate == false)
                predicate = CreateObjectProperty(predicateName);
            else
                predicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + predicateName) as RDFOntologyObjectProperty;

            string subject;

            if (hasPredicate == true)
            {
                RDFOntologyTaxonomyEntry CharacterPredicateAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SingleOrDefault();
                if(CharacterPredicateAssertions != null)
                {
                    subject = CharacterPredicateAssertions.TaxonomySubject.ToString();
                    RemoveDatatypeProperty(predicateName);
                    SaveCharacter();
                }
                else
                    subject = CurrentCharacterContext + CurrentCharacterName;
            }
            else
                subject = CurrentCharacterContext + CurrentCharacterName;
            AddObjectProperty(subject, CurrentCharacterContext + predicateName, CurrentCharacterContext + objectName);
            SaveCharacter();
        }

        internal bool UpdateLimit(string ElementName, float? update)
        {
            bool hasUpdated = false;
            List<string> LimitWords = new List<string>()
            {
                "Límite",
                "Limit",
                "Limite"
            };

            RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;

            IEnumerable<RDFOntologyProperty> ResultProperties = GamePropertyModel.Where(entry => LimitWords.Any(word => entry.ToString().Contains(word)));
            List<string> ElementWords = ElementName.Split('_').ToList();
            List<string> CompareList = new List<string>();
            int index = 0;
            int FilterResultsCounter = ResultProperties.Count();

            while (FilterResultsCounter > 1)
            {
                CompareList.Add(ElementWords.ElementAtOrDefault(index));
                ResultProperties = ResultProperties.Where(entry => CompareList.All(word => entry.ToString().Contains(word)));
                FilterResultsCounter = ResultProperties.Count();
                ++index;
            }

            if (FilterResultsCounter > 0)
            {
                RDFOntologyDatatypeProperty LimitProperty = ResultProperties.SingleOrDefault() as RDFOntologyDatatypeProperty;
                string propertyName = LimitProperty.ToString().Substring(LimitProperty.ToString().LastIndexOf('#') + 1);
                UpdateDatatypeAssertion(propertyName, update.ToString());
                hasUpdated = true;
            }
            else
            {
                string parents = GetParentClasses(ElementName);
                if (parents != null)
                {
                    List<string> parentList = parents.Split(':').ToList();
                    foreach (string parent in parentList)
                    {
                        if (hasUpdated == false)
                            hasUpdated = UpdateLimit(parent, update);
                    }
                }
            }
            SaveCharacter();
            return hasUpdated;
        }
    }
}
