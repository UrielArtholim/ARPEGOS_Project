
namespace ARPEGOS.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ARPEGOS.Helpers;
    using RDFSharp.Semantics.OWL;

    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Update available points limit given the element name
        /// </summary>
        /// <param name="stage">Element which contains the limit</param>
        /// <param name="update">Updated value of the limit</param>
        internal bool UpdateAvailablePoints(string stageString, float? update)
        {
            
            var stageName = stageString.Split('#').Last();
            var hasUpdated = false;
            var AvailableWords = new List<string>()
            {
                "Disponible",
                "Available"
            };
            var GamePropertyModel = DependencyHelper.CurrentContext.CurrentGame.Ontology.Model.PropertyModel;
            var ResultProperties = GamePropertyModel.Where(entry => AvailableWords.Any(word => entry.ToString().Contains(word)));
            var ElementWords = stageName.Split('_').ToList();
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
                var parents = GetParentClasses(stageString);
                if(parents != null)
                {
                    var parentList = parents.Split('|').ToList();
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
        public void UpdateDatatypeAssertion(string predicateString, string value)
        {
            
            string predicateName = predicateString.Split('#').Last();
            var hasPredicate = CheckDatatypeProperty(predicateString);
            RDFOntologyDatatypeProperty predicate;

            string subjectString, valuetype;

            if (hasPredicate == true)
            {
                predicate = this.Ontology.Model.PropertyModel.SelectProperty(predicateString) as RDFOntologyDatatypeProperty;
                var CharacterPredicateAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SingleOrDefault();
                if(CharacterPredicateAssertions != null)
                {
                    valuetype = CharacterPredicateAssertions.TaxonomyObject.ToString().Split('#').Last();
                    subjectString = CharacterPredicateAssertions.TaxonomySubject.ToString();
                    RemoveDatatypeProperty(predicateString);
                    this.Save();
                }
                else
                {
                    subjectString = $"{this.Context}{FileService.EscapedName(this.Name)}";
                    valuetype = predicate.Range.Value.ToString().Split('#').Last();
                }
            }
            else
            {
                predicate = CreateDatatypeProperty(predicateName);
                subjectString = $"{this.Context}{FileService.EscapedName(this.Name)}";
                valuetype = predicate.Range.Value.ToString().Split('#').Last();
            }
            AddDatatypeProperty(subjectString, predicateString, value, valuetype);            
            this.Save();
        }

        /// <summary>
        /// Updates an object assertion in character given the predicate and the new object.
        /// </summary>
        /// <param name="predicateName">>Name of the predicate</param>
        /// <param name="objectName">New object of the assertion</param>
        public void UpdateObjectAssertion(string predicateString, string objectString)
        {
            
            var predicateName = predicateString.Split('#').Last();
            var hasPredicate = CheckObjectProperty(predicateString);
            RDFOntologyObjectProperty predicate;
            string subject;

            if (hasPredicate == true)
            {
                predicate = this.Ontology.Model.PropertyModel.SelectProperty(predicateString) as RDFOntologyObjectProperty;
                var CharacterPredicateAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SingleOrDefault();
                if(CharacterPredicateAssertions != null)
                {
                    subject = CharacterPredicateAssertions.TaxonomySubject.ToString();
                    RemoveObjectProperty(predicateString);
                    this.Save();
                }
                else
                {
                    predicate = CreateObjectProperty(predicateName);
                    subject = $"{this.Context}{FileService.EscapedName(this.Name)}";
                }
            }
            else
            {
                predicate = CreateObjectProperty(predicateName);
                subject = $"{this.Context}{FileService.EscapedName(this.Name)}";
            }
            AddObjectProperty(subject, predicateString, objectString);
            this.Save();
        }

        internal bool UpdateLimit(string elementString, float? update)
        {
            
            var elementName = elementString.Split('#').Last();
            var hasUpdated = false;
            var LimitWords = new List<string>()
            {
                "Límite",
                "Limit",
                "Limite"
            };
            var GamePropertyModel = DependencyHelper.CurrentContext.CurrentGame.Ontology.Model.PropertyModel;
            var ResultProperties = GamePropertyModel.Where(entry => LimitWords.Any(word => entry.ToString().Contains(word)));
            var ElementWords = elementName.Split('_').ToList();
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
                var LimitProperty = ResultProperties.Single() as RDFOntologyDatatypeProperty;
                var propertyString = LimitProperty.ToString();
                UpdateDatatypeAssertion(propertyString, update.ToString());
                hasUpdated = true;
            }
            else
            {
                var parents = GetParentClasses(elementString);
                if (parents != null)
                {
                    var parentList = parents.Split('|').ToList();
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
