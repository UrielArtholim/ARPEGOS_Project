
namespace ARPEGOS.Services
{
    using RDFSharp.Semantics.OWL;

    public partial class CharacterOntologyService : OntologyService
    {
        public CharacterOntologyService (string name, string path, string context, RDFOntology ontology) : base(name, path, context, ontology) { }

        public void Save()
        {
            var graph = this.Ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData);
            graph.ToFile(RDFFormat, this.Path);
        }
    }
}
