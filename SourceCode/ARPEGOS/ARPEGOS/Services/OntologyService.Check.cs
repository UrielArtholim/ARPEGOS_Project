
namespace ARPEGOS.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using RDFSharp.Model;
    using RDFSharp.Semantics;

    public partial class OntologyService
    {
        /// <summary>
        /// Check if literal exists inside the current ontology
        /// </summary>
        /// <param name="value">Value of the literal</param>
        /// <param name="type">Semantic datatype of the literal</param>
        /// <returns></returns>
        public bool CheckLiteral(string value, string type)
        {
            var dataModel = this.Ontology.Data;
            var literalType = CheckDatatypeFromString(type);
            var literal = new RDFTypedLiteral(value, literalType);
            return dataModel.SelectLiteral(literal.ToString()) != null;
        }

        /// <summary>
        /// Check if fact exists inside the current ontology
        /// </summary>
        /// <param name="name">Name of the fact</param>
        /// <returns></returns>
        public bool CheckFact(string name)
        {
            var escapedName = FileService.EscapedName(name);
            var dataModel = this.Ontology.Data;
            return dataModel.SelectFact($"{this.Context}{escapedName}") != null;
        }

        /// <summary>
        /// Check if class exists inside the current ontology
        /// </summary>
        /// <param name="name">Name of the class</param>
        /// <returns></returns>
        public bool CheckClass(string name)
        {
            var escapedName = FileService.EscapedName(name);
            var classModel = this.Ontology.Model.ClassModel;
            return classModel.SelectClass($"{this.Context}{escapedName}") != null;
        }

        /// <summary>
        /// Check if object property exists inside the current ontology
        /// </summary>
        /// <param name="name">Name of the object property</param>
        /// <returns></returns>
        public bool CheckObjectProperty(string name)
        {
            var escapedName = FileService.EscapedName(name);
            var exists = false;
            var propertyModel = this.Ontology.Model.PropertyModel;
            var property = propertyModel.SelectProperty($"{this.Context}{escapedName}");

            if (property != null)
            {
                var objectEnumerator = propertyModel.ObjectPropertiesEnumerator;
                while (objectEnumerator.MoveNext())
                {
                    // performance doesn't change drastically from lastindexof + substring, and with split is more readable
                    if (objectEnumerator.Current?.ToString().Split('#').Last() == escapedName)
                    {
                        exists = true;
                        break;
                    }
                }
            }
            
            return exists;
        }

        /// <summary>
        /// Check if datatype property exists inside the current ontology
        /// </summary>
        /// <param name="name">Name of the datatype property</param>
        /// <returns></returns>
        public bool CheckDatatypeProperty(string name)
        {
            var escapedName = FileService.EscapedName(name);
            var exists = false;
            var propertyModel = this.Ontology.Model.PropertyModel;
            var property = propertyModel.SelectProperty($"{this.Context}{escapedName}");

            if (property != null)
            {
                var datatypeEnumerator = propertyModel.DatatypePropertiesEnumerator;
                while (datatypeEnumerator.MoveNext())
                {
                    // performance doesn't change drastically from lastindexof + substring, and with split is more readable
                    if (datatypeEnumerator.Current?.ToString().Split('#').Last() == escapedName)
                    {
                        exists = true;
                        break;
                    }
                }
            }
            
            return exists;
        }

        /// <summary>
        /// Check if individual exists inside the current ontology
        /// </summary>
        /// <param name="name">Name of the individual</param>
        /// <returns></returns>
        public bool CheckIndividual(string name)
        {
            return this.CheckFact(name);
        }

        /// <summary>
        /// Returns the general cost associated to the given stage
        /// </summary>
        /// <param name="stage">Name of the stage</param>
        /// <returns></returns>
        public string CheckGeneralCost(string stage)
        {
            var generalCostPredicateName = string.Empty;
            var stageWords = stage.Split('_');
            var filterCounter = 2;
            var wordCounter = stageWords.Length;

            while (filterCounter > 1)
            {
                if (wordCounter > 0)
                {
                    var subjectFactName = string.Join('_', stageWords.Take(wordCounter));
                    var subjectFact = this.Ontology.Model.ClassModel.SelectClass($"{this.Context}{subjectFactName}");
                    if (subjectFact != null)
                    {
                        var subjectFactCostAnnotations = this.Ontology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(subjectFact).Where(entry => entry.ToString().Contains("GeneralCostDefinedBy"));
                        if (subjectFactCostAnnotations.Count() == 1)
                        {
                            filterCounter = 1;
                            generalCostPredicateName = subjectFactCostAnnotations.SingleOrDefault().TaxonomyObject.ToString().Split('^').First();
                        }
                    }
                    --wordCounter;
                }
                else
                    filterCounter = 0;
            }

            if (filterCounter != 1)
            {
                var parents = this.GetParentClasses(stage);
                if (parents != null)
                {
                    foreach (var parent in parents.Split(':'))
                    {
                        generalCostPredicateName = this.CheckGeneralCost(parent);
                        if (generalCostPredicateName != null)
                            break;
                    }
                }
            }

            return generalCostPredicateName;
        }

        /// <summary>
        /// Returns true if an element belongs to Equipment given its class
        /// </summary>
        /// <param name="elementClassName"></param>
        /// <returns></returns>
        public bool CheckEquipmentClass (string elementClassName)
        {
            var equipmentWords = new List<string> { "Equipamiento", "Equipment", "Équipement" };
            var elementClass = this.Ontology.Model.ClassModel.SelectClass($"{this.Context}{elementClassName}");
            var elementClassTypeEntry = this.Ontology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(elementClass).SingleOrDefault();
            if (elementClassTypeEntry != null)
            {
                // performance doesn't change drastically from lastindexof + substring, and with split is more readable
                var elementSuperClass = elementClassTypeEntry.TaxonomyObject.ToString().Split('#').Last();
                return equipmentWords.Any(word => elementSuperClass.Contains(word)) || this.CheckEquipmentClass(elementSuperClass);
            }

            return false;
        }

        /// <summary>
        /// Returns the description of a valued list view given the stage
        /// </summary>
        /// <param name="stage">Name of the stage stage</param>
        /// <returns></returns>
        public string CheckValueListInfo(string stage, bool applyOnCharacter = false)
        {
            string info = null;

            var stageDefinitionIndividual = this.Ontology.Data.SelectFact($"{this.Context}{stage}");
            var stageAnnotations = this.Ontology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(stageDefinitionIndividual);
            var stageDefinitionAnnotation = stageAnnotations.SingleOrDefault(entry => entry.TaxonomyPredicate.ToString().Contains("ValuedListInfo"));
            if (stageDefinitionAnnotation != null)
            {
                info = stageDefinitionAnnotation.TaxonomyObject.ToString().Split('^').First();
            }
            else
            {
                var parents = this.GetParentClasses(stage);
                if (parents != null)
                {
                    var parentList = parents.Split(':').ToList();
                    foreach (var parent in parentList)
                    {
                        info = this.CheckValueListInfo(parent);
                        if (info != null)
                            break;
                    }
                }
            }

            return info;
        }
    }
}
