
namespace ARPEGOS.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using ARPEGOS.Configuration;

    using RDFSharp.Model;
    using RDFSharp.Semantics;

    public abstract partial class OntologyService
    {
        /// <summary>
        /// Gets the name of the ontology
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the file path of the ontology
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets base URI of the ontology
        /// </summary>
        public string Context { get; }

        /// <summary>
        /// Gets the ontology accessor
        /// </summary>
        public RDFOntology Ontology { get; }

        protected static RDFModelEnums.RDFFormats RDFFormat => RDFModelEnums.RDFFormats.RdfXml;

        public OntologyService(string name, string path, string context, RDFOntology ontology)
        {
            this.Name = FileService.FormatName(name);
            this.Path = path;
            this.Context = context;
            this.Ontology = ontology;
        }

        // Ejemplo del naming de los nombres pa que nos entendamos
        // nombre => glenn radars
        // nombre formateado => Glenn Radars
        // nombre escaped => Glenn_Radars
        // file path => .../Glenn_Radars.owl
        // los path van a usar el escapado, aunk los nombres guardados en las ontologias van a ser los formateados, por ser mas userfriendly

        public static async Task<GameOntologyService> LoadGame(string name, string version)
        {
            var path = FileService.GetGameFilePath(name, version);
            var graph = RDFGraph.FromFile(RDFFormat, path);
            var context = $"{graph.Context}#";
            graph.SetContext(new Uri(context));
            var prefix = string.Concat(Regex.Matches(FileService.EscapedName(name), "[A-Z]").Select(match => match.Value)).ToLower();
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(prefix, graph.Context.ToString()));
            var ontology = RDFOntology.FromRDFGraph(graph);

            return new GameOntologyService(name, path, context, ontology);
        }

        public static async Task<CharacterOntologyService> LoadCharacter(string name, GameOntologyService game)
        {
            var path = FileService.GetCharacterFilePath(name, game);
            var context = $"http://arpegos_project/Games/{FileService.EscapedName(game.Name)}/characters/{FileService.EscapedName(name)}#";
            var graph = RDFGraph.FromFile(RDFFormat, path);
            graph.SetContext(new Uri(context));
            var ontology = RDFOntology.FromRDFGraph(graph);

            return new CharacterOntologyService(name, path, context, ontology, game);
        }

        public static async Task<CharacterOntologyService> CreateCharacter(string name, GameOntologyService game)
        {
            var path = FileService.GetCharacterFilePath(name, game);
            if (File.Exists(path))
            {
                throw new ArgumentException($"Character {name} already exists");
            }

            var context = $"http://arpegos_project/Games/{FileService.EscapedName(game.Name)}/characters/{FileService.EscapedName(name)}#";
            var graph = new RDFGraph();
            graph.SetContext(new Uri(context));
            var prefix = string.Concat(Regex.Matches(FileService.EscapedName(name), "[A-Z]").Select(match => match.Value)).ToLower();
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(prefix, graph.Context.ToString()));
            var ontology = RDFOntology.FromRDFGraph(graph);

            var character = new CharacterOntologyService(name, path, context, ontology, game);
            character.Save();
            return character;
        }
    }
}
