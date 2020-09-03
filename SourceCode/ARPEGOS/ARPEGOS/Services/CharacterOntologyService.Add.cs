﻿
namespace ARPEGOS.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using RDFSharp.Model;
    using RDFSharp.Semantics.OWL;

    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Asserts the object property between subject and object inside the character
        /// </summary>
        /// <param name="subjectFact">Fact that serves as subject of the assertion</param>
        /// <param name="predicate">Object property that serves as the predicate of the assertion</param>
        /// <param name="objectFact">Fact that serves as object of the assertion</param>
        public void AddObjectProperty (string subjectFullName, string predicateFullName, string objectFullName)
        {
            var characterDataModel = this.Ontology.Data;
            var subjectName = subjectFullName.Split('#').Last();
            var predicateName = predicateFullName.Split('#').Last();
            var objectName = objectFullName.Split('#').Last();

            var subjectCharacterString = $"{this.Context}{subjectName}";
            var predicateCharacterString = $"{this.Context}{predicateName}";
            var objectCharacterString = $"{this.Context}{objectName}";

            var subjectFact = !this.CheckIndividual(subjectCharacterString) ? this.CreateIndividual(subjectName) : characterDataModel.SelectFact(subjectCharacterString);
            var predicate = !this.CheckObjectProperty(predicateCharacterString) ? this.CreateObjectProperty(predicateName) : this.Ontology.Model.PropertyModel.SelectProperty(predicateCharacterString);
            var objectFact = !this.CheckIndividual(objectCharacterString) ? this.CreateIndividual(objectName) : characterDataModel.SelectFact(objectCharacterString);
            characterDataModel.AddAssertionRelation(subjectFact, predicate as RDFOntologyObjectProperty, objectFact);
            this.Save();
        }

        /// <summary>
        /// Asserts the datatype property between subject and object inside the character
        /// </summary>
        /// <param name="subjectFact">Fact that serves as subject of the assertion</param>
        /// <param name="predicate">Datatype property that serves as the predicate of the assertion</param>
        /// <param name="objectFact">Literal that serves as object of the assertion</param>
        public void AddDatatypeProperty (string subjectFullName, string predicateFullName, string value, string valuetype)
        {
            var characterDataModel = this.Ontology.Data;
            var subjectName = subjectFullName.Split('#').Last();
            var predicateName = predicateFullName.Split('#').Last();

            var subjectCharacterString = $"{this.Context}{subjectName}";
            var predicateCharacterString = $"{this.Context}{predicateName}";

            var subjectFact = !this.CheckIndividual(subjectCharacterString) ? this.CreateIndividual(subjectName) : characterDataModel.SelectFact(subjectCharacterString);
            var predicate = !this.CheckDatatypeProperty(predicateCharacterString) ? this.CreateDatatypeProperty(predicateName) : this.Ontology.Model.PropertyModel.SelectProperty(predicateCharacterString);
            var objectLiteral = this.CreateLiteral(value, valuetype);
            characterDataModel.AddAssertionRelation(subjectFact, predicate as RDFOntologyDatatypeProperty, objectLiteral);
            this.Save();
        }

        /// <summary>
        /// Asserts an annotation property to classify a character property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        internal void AddClassification (string propertyString)
        {
            var text = string.Empty;
            var descriptionType = "string";
            var hierarchy = new List<string>();
            var currentProperty = this.Ontology.Model.PropertyModel.SelectProperty(propertyString);
            var propertyParents = this.Ontology.Model.PropertyModel.GetSuperPropertiesOf(currentProperty).ToList();
            propertyParents.Reverse();

            foreach (var parent in propertyParents)
            {
                var parentString = parent.ToString();
                var parentName = parent.ToString().Split('#').Last();
                var groupName = parentName.Replace("tiene", "").Replace("Personaje", "").Replace("Per", "").Replace("_", " ");
                groupName = System.Text.RegularExpressions.Regex.Replace(groupName, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
                hierarchy.Add(groupName);
            }

            hierarchy.Remove(hierarchy.First());
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
            var AnnotationType = "Visualization";
            var annotation = this.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.Where(annotation => annotation.TaxonomyPredicate.ToString().Contains("Visualization")).First().TaxonomyPredicate as RDFOntologyAnnotationProperty;
            if (annotation == null)
                annotation = new RDFOntologyAnnotationProperty(new RDFResource($"{this.Context}{AnnotationType}"));
            this.Ontology.Model.PropertyModel.AddCustomAnnotation(annotation, currentProperty, description);

            //Comprobar que la propiedad es de una habilidad
            var activeSkillName = "Activa";
            var propertyParent = this.Ontology.Model.PropertyModel.Relations.SubPropertyOf.SelectEntriesBySubject(currentProperty).Single().TaxonomyObject;
            var propertyParentName = propertyParent.ToString().Split('#').Last();
            while (propertyParent != null && propertyParentName.Contains(activeSkillName))
            {
                var propertyParentTaxonomy = this.Ontology.Model.PropertyModel.Relations.SubPropertyOf.SelectEntriesBySubject(propertyParent);
                if (propertyParentTaxonomy.EntriesCount > 0)
                {
                    propertyParent = propertyParentTaxonomy.Single().TaxonomyObject;
                    propertyParentName = propertyParent.ToString().Split('#').Last();
                }

            }

            if (propertyParent != null)
            {
                if(this.CheckDatatypeProperty(propertyString) == true)
                {
                    var propertyShortName = propertyString.Split('#').Last();
                    if(!propertyShortName.Contains("Total"))
                        return;
                }

                description = CreateLiteral("true", "boolean");
                AnnotationType = "SkillProperty";
                annotation = this.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("SkillProperty")).First().TaxonomyPredicate as RDFOntologyAnnotationProperty;
                if (annotation == null)
                {
                    annotation = new RDFOntologyAnnotationProperty(new RDFResource($"{this.Context}{AnnotationType}"));
                    this.Ontology.Model.PropertyModel.AddProperty(annotation);
                }
                this.Ontology.Model.PropertyModel.AddCustomAnnotation(annotation as RDFOntologyAnnotationProperty, currentProperty, description);
            }

            this.Save();
        }
    }
}
