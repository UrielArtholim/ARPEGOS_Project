
namespace ARPEGOS.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using ARPEGOS.Configuration;
    using ARPEGOS.Helpers;
    using RDFSharp.Model;
    using RDFSharp.Semantics.OWL;

    public abstract partial class OntologyService
    {
        public static async Task<GameOntologyService> LoadGame(string name, string version)
        {
            if(DependencyHelper.CurrentContext.CurrentGame != null)
            {
                var currentGamePath = DependencyHelper.CurrentContext.CurrentGame.Path;
                if (currentGamePath.Contains(FileService.EscapedName(version)))
                    return DependencyHelper.CurrentContext.CurrentGame;
            }
            name = FileService.FormatName(name);
            var path = FileService.GetGameFilePath(name, version);
            var graph =  await Task.Run(()=> RDFGraph.FromFile(RDFFormat, path));
            var context = $"{graph.Context}#";
            graph.SetContext(new Uri(context));
            //var prefix = string.Concat(Regex.Matches(FileService.EscapedName(name), "[A-Z]").Select(match => match.Value)).ToLower();
            //RDFNamespaceRegister.AddNamespace(new RDFNamespace(prefix, graph.Context.ToString()));
            var ontology = RDFOntology.FromRDFGraph(graph);

            return new GameOntologyService(name, path, context, ontology);
        }

        public static async Task<CharacterOntologyService> LoadCharacter(string name, GameOntologyService game)
        {
            if (DependencyHelper.CurrentContext.CurrentCharacter != null)
            {
                var currentCharacterPath = DependencyHelper.CurrentContext.CurrentCharacter.Path;
                if (currentCharacterPath.Contains(FileService.EscapedName(name)))
                    return DependencyHelper.CurrentContext.CurrentCharacter;
            }

            var path = FileService.GetCharacterFilePath(name, game);
            var context = $"http://arpegos_project/Games/{FileService.EscapedName(game.Name)}/characters/{FileService.EscapedName(name)}#";
            var graph = await Task.Run(() => RDFGraph.FromFile(RDFFormat, path));
            graph.SetContext(new Uri(context));
            var ontology = RDFOntology.FromRDFGraph(graph);

            return new CharacterOntologyService(name, path, context, ontology);
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
            //var prefix = string.Concat(Regex.Matches(FileService.EscapedName(name), "[A-Z]").Select(match => match.Value)).ToLower();
            //RDFNamespaceRegister.AddNamespace(new RDFNamespace(prefix, graph.Context.ToString()));
            RDFOntology ontology = null;
            await Task.Run(() => ontology = RDFOntology.FromRDFGraph(graph));
            var character = new CharacterOntologyService(name, path, context, ontology);
            character.Save();
            return character;            
        }

        public static async Task<bool> DeleteCharacter(string characterName, GameOntologyService currentGame)
        {
            return await FileService.DeleteCharacter(characterName, currentGame.Name);
        }
    }
}
