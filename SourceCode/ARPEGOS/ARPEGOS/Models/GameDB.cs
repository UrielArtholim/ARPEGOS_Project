namespace ARPEGOS.Models
{
    using ARPEGOS.Models;
    using RDFSharp.Model;
    using RDFSharp.Query;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class GameDB
    {
        #region Properties
        static readonly RDFModelEnums.RDFFormats RdfFormat = RDFModelEnums.RDFFormats.RdfXml;
        public RDFGraph GameGraph { get; internal set; }
        public List<RDFTriple> TripleList { get; internal set; }
        public string GameFile { get; internal set; }
        #endregion

        #region Constructors

        public GameDB(string game, string title)
        {
            GameFile = Path.Combine(SystemControl.DirectoryHelper.GetBaseDirectory(), game, "gamefiles", title);
            GameGraph = RDFGraph.FromFile(RdfFormat, GameFile);
            GameGraph.SetContext(new Uri(GameGraph.Context.ToString() + "#"));
        }
        #endregion

        #region Methods

        public void SaveData()
        {
            foreach (var statement in TripleList)
                GameGraph.AddTriple(statement);

            Console.WriteLine("Saving data into file " +GameFile);
            GameGraph.ToFile(RdfFormat,GameFile);
            Console.WriteLine("Data saved");
            Console.WriteLine("----------------------------\n");
        }

        public void AddObjectTriple(string subjectName, string objectProperty, string objectName)
        {
            string subjectUri = GameGraph.Context + subjectName;
            string objectPropertyUri = GameGraph.Context + objectProperty;
            string objectUri = GameGraph.Context + objectName;
            RDFResource subjectResource = new RDFResource(subjectUri);
            RDFResource objectPropertyResource = new RDFResource(objectPropertyUri);
            RDFResource objectResource = new RDFResource(objectUri);
            RDFTriple currentTriple = new RDFTriple(subjectResource, objectPropertyResource, objectResource);
            if(!GameGraph.ContainsTriple(currentTriple))
                TripleList.Add(currentTriple);
        }
        
        public List<string> GetIndividualsOfClass(string className)
        {
            RDFSelectQuery selectQuery = new RDFSelectQuery();
            RDFVariable x = new RDFVariable("x");
            var classResource = new RDFResource(GameGraph.Context + className);
            var rdfType = new RDFResource(RDFVocabulary.RDF.BASE_URI + "type");
            var x_fromClass = new RDFPattern(x, rdfType, classResource);
            var selectPatternGroup = new RDFPatternGroup("selectPatternGroup");
            selectPatternGroup.AddPattern(x_fromClass);
            selectQuery.AddPatternGroup(selectPatternGroup);
            
            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            Console.WriteLine("Query result count: " + selectResult.SelectResultsCount);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            List<string> resultList = new List<string>();
            
            foreach(DataRow row in rows)
                foreach(var item in row.ItemArray)
                {
                    resultList.Add(item.ToString());
                }

            Console.ReadLine();
            return (resultList.Count() != 0)? resultList : null;
        }

        public List<string> GetSubclassesOf(string className)
        {
            
            RDFSelectQuery selectQuery = new RDFSelectQuery();
            RDFVariable x = new RDFVariable("x");

            var classResource = new RDFResource(GameGraph.Context + className);
            var rdfsSubClassOf = new RDFResource(RDFVocabulary.RDFS.BASE_URI + "subClassOf");
            var x_subClassOf = new RDFPattern(x, rdfsSubClassOf, classResource);
            var selectPatternGroup = new RDFPatternGroup("selectPatternGroup");
            selectPatternGroup.AddPattern(x_subClassOf);
            selectQuery.AddPatternGroup(selectPatternGroup);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            Console.WriteLine("Query result count: " + selectResult.SelectResultsCount);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            List<string> resultList = new List<string>();

            foreach (DataRow row in rows)
                foreach (var item in row.ItemArray)
                {
                    Console.WriteLine(item);
                    string itemString = item.ToString();
                    resultList.Add(itemString);
                }

            Console.ReadLine();
            return (resultList.Count() != 0) ? resultList : null;
        }

        public List<string> getObjectPropertyOfElement(string elementName)
        {
            RDFSelectQuery selectQuery = new RDFSelectQuery();
            RDFVariable x = new RDFVariable("x");
            RDFRegexFilter regexFilter = new RDFRegexFilter(x, new Regex("tiene" + elementName));

            // Triple: x rdf:type owl:ObjectProperty
            var rdfType = new RDFResource(RDFVocabulary.RDF.BASE_URI + "type");
            var x_fromClass = new RDFPattern(x, rdfType, RDFVocabulary.OWL.OBJECT_PROPERTY);
            var selectPatternGroup = new RDFPatternGroup("selectPatternGroup");

            selectPatternGroup.AddPattern(x_fromClass);
            selectPatternGroup.AddFilter(regexFilter);
            selectQuery.AddPatternGroup(selectPatternGroup);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            Console.WriteLine("Query result count: " + selectResult.SelectResultsCount);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            List<string> resultList = new List<string>();

            foreach (DataRow row in rows)
                foreach (var item in row.ItemArray)
                {
                    resultList.Add(item.ToString());
                }

            Console.ReadLine();
            return (resultList.Count() != 0) ? resultList : null;
        }

        public List<string> getDatatypePropertyOfElement(string elementName)
        {
            RDFSelectQuery selectQuery = new RDFSelectQuery();
            RDFVariable x = new RDFVariable("x");
            RDFRegexFilter regexFilter = new RDFRegexFilter(x, new Regex("Per_" + elementName));

            // Triple: x rdf:type owl:ObjectProperty
            var rdfType = new RDFResource(RDFVocabulary.RDF.BASE_URI + "type");
            var x_fromClass = new RDFPattern(x, rdfType, RDFVocabulary.OWL.DATATYPE_PROPERTY);
            var selectPatternGroup = new RDFPatternGroup("selectPatternGroup");

            // Triple: x contains elementName in its name

            selectPatternGroup.AddPattern(x_fromClass);
            selectPatternGroup.AddFilter(regexFilter);
            selectQuery.AddPatternGroup(selectPatternGroup);

            RDFSelectQueryResult selectResult = selectQuery.ApplyToGraph(GameGraph);
            Console.WriteLine("Query result count: " + selectResult.SelectResultsCount);
            DataRowCollection rows = selectResult.SelectResults.Rows;
            List<string> resultList = new List<string>();

            foreach (DataRow row in rows)
                foreach (var item in row.ItemArray)
                {
                    resultList.Add(item.ToString());
                }

            Console.ReadLine();
            return (resultList.Count() != 0) ? resultList : null;
        }

        #endregion
    }
}
