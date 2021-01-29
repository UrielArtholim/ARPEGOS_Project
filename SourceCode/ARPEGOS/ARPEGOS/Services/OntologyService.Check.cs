
namespace ARPEGOS.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using RDFSharp.Model;
    using RDFSharp.Semantics.OWL;

    public abstract partial class OntologyService
    {
        /// <summary>
        /// Returns true if an element belongs to Equipment given its class
        /// </summary>
        /// <param name="elementClassName"></param>
        /// <returns></returns>
        public bool CheckEquipmentClass (string elementClassString)
        {
            var equipmentWords = new List<string> { "Equipamiento", "Equipment", "Équipement" };
            var elementClass = this.Ontology.Model.ClassModel.SelectClass(elementClassString);
            var elementClassTypeEntry = this.Ontology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(elementClass).Single();
            if (elementClassTypeEntry != null)
            {
                // performance doesn't change drastically from lastindexof + substring, and with split is more readable
                var elementSuperClassString = elementClassTypeEntry.TaxonomyObject.ToString();
                var elementSuperClassName = elementSuperClassString.Split('#').Last();
                return equipmentWords.Any(word => elementSuperClassName.Contains(word)) || this.CheckEquipmentClass(elementSuperClassString);
            }

            return false;
        }                
    }
}
