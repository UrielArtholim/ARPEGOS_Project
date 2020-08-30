using RDFSharp.Model;
using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARPEGOS.Services
{
    public abstract partial class OntologyService
    {
        /// <summary>
        /// Returns the root class of the creation scheme of the active game
        /// </summary>
        /// <returns></returns>
        public string GetCreationSchemeRootClass ()
        {
            var rootProperty = new RDFOntologyAnnotationProperty(new RDFResource($"{this.Context}{"CreationSchemeRoot"}"));
            var creationSchemeRootUri = this.Ontology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesByPredicate(rootProperty).Single().TaxonomySubject.ToString();
            return creationSchemeRootUri.Split('#').Last();
        }
    }
}
