
namespace ARPEGOS.Services
{
    using RDFSharp.Semantics;

    public class GameOntologyService : OntologyService
    {
        public GameOntologyService(string name, string path, string context, RDFOntology ontology) 
            : base(name, path, context, ontology) { }
    }
}
