
namespace ARPEGOS.Models
{
    using RDFSharp.Semantics;

    public class Game
    {
        /// <summary>
        /// Name of the element
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path of the file
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Base URI of the selected element
        /// </summary>
        public string Context { get; set; }

        /// <summary>
        /// Semantic representation of the current game
        /// </summary>
        public RDFOntology Ontology { get; set; }

    }
}
