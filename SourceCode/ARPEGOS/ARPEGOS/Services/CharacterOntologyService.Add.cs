
namespace ARPEGOS.Services
{
    using RDFSharp.Semantics;

    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Asserts the object property between subject and object inside the character
        /// </summary>
        /// <param name="subjectFact">Fact that serves as subject of the assertion</param>
        /// <param name="predicate">Object property that serves as the predicate of the assertion</param>
        /// <param name="objectFact">Fact that serves as object of the assertion</param>
        internal void AddObjectProperty(string subjectFullName, string predicateFullName, string objectFullName)
        {
            RDFOntologyData CharacterDataModel = CharacterOntology.Data;
            RDFOntologyFact subjectFact, objectFact;
            RDFOntologyProperty predicate;

            string subjectName = subjectFullName.ToString().Substring(subjectFullName.ToString().LastIndexOf('#') + 1);
            string predicateName = predicateFullName.ToString().Substring(predicateFullName.ToString().LastIndexOf('#') + 1);
            string objectName = objectFullName.ToString().Substring(objectFullName.ToString().LastIndexOf('#') + 1);
            if (!CheckIndividual(subjectName))
                subjectFact = CreateIndividual(subjectName);
            else
                subjectFact = CharacterDataModel.SelectFact(subjectFullName);

            if (!CheckObjectProperty(predicateName))
                predicate = CreateObjectProperty(predicateName);
            else
                predicate = CharacterOntology.Model.PropertyModel.SelectProperty(predicateFullName);

            if (!CheckIndividual(objectName))
                objectFact = CreateIndividual(objectName);
            else
                objectFact = CharacterDataModel.SelectFact(objectFullName);

            CharacterDataModel.AddAssertionRelation(subjectFact, predicate as RDFOntologyObjectProperty, objectFact);
            this.Save();
        }

        /// <summary>
        /// Asserts the datatype property between subject and object inside the character
        /// </summary>
        /// <param name="subjectFact">Fact that serves as subject of the assertion</param>
        /// <param name="predicate">Datatype property that serves as the predicate of the assertion</param>
        /// <param name="objectFact">Literal that serves as object of the assertion</param>
        internal void AddDatatypeProperty(string subjectFullName, string predicateFullName, string value, string valuetype)
        {
            RDFOntologyData CharacterDataModel = CharacterOntology.Data;
            RDFOntologyFact subjectFact;
            RDFOntologyProperty predicate;
            RDFOntologyLiteral objectLiteral;
            string subjectName = subjectFullName.Substring(subjectFullName.LastIndexOf('#') + 1);
            string predicateName = predicateFullName.Substring(predicateFullName.LastIndexOf('#') + 1);

            if (!CheckIndividual(subjectName))
                subjectFact = CreateIndividual(subjectName);
            else
                subjectFact = CharacterDataModel.SelectFact(subjectFullName);

            if (!CheckDatatypeProperty(predicateName))
                predicate = CreateDatatypeProperty(predicateName);
            else
                predicate = CharacterOntology.Model.PropertyModel.SelectProperty(predicateFullName);

            objectLiteral = CreateLiteral(value, valuetype);

            CharacterDataModel.AddAssertionRelation(subjectFact, predicate as RDFOntologyDatatypeProperty, objectLiteral);
            this.Save();
        }
    }
}
