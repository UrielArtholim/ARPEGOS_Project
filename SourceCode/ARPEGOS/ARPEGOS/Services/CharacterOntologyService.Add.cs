
namespace ARPEGOS.Services
{
    using System.Linq;

    using RDFSharp.Semantics;

    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Asserts the object property between subject and object inside the character
        /// </summary>
        /// <param name="subjectFact">Fact that serves as subject of the assertion</param>
        /// <param name="predicate">Object property that serves as the predicate of the assertion</param>
        /// <param name="objectFact">Fact that serves as object of the assertion</param>
        public void AddObjectProperty(string subjectFullName, string predicateFullName, string objectFullName)
        {
            var characterDataModel = this.Ontology.Data;
            var subjectName = subjectFullName.Split('#').Last();
            var predicateName = predicateFullName.Split('#').Last();
            var objectName = objectFullName.Split('#').Last();
            var subjectFact = !this.CheckIndividual(subjectName) ? this.CreateIndividual(subjectName) : characterDataModel.SelectFact(subjectFullName);
            var predicate = !this.CheckObjectProperty(predicateName) ? this.CreateObjectProperty(predicateName) : this.Ontology.Model.PropertyModel.SelectProperty(predicateFullName);
            var objectFact = !this.CheckIndividual(objectName) ? this.CreateIndividual(objectName) : characterDataModel.SelectFact(objectFullName);
            characterDataModel.AddAssertionRelation(subjectFact, predicate as RDFOntologyObjectProperty, objectFact);
            this.Save();
        }

        /// <summary>
        /// Asserts the datatype property between subject and object inside the character
        /// </summary>
        /// <param name="subjectFact">Fact that serves as subject of the assertion</param>
        /// <param name="predicate">Datatype property that serves as the predicate of the assertion</param>
        /// <param name="objectFact">Literal that serves as object of the assertion</param>
        public void AddDatatypeProperty(string subjectFullName, string predicateFullName, string value, string valuetype)
        {
            var characterDataModel = this.Ontology.Data;
            var subjectName = subjectFullName.Split('#').Last();
            var predicateName = predicateFullName.Split('#').Last();
            var subjectFact = !this.CheckIndividual(subjectName) ? this.CreateIndividual(subjectName) : characterDataModel.SelectFact(subjectFullName);
            var predicate = !this.CheckDatatypeProperty(predicateName) ? this.CreateDatatypeProperty(predicateName) : this.Ontology.Model.PropertyModel.SelectProperty(predicateFullName);
            var objectLiteral = this.CreateLiteral(value, valuetype);
            characterDataModel.AddAssertionRelation(subjectFact, predicate as RDFOntologyDatatypeProperty, objectLiteral);
            this.Save();
        }
    }
}
