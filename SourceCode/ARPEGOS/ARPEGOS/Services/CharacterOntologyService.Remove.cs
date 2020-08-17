
namespace ARPEGOS.Services
{
    using RDFSharp.Semantics;

    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Removes datatype property assertions from the character given the predicate. Is optional to give a specific value.
        /// </summary>
        /// <param name="predicateName">Name of the predicate</param>
        /// <param name="literal">Value of the property</param>
        internal void RemoveDatatypeProperty(string predicateName, string literal = null)
        {
            RDFOntologyDatatypeProperty predicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + predicateName) as RDFOntologyDatatypeProperty;
            RDFOntologyTaxonomy CharacterPredicateAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate);
            if(literal == null)
            {
                foreach(RDFOntologyTaxonomyEntry entry in CharacterPredicateAssertions)
                {
                    RDFOntologyFact entrySubject = entry.TaxonomySubject as RDFOntologyFact;
                    RDFOntologyLiteral entryLiteral = entry.TaxonomyObject as RDFOntologyLiteral;
                    CharacterOntology.Data.RemoveAssertionRelation(entrySubject, predicate, entryLiteral);
                }
            }
            else
            {
                RDFOntologyLiteral entryLiteral = CharacterOntology.Data.SelectLiteral(literal);
                RDFOntologyTaxonomy entries = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SelectEntriesByObject(entryLiteral);
                foreach(RDFOntologyTaxonomyEntry entry in entries)
                {
                    RDFOntologyFact entrySubject = entry.TaxonomySubject as RDFOntologyFact;
                    CharacterOntology.Data.RemoveAssertionRelation(entrySubject, predicate, entryLiteral);
                }
            }
            SaveCharacter();
        }

        /// <summary>
        /// Removes object property assertions from the character given the predicate. Is optional to give a specific object.
        /// </summary>
        /// <param name="predicateName"></param>
        /// <param name="objectFactName"></param>
        internal void RemoveObjectProperty(string predicateName, string objectFactName = null)
        {
            RDFOntologyObjectProperty predicate = CharacterOntology.Model.PropertyModel.SelectProperty(CurrentCharacterContext + predicateName) as RDFOntologyObjectProperty;
            RDFOntologyTaxonomy CharacterPredicateAssertions = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate);
            if (objectFactName == null)
            {
                foreach (RDFOntologyTaxonomyEntry entry in CharacterPredicateAssertions)
                {
                    RDFOntologyFact entrySubject = entry.TaxonomySubject as RDFOntologyFact;
                    RDFOntologyFact entryObject = entry.TaxonomyObject as RDFOntologyFact;
                    CharacterOntology.Data.RemoveAssertionRelation(entrySubject, predicate, entryObject);
                }
            }
            else
            {
                RDFOntologyFact entryObject = CharacterOntology.Data.SelectFact(CurrentCharacterContext + objectFactName);
                RDFOntologyTaxonomyEntry entry = CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate).SelectEntriesByObject(entryObject).SingleOrDefault();
                RDFOntologyFact entrySubject = entry.TaxonomySubject as RDFOntologyFact;
                CharacterOntology.Data.RemoveAssertionRelation(entrySubject, predicate, entryObject);
            }
            SaveCharacter();
        }
    }
}
