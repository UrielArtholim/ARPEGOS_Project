
namespace ARPEGOS.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ARPEGOS.Helpers;
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
            AddClassification(predicate.ToString(), objectFact.ToString());
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
            AddClassification(predicate.ToString());
        }

        /// <summary>
        /// Asserts an annotation property to classify a character property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        internal void AddClassification (string propertyString, string objectString = null)
        {
            var game = DependencyHelper.CurrentContext.CurrentGame;
            var currentProperty = this.Ontology.Model.PropertyModel.SelectProperty(propertyString);
            var text = string.Empty;
            var descriptionType = "string";
            var hierarchy = new List<string>();
            var propertyParents = this.Ontology.Model.PropertyModel.GetSuperPropertiesOf(currentProperty).ToList();
            propertyParents.Reverse();

            foreach (var parent in propertyParents)
            {
                var parentString = parent.ToString();
                var parentName = parent.ToString().Split('#').Last();
                var groupName = parentName.Replace("tiene", "").Replace("Personaje", "").Replace("Per_", "").Replace("_", " ");
                groupName = System.Text.RegularExpressions.Regex.Replace(groupName, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
                hierarchy.Add(groupName);
            }

            if(hierarchy.Count > 0)
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
            if (string.IsNullOrEmpty(text))
                text = "General";
            var AnnotationType = "Visualization";
            RDFOntologyLiteral description = CreateLiteral(text, descriptionType);
            var annotationPropertyString = $"{this.Context}{AnnotationType}";
            RDFOntologyAnnotationProperty annotation = this.Ontology.Model.PropertyModel.SelectProperty(annotationPropertyString) as RDFOntologyAnnotationProperty;
            if(annotation == null)
            {
                annotation = new RDFOntologyAnnotationProperty(new RDFResource(annotationPropertyString));
                this.Ontology.Model.PropertyModel.AddProperty(annotation);
            }
            this.Ontology.Model.PropertyModel.AddCustomAnnotation(annotation, currentProperty, description);

            if(objectString != null)
            {
                bool activeSkillAnnotationFound = false;
                var objectFact = this.Ontology.Data.SelectFact(objectString);
                var objectFactCustomAnnotations = this.Ontology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(objectFact);
                if(objectFactCustomAnnotations.Count()> 0)
                    activeSkillAnnotationFound = objectFactCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("ActiveSkill")).Count() > 0 ? true : false;
                if (activeSkillAnnotationFound == false)
                {                    
                    var objectClass = currentProperty.Range.ToString();
                    if (objectClass != null)
                    {
                        var objectClassName = objectClass.Split('#').Last();
                        var gameObjectClassString = $"{game.Context}{objectClassName}";
                        var gameObjectClass = game.Ontology.Model.ClassModel.SelectClass(gameObjectClassString);
                        var gameCustomAnnotations = game.Ontology.Model.ClassModel.Annotations.CustomAnnotations;
                        var objectClassCustomAnnotations = gameCustomAnnotations.SelectEntriesBySubject(gameObjectClass);
                        if (objectClassCustomAnnotations.Count() > 0)
                        {
                            var objectClassActiveSkillAnnotation = objectClassCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("ActiveSkill"));
                            if (objectClassActiveSkillAnnotation.Count() > 0)
                            {
                                annotationPropertyString = $"{this.Context}ActiveSkill";
                                annotation = this.Ontology.Model.PropertyModel.SelectProperty(annotationPropertyString) as RDFOntologyAnnotationProperty;
                                if (annotation == null)
                                {
                                    annotation = new RDFOntologyAnnotationProperty(new RDFResource(annotationPropertyString));
                                    this.Ontology.Model.PropertyModel.AddProperty(annotation);
                                }

                                var annotationValue = new RDFOntologyLiteral(new RDFTypedLiteral("true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN));
                                this.Ontology.Data.AddCustomAnnotation(annotation, objectFact, annotationValue);
                            }
                        }
                    }
                }
            }
            
            this.Save();
        }
    }
}
