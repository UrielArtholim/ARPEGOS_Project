using ARPEGOS.Helpers;
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
        public string GetCreationSchemeRootClass()
        {
            string rootClassName = string.Empty;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            var ontology_class_annotations = game.Ontology.Model.ClassModel.Annotations.CustomAnnotations;
            var entries = ontology_class_annotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("CreationSchemeRoot"));
            if (entries.Count() > 0)
                rootClassName = entries.Single().TaxonomySubject.ToString().Split('#').Last();
            return rootClassName;
        }
    }
}
