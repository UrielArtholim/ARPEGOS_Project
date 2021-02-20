using ARPEGOS.Services;
using NUnit.Framework;
using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARPEGOS_Unit_Test.Tests
{
    [TestFixture]
    public partial class CharacterOntologyServiceTests
    {
        [Test]
        [TestCase("tieneCategoría", "Categoría Asesino", ExpectedResult = true)]
        public bool UpdateObjectAssertion_Test(string predicateName, string newObjectName)
        {
            var hasUpdated = false;
            var predicateString = Character.GetFullString(FileService.EscapedName(predicateName), true);
            var characterFact = Character.Ontology.Data.SelectFact($"{Character.Context}{FileService.EscapedName(Character.Name)}");
            var predicateProperty = Character.Ontology.Model.PropertyModel.SelectProperty(predicateString);
            var predicateAssertionEntries = Character.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicateProperty);
            var previousObjectName = string.Empty;
            RDFOntologyTaxonomyEntry predicateAssertion;
            if (predicateAssertionEntries.EntriesCount > 1)
                Character.RemoveObjectProperty(predicateString);
            else
                previousObjectName = predicateAssertionEntries.Single().TaxonomyObject.ToString().Split('^').First().Split('#').Last();
            Character.UpdateObjectAssertion(FileService.EscapedName(predicateName) , FileService.EscapedName(newObjectName));
            predicateAssertionEntries = Character.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicateProperty);
            if (predicateAssertionEntries.EntriesCount > 1)
                predicateAssertion = predicateAssertionEntries.Where(entry => entry.TaxonomyObject.ToString().Contains(FileService.EscapedName(newObjectName))).Single();
            else
                predicateAssertion = predicateAssertionEntries.Single();
            var currentObjectName = predicateAssertion.TaxonomyObject.ToString().Split('^').First().Split('#').Last();
            hasUpdated = !string.Equals(previousObjectName , currentObjectName);
            Character.UpdateObjectAssertion(FileService.EscapedName(predicateName) , previousObjectName);
            return hasUpdated;
        }

        [Test]
        [TestCase("Per Nivel","5", ExpectedResult = true)]
        public bool UpdateDatatypeAssertion_Test (string predicateName , string newValue)
        {
            var hasUpdated = false;
            var predicateString = Character.GetFullString(FileService.EscapedName(predicateName) , true);
            var characterFact = Character.Ontology.Data.SelectFact($"{Character.Context}{FileService.EscapedName(Character.Name)}");
            var predicateProperty = Character.Ontology.Model.PropertyModel.SelectProperty(predicateString);
            var predicateAssertionEntries = Character.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicateProperty);
            var previousValue = string.Empty;
            RDFOntologyTaxonomyEntry predicateAssertion;
            if (predicateAssertionEntries.EntriesCount > 1)
                Character.RemoveDatatypeProperty(predicateString);
            else
                previousValue = predicateAssertionEntries.Single().TaxonomyObject.ToString().Split('^').First();
            Character.UpdateDatatypeAssertion(FileService.EscapedName(predicateName) , newValue);
            predicateAssertionEntries = Character.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(predicateProperty);
            if (predicateAssertionEntries.EntriesCount > 1)
                predicateAssertion = predicateAssertionEntries.Where(entry => entry.TaxonomyObject.ToString().Contains(newValue)).Single();
            else
                predicateAssertion = predicateAssertionEntries.Single();
            var currentValue = predicateAssertion.TaxonomyObject.ToString().Split('^').First();
            hasUpdated = !string.Equals(previousValue , currentValue);
            Character.UpdateDatatypeAssertion(FileService.EscapedName(predicateName) , previousValue);
            return hasUpdated;
        }
    }
}
