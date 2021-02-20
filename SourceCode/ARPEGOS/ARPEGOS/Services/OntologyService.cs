
namespace ARPEGOS.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RDFSharp.Model;
    using RDFSharp.Semantics.OWL;

    public abstract partial class OntologyService
    {

        /// <summary>
        /// Gets the name of the ontology
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the formatted name of the ontology
        /// </summary>
        public string FormattedName { get; }

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
            this.Name = name;
            this.FormattedName = FileService.FormatName(name);
            this.Path = path;
            this.Context = context;
            this.Ontology = ontology;
        }
        /// <summary>
        /// Returns the result of "value1" -> "op" -> "value2"
        /// </summary>
        /// <param name="op"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static dynamic ConvertToOperator(string op, double value1, double value2)
        {
            switch (op)
            {
                case "+":
                    return value1 + value2;
                case "-":
                    return value1 - value2;
                case "*":
                    return value1 * value2;
                case "/":
                    return value1 / value2;
                case "%":
                    return value1 % value2;
                case "<":
                    return value1 < value2;
                case ">":
                    return value1 > value2;
                case "<=":
                    return value1 <= value2;
                case ">=":
                    return value1 >= value2;
                case "==":
                    return value1 == value2;
                case "!=":
                    return value1 != value2;
                default:
                    throw new ArgumentException("Unrecognized op");
            }
        }
    }
}
