
namespace ARPEGOS.Services
{
    using RDFSharp.Semantics;

    public partial class CharacterOntologyService : OntologyService
    {
        public GameOntologyService Game { get; }

        public CharacterOntologyService(string name, string path, string context, RDFOntology ontology, GameOntologyService game)
            : base(name, path, context, ontology)
        {
            this.Game = game;
        }

        public void Save()
        {
            var graph = this.Ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData);
            graph.ToFile(RDFFormat, this.Path);
        }
    }
}
