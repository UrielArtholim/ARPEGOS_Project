using ARPEGOS.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARPEGOS_Unit_Test.Tests
{
    [TestFixture]
    public partial class CharacterOntologyServiceTests
    {
        #region Add Tests
        [Test]
        [TestCase("Guybrush Threepwood", "tieneCategoría", "Categoría Novel", ExpectedResult = true)]
        [TestCase("Guybrush Threepwood", "tieneArte_Marcial_Básica", "Kempo", ExpectedResult = true)]
        [TestCase("Guybrush Threepwood" , "tieneConjuro" , "Crear Frío" , ExpectedResult = true)]
        [TestCase("Guybrush Threepwood", "tieneRaza", "Humano", ExpectedResult = true)]
        public async Task<bool> AddObjectProperty_Test(string Subject, string Predicate, string Object)
        {
            await Task.Run(() => Character.AddObjectProperty(Subject, Predicate, Object));
            //Character.Save();
            var escapedSubject = FileService.EscapedName(Subject);
            var escapedPredicate = FileService.EscapedName(Predicate);
            var escapedObject = FileService.EscapedName(Object);

            var SubjectFact = Character.Ontology.Data.SelectFact($"{Character.Context}{escapedSubject}");
            var PredicateProperty = Character.Ontology.Model.PropertyModel.SelectProperty($"{Character.Context}{escapedPredicate}");
            var ObjectFact = Character.Ontology.Data.SelectFact($"{Character.Context}{escapedObject}");
            var checkAssertion = false;
            if (SubjectFact != null && PredicateProperty != null && ObjectFact != null)
                checkAssertion = Character.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact)
                                                                                 .SelectEntriesByPredicate(PredicateProperty)
                                                                                 .SelectEntriesByObject(ObjectFact).Count() > 0;
            var annotationPredicate = Character.Ontology.Model.PropertyModel.SelectProperty($"{Character.Context}Visualization");
            var checkClassification = Character.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(PredicateProperty).SelectEntriesByPredicate(annotationPredicate).Single() != null;
            return checkAssertion && checkClassification;
        }

        [Test]
        [TestCase("Guybrush Threepwood", "Per_Nivel", "10", "unsignedInt", ExpectedResult = true)]
        [TestCase("Guybrush Threepwood", "Per_Esquiva_PD", "20", "unsignedInt", ExpectedResult = true)]
        [TestCase("Guybrush Threepwood", "Per_Esquiva_Bono_Categoría", "10", "unsignedInt", ExpectedResult = true)]
        public async Task<bool> AddDatatypeProperty_Test(string Subject, string Predicate, string Value, string ValueType)
        {
            await Task.Run(() => Character.AddDatatypeProperty(Subject, Predicate, Value, ValueType));
            //Character.Save();

            var escapedSubject = FileService.EscapedName(Subject);
            var escapedPredicate = FileService.EscapedName(Predicate);

            var SubjectFact = Character.Ontology.Data.SelectFact($"{Character.Context}{escapedSubject}");
            var PredicateProperty = Character.Ontology.Model.PropertyModel.SelectProperty($"{Character.Context}{escapedPredicate}");
            var checkAssertion = false;
            if (SubjectFact != null && PredicateProperty != null)
            {
                var entry = Character.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(SubjectFact).SelectEntriesByPredicate(PredicateProperty).First();
                var entryLiteral = entry.TaxonomyObject.ToString().Split('^');
                var checkLiteralValue = string.Equals(Value, entryLiteral.First());
                var checkLiteralType = entryLiteral.Last().ToLower().Contains(ValueType.ToLower());
                if (checkLiteralValue && checkLiteralType)
                    checkAssertion = true;
            }

            var annotationPredicate = Character.Ontology.Model.PropertyModel.SelectProperty($"{Character.Context}Visualization");
            var checkClassification = Character.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(PredicateProperty).SelectEntriesByPredicate(annotationPredicate).Single() != null;
            return checkAssertion && checkClassification;
        }

        #endregion
    }
}
