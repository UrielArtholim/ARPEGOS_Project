
namespace ARPEGOS.Services
{
    using RDFSharp.Semantics.OWL;
    using System.Threading.Tasks;
    using Xamarin.Essentials;

    public partial class CharacterOntologyService : OntologyService
    {
        public CharacterOntologyService (string name, string path, string context, RDFOntology ontology) : base(name, path, context, ontology) { }

        public static object SaveLock = new object();
        public void Save()
        {
            lock(SaveLock)
            {
                var graph = this.Ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData);
                MainThread.BeginInvokeOnMainThread(()=> graph.ToFile(RDFFormat, this.Path));
            }            
        }
    }
}
