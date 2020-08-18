
namespace ARPEGOS.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using RDFSharp.Model;
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

        /// <summary>
        /// Asserts an annotation property to classify a character property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        internal void AddClassification (string propertyName)
        {
            var text = string.Empty;
            var descriptionType = "string";
            var hierarchy = new List<string>();
            var currentProperty = this.Ontology.Model.PropertyModel.SelectProperty(this.Context + propertyName);
            var propertyParents = this.Ontology.Model.PropertyModel.GetSuperPropertiesOf(currentProperty).ToList();
            propertyParents.Reverse();
            foreach (var parent in propertyParents)
            {
                var parentName = parent.ToString().Substring(parent.ToString().LastIndexOf('#') + 1);
                var groupName = parentName.Replace("tiene", "").Replace("Personaje", "").Replace("Per", "").Replace("_", " ");
                groupName = System.Text.RegularExpressions.Regex.Replace(groupName, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
                hierarchy.Add(groupName);
            }
            hierarchy.Remove(hierarchy.FirstOrDefault());
            if (hierarchy.Count > 1)
            {
                for (int i = 0; i < hierarchy.Count; ++i)
                {
                    var words = hierarchy[i].Split(" ").ToList();
                    if (words.Contains(""))
                        words.Remove("");

                    for (int j = i + 1; j < hierarchy.Count; ++j)
                    {
                        if (words.Any(word => hierarchy[j].Contains(word)))
                        {
                            var temp = hierarchy[j].Split(" ").ToList();
                            var newTemp = new List<string>();
                            for (int k = 0; k < temp.Count; ++k)
                            {
                                if (words.All(word => temp[k] != word))
                                    newTemp.Add(temp[k]);
                            }
                            hierarchy[j] = string.Join(" ", newTemp);
                        }
                    }
                }
            }
            text = string.Join(":", hierarchy).Replace(" ", "");
            RDFOntologyLiteral description = CreateLiteral(text, descriptionType);
            string AnnotationType = "Visualization";
            RDFOntologyProperty annotation = this.Ontology.Model.PropertyModel.SelectProperty(this.Context + AnnotationType);
            if (annotation == null)
                this.Ontology.Model.PropertyModel.AddProperty(new RDFOntologyAnnotationProperty(new RDFResource(this.Context + AnnotationType)));
            this.Ontology.Model.PropertyModel.AddCustomAnnotation(annotation as RDFOntologyAnnotationProperty, currentProperty, description);
            this.Save();
        }
    }
}
