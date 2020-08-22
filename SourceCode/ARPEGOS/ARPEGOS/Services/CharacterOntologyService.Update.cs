
namespace ARPEGOS.Services
{
    using System.Collections.Generic;
    using System.Linq;

    using RDFSharp.Semantics.OWL;

    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Update available points limit given the element name
        /// </summary>
        /// <param name="stage">Element which contains the limit</param>
        /// <param name="update">Updated value of the limit</param>
        internal bool UpdateAvailablePoints(string stage, float? update)
        {
            var hasUpdated = false;
            var AvailableWords = new List<string>()
            {
                "Disponible",
                "Available"
            };
            var GamePropertyModel = this.Game.Ontology.Model.PropertyModel;
            var ResultProperties = GamePropertyModel.Where(entry => AvailableWords.Any(word => entry.ToString().Contains(word)));
            var ElementWords = stage.Split('_').ToList();
            var CompareList = new List<string>();
            var index = 0;
            var FilterResultsCounter = ResultProperties.Count();
            while (FilterResultsCounter > 1)
            {
                CompareList.Add(ElementWords.ElementAtOrDefault(index));
                ResultProperties = ResultProperties.Where(entry => CompareList.All(word => entry.ToString().Contains(word)));
                FilterResultsCounter = ResultProperties.Count();
                ++index;
            }

            if (FilterResultsCounter > 0)
            {
                var LimitProperty = ResultProperties.SingleOrDefault();
                var propertyName = LimitProperty.ToString().Split('#').Last();
                UpdateDatatypeAssertion(propertyName, update.ToString());
            }
            else
            {
                var parents = GetParentClasses(stage);
                if(parents != null)
                {
                    var parentList = parents.Split(':').ToList();
                    foreach (string parent in parentList)
                        if (hasUpdated == false)
                            hasUpdated = UpdateAvailablePoints(parent, update);
                }                
            }
            this.Save();
            return hasUpdated;
        }

        /// <summary>
        /// Updates a datatype assertion in character given the predicate and the new value.
        /// </summary>
        /// <param name="predicateName">Name of the predicate</param>
        /// <param name="value">New value of the assertion</param>
        internal void UpdateDatatypeAssertion(string predicateName, string value)
        {
            var hasPredicate = CheckDatatypeProperty(predicateName);
            RDFOntologyDatatypeProperty predicate;
            if (hasPredicate == false)
                predicate = CreateDatatypeProperty(predicateName);
            else
                predicate = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{predicateName}") as RDFOntologyDatatypeProperty;

            string subject, valuetype;

            if (hasPredicate == true)
            {
                var CharacterPredicateAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SingleOrDefault();
                if(CharacterPredicateAssertions != null)
                {
                    valuetype = CharacterPredicateAssertions.TaxonomyObject.ToString().Split('#').Last();
                    subject = CharacterPredicateAssertions.TaxonomySubject.ToString();
                    RemoveDatatypeProperty(predicateName);
                    this.Save();
                }
                else
                {
                    subject = $"{this.Context}{this.Name}";
                    valuetype = predicate.Range.Value.ToString().Split('#').Last();
                }
            }
            else
            {
                subject = $"{this.Context}{this.Name}";
                valuetype = predicate.Range.Value.ToString().Split('#').Last();
            }
            AddDatatypeProperty(subject, $"{this.Context}{predicateName}", value, valuetype);
            this.Save();
        }

        /// <summary>
        /// Updates an object assertion in character given the predicate and the new object.
        /// </summary>
        /// <param name="predicateName">>Name of the predicate</param>
        /// <param name="objectName">New object of the assertion</param>
        internal void UpdateObjectAssertion(string predicateName, string objectName)
        {
            var hasPredicate = CheckObjectProperty(predicateName);
            RDFOntologyObjectProperty predicate;
            if (hasPredicate == false)
                predicate = CreateObjectProperty(predicateName);
            else
                predicate = this.Ontology.Model.PropertyModel.SelectProperty($"{this.Context}{predicateName}") as RDFOntologyObjectProperty;
            string subject;

            if (hasPredicate == true)
            {
                var CharacterPredicateAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SingleOrDefault();
                if(CharacterPredicateAssertions != null)
                {
                    subject = CharacterPredicateAssertions.TaxonomySubject.ToString();
                    RemoveDatatypeProperty(predicateName);
                    this.Save();
                }
                else
                    subject = $"{this.Context}{this.Name}";
            }
            else
                subject = $"{this.Context}{this.Name}";
            AddObjectProperty(subject, $"{this.Context}{predicateName}", $"{this.Context}{objectName}");
            this.Save();
        }

        internal bool UpdateLimit(string ElementName, float? update)
        {
            var hasUpdated = false;
            var LimitWords = new List<string>()
            {
                "Límite",
                "Limit",
                "Limite"
            };
            var GamePropertyModel = this.Game.Ontology.Model.PropertyModel;
            var ResultProperties = GamePropertyModel.Where(entry => LimitWords.Any(word => entry.ToString().Contains(word)));
            var ElementWords = ElementName.Split('_').ToList();
            var CompareList = new List<string>();
            var index = 0;
            var FilterResultsCounter = ResultProperties.Count();

            while (FilterResultsCounter > 1)
            {
                CompareList.Add(ElementWords.ElementAtOrDefault(index));
                ResultProperties = ResultProperties.Where(entry => CompareList.All(word => entry.ToString().Contains(word)));
                FilterResultsCounter = ResultProperties.Count();
                ++index;
            }

            if (FilterResultsCounter > 0)
            {
                var LimitProperty = ResultProperties.SingleOrDefault() as RDFOntologyDatatypeProperty;
                var propertyName = LimitProperty.ToString().Split('#').Last();
                UpdateDatatypeAssertion(propertyName, update.ToString());
                hasUpdated = true;
            }
            else
            {
                var parents = GetParentClasses(ElementName);
                if (parents != null)
                {
                    var parentList = parents.Split(':').ToList();
                    foreach (string parent in parentList)
                    {
                        if (hasUpdated == false)
                            hasUpdated = UpdateLimit(parent, update);
                    }
                }
            }
            this.Save();
            return hasUpdated;
        }
    }
}
