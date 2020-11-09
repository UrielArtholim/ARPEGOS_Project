
namespace ARPEGOS.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using RDFSharp.Semantics.OWL;

    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Removes datatype property assertions from the character given the predicate. Is optional to give a specific value.
        /// </summary>
        /// <param name="predicateName">Name of the predicate</param>
        /// <param name="literal">Value of the property</param>
        internal void RemoveDatatypeProperty(string predicateString, string literalString = null)
        {
            
            var predicate = this.Ontology.Model.PropertyModel.SelectProperty(predicateString) as RDFOntologyDatatypeProperty;
            var CharacterPredicateAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate);
            if(literalString == null)
            {
                foreach(var entry in CharacterPredicateAssertions)
                {
                    var entrySubject = entry.TaxonomySubject as RDFOntologyFact;
                    var entryLiteral = entry.TaxonomyObject as RDFOntologyLiteral;
                    this.Ontology.Data.RemoveAssertionRelation(entrySubject, predicate, entryLiteral);
                }
            }
            else
            {
                var entryLiteral = this.Ontology.Data.SelectLiteral(literalString);
                var entries = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SelectEntriesByObject(entryLiteral);
                foreach(var entry in entries)
                {
                    var entrySubject = entry.TaxonomySubject as RDFOntologyFact;
                    this.Ontology.Data.RemoveAssertionRelation(entrySubject, predicate, entryLiteral);
                }
            }
            this.Save();
        }

        /// <summary>
        /// Removes object property assertions from the character given the predicate. Is optional to give a specific object.
        /// </summary>
        /// <param name="predicateName"></param>
        /// <param name="objectFactName"></param>
        internal void RemoveObjectProperty(string predicateString, string objectFactString = null)
        {
            
            var predicate = this.Ontology.Model.PropertyModel.SelectProperty(predicateString) as RDFOntologyObjectProperty;
            var CharacterPredicateAssertions = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate);
            if (objectFactString == null)
            {
                foreach (var entry in CharacterPredicateAssertions)
                {
                    var entrySubject = entry.TaxonomySubject as RDFOntologyFact;
                    var entryObject = entry.TaxonomyObject as RDFOntologyFact;
                    this.Ontology.Data.RemoveAssertionRelation(entrySubject, predicate, entryObject);
                }
            }
            else
            {
                var entryObject = this.Ontology.Data.SelectFact(objectFactString);
                var entry = this.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SelectEntriesByObject(entryObject).SingleOrDefault();
                var entrySubject = entry.TaxonomySubject as RDFOntologyFact;
                this.Ontology.Data.RemoveAssertionRelation(entrySubject, predicate, entryObject);
            }
            this.Save();
        }
    }
}
