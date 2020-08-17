
namespace ARPEGOS.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RDFSharp.Model;
    using RDFSharp.Semantics;

    public abstract partial class OntologyService
    {
        /// <summary>
        /// Returns true if the given cost is related to the given limit
        /// </summary>
        /// <param name="cost">Name of the cost</param>
        /// <param name="limit">Name of the limit</param>
        /// <returns></returns>
        public static bool CheckCostAndLimit(string cost, string limit)
        {
            var ignorableWords = new List<string> { "Coste", "Cost", "Coût", "Total"};
            var costWords = cost.Split('_').ToList();
            var limitWords = limit.Split('_').ToList();

            var comparableWords = Math.Min(costWords.Count, limitWords.Count);
            if (costWords.Count == comparableWords)
            {
                if (ignorableWords.Any(cost.Contains))
                    --comparableWords;
            }
            else
            {
                if (ignorableWords.Any(limit.Contains))
                    --comparableWords;
            }

            var costMatchLimit = comparableWords > 0;

            for (var index = 0; index < comparableWords; ++index)
            {
                var itemCostWord = costWords.ElementAtOrDefault(index);
                var generalLimitWord = limitWords.ElementAtOrDefault(index);
                var lowerLength = Math.Min(generalLimitWord.Length, itemCostWord.Length);
                if (itemCostWord.Length > lowerLength)
                    itemCostWord = itemCostWord.Substring(0, lowerLength);
                if (generalLimitWord.Length > lowerLength)
                    generalLimitWord = generalLimitWord.Substring(0, lowerLength);
                if (itemCostWord != generalLimitWord)
                    costMatchLimit = false;
            }
            return costMatchLimit;
        }

        /// <summary>
        /// Returns semantic datatype given its name
        /// </summary>
        /// <param name="type">Name of the semantic datatype</param>
        /// <returns></returns>
        public static RDFModelEnums.RDFDatatypes CheckDatatypeFromString(string type)
        {
            switch (type)
            {
                case "XMLLiteral":
                    return RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL;
                case "string":
                    return RDFModelEnums.RDFDatatypes.XSD_STRING;
                case "boolean":
                    return RDFModelEnums.RDFDatatypes.XSD_BOOLEAN;
                case "decimal":
                    return RDFModelEnums.RDFDatatypes.XSD_DECIMAL;
                case "float":
                    return RDFModelEnums.RDFDatatypes.XSD_FLOAT;
                case "double":
                    return RDFModelEnums.RDFDatatypes.XSD_DOUBLE;
                case "positiveInteger":
                    return RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER;
                case "negativeInteger":
                    return RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER;
                case "nonPositiveInteger":
                    return RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER;
                case "nonNegativeInteger":
                    return RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER;
                case "integer":
                    return RDFModelEnums.RDFDatatypes.XSD_INTEGER;
                case "long":
                    return RDFModelEnums.RDFDatatypes.XSD_LONG;
                case "int":
                    return RDFModelEnums.RDFDatatypes.XSD_INT;
                case "short":
                    return RDFModelEnums.RDFDatatypes.XSD_SHORT;
                case "byte":
                    return RDFModelEnums.RDFDatatypes.XSD_BYTE;
                case "unsignedLong":
                    return RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG;
                case "unsignedShort":
                    return RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT;
                case "unsignedByte":
                    return RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE;
                case "unsignedInt":
                    return RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT;
                case "duration":
                    return RDFModelEnums.RDFDatatypes.XSD_DURATION;
                case "dateTime":
                    return RDFModelEnums.RDFDatatypes.XSD_DATETIME;
                case "date":
                    return RDFModelEnums.RDFDatatypes.XSD_DATE;
                case "time":
                    return RDFModelEnums.RDFDatatypes.XSD_TIME;
                case "gYear":
                    return RDFModelEnums.RDFDatatypes.XSD_GYEAR;
                case "gMonth":
                    return RDFModelEnums.RDFDatatypes.XSD_GMONTH;
                case "gDay":
                    return RDFModelEnums.RDFDatatypes.XSD_GDAY;
                case "gYearMonth":
                    return RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH;
                case "gMonthDay":
                    return RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY;
                case "hexBinary":
                    return RDFModelEnums.RDFDatatypes.XSD_HEXBINARY;
                case "base64Binary":
                    return RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY;
                case "anyURI":
                    return RDFModelEnums.RDFDatatypes.XSD_ANYURI;
                case "QName":
                    return RDFModelEnums.RDFDatatypes.XSD_QNAME;
                case "notation":
                    return RDFModelEnums.RDFDatatypes.XSD_NOTATION;
                case "language":
                    return RDFModelEnums.RDFDatatypes.XSD_LANGUAGE;
                case "normalizedString":
                    return RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING;
                case "token":
                    return RDFModelEnums.RDFDatatypes.XSD_TOKEN;
                case "NMToken":
                    return RDFModelEnums.RDFDatatypes.XSD_NMTOKEN;
                case "name":
                    return RDFModelEnums.RDFDatatypes.XSD_NAME;
                case "NCName":
                    return RDFModelEnums.RDFDatatypes.XSD_NCNAME;
                case "ID":
                    return RDFModelEnums.RDFDatatypes.XSD_ID;
                default:
                    return RDFModelEnums.RDFDatatypes.RDFS_LITERAL;
            }
        }

        /// <summary>
        /// Return class given a semantic datatype
        /// </summary>
        /// <param name="datatype">Semantic datatype</param>
        /// <returns></returns>
        public static RDFOntologyClass CheckClassFromDatatype(RDFModelEnums.RDFDatatypes datatype)
        {
            switch (datatype)
            {
                case RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL:
                    return new RDFOntologyClass(RDFVocabulary.RDF.XML_LITERAL);
                case RDFModelEnums.RDFDatatypes.RDFS_LITERAL:
                    return new RDFOntologyClass(RDFVocabulary.RDFS.LITERAL);
                case RDFModelEnums.RDFDatatypes.XSD_STRING:
                    return new RDFOntologyClass(RDFVocabulary.XSD.STRING);
                case RDFModelEnums.RDFDatatypes.XSD_ANYURI:
                    return new RDFOntologyClass(RDFVocabulary.XSD.ANY_URI);
                case RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY:
                    return new RDFOntologyClass(RDFVocabulary.XSD.BASE64_BINARY);
                case RDFModelEnums.RDFDatatypes.XSD_BOOLEAN:
                    return new RDFOntologyClass(RDFVocabulary.XSD.BOOLEAN);
                case RDFModelEnums.RDFDatatypes.XSD_BYTE:
                    return new RDFOntologyClass(RDFVocabulary.XSD.BYTE);
                case RDFModelEnums.RDFDatatypes.XSD_DATE:
                    return new RDFOntologyClass(RDFVocabulary.XSD.DATE);
                case RDFModelEnums.RDFDatatypes.XSD_DATETIME:
                    return new RDFOntologyClass(RDFVocabulary.XSD.DATETIME);
                case RDFModelEnums.RDFDatatypes.XSD_DECIMAL:
                    return new RDFOntologyClass(RDFVocabulary.XSD.DECIMAL);
                case RDFModelEnums.RDFDatatypes.XSD_DOUBLE:
                    return new RDFOntologyClass(RDFVocabulary.XSD.DOUBLE);
                case RDFModelEnums.RDFDatatypes.XSD_DURATION:
                    return new RDFOntologyClass(RDFVocabulary.XSD.DURATION);
                case RDFModelEnums.RDFDatatypes.XSD_FLOAT:
                    return new RDFOntologyClass(RDFVocabulary.XSD.FLOAT);
                case RDFModelEnums.RDFDatatypes.XSD_GDAY:
                    return new RDFOntologyClass(RDFVocabulary.XSD.G_DAY);
                case RDFModelEnums.RDFDatatypes.XSD_GMONTH:
                    return new RDFOntologyClass(RDFVocabulary.XSD.G_MONTH);
                case RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY:
                    return new RDFOntologyClass(RDFVocabulary.XSD.G_MONTH_DAY);
                case RDFModelEnums.RDFDatatypes.XSD_GYEAR:
                    return new RDFOntologyClass(RDFVocabulary.XSD.G_YEAR);
                case RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH:
                    return new RDFOntologyClass(RDFVocabulary.XSD.G_YEAR_MONTH);
                case RDFModelEnums.RDFDatatypes.XSD_HEXBINARY:
                    return new RDFOntologyClass(RDFVocabulary.XSD.HEX_BINARY);
                case RDFModelEnums.RDFDatatypes.XSD_INT:
                    return new RDFOntologyClass(RDFVocabulary.XSD.INT);
                case RDFModelEnums.RDFDatatypes.XSD_INTEGER:
                    return new RDFOntologyClass(RDFVocabulary.XSD.INTEGER);
                case RDFModelEnums.RDFDatatypes.XSD_LANGUAGE:
                    return new RDFOntologyClass(RDFVocabulary.XSD.LANGUAGE);
                case RDFModelEnums.RDFDatatypes.XSD_LONG:
                    return new RDFOntologyClass(RDFVocabulary.XSD.LONG);
                case RDFModelEnums.RDFDatatypes.XSD_NAME:
                    return new RDFOntologyClass(RDFVocabulary.XSD.NAME);
                case RDFModelEnums.RDFDatatypes.XSD_NCNAME:
                    return new RDFOntologyClass(RDFVocabulary.XSD.NCNAME);
                case RDFModelEnums.RDFDatatypes.XSD_ID:
                    return new RDFOntologyClass(RDFVocabulary.XSD.ID);
                case RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER:
                    return new RDFOntologyClass(RDFVocabulary.XSD.NEGATIVE_INTEGER);
                case RDFModelEnums.RDFDatatypes.XSD_NMTOKEN:
                    return new RDFOntologyClass(RDFVocabulary.XSD.NMTOKEN);
                case RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER:
                    return new RDFOntologyClass(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER);
                case RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER:
                    return new RDFOntologyClass(RDFVocabulary.XSD.NON_POSITIVE_INTEGER);
                case RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING:
                    return new RDFOntologyClass(RDFVocabulary.XSD.NORMALIZED_STRING);
                case RDFModelEnums.RDFDatatypes.XSD_NOTATION:
                    return new RDFOntologyClass(RDFVocabulary.XSD.NOTATION);
                case RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER:
                    return new RDFOntologyClass(RDFVocabulary.XSD.POSITIVE_INTEGER);
                case RDFModelEnums.RDFDatatypes.XSD_QNAME:
                    return new RDFOntologyClass(RDFVocabulary.XSD.QNAME);
                case RDFModelEnums.RDFDatatypes.XSD_SHORT:
                    return new RDFOntologyClass(RDFVocabulary.XSD.SHORT);
                case RDFModelEnums.RDFDatatypes.XSD_TIME:
                    return new RDFOntologyClass(RDFVocabulary.XSD.TIME);
                case RDFModelEnums.RDFDatatypes.XSD_TOKEN:
                    return new RDFOntologyClass(RDFVocabulary.XSD.TOKEN);
                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE:
                    return new RDFOntologyClass(RDFVocabulary.XSD.UNSIGNED_BYTE);
                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT:
                    return new RDFOntologyClass(RDFVocabulary.XSD.UNSIGNED_INT);
                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG:
                    return new RDFOntologyClass(RDFVocabulary.XSD.UNSIGNED_LONG);
                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT:
                    return new RDFOntologyClass(RDFVocabulary.XSD.UNSIGNED_SHORT);
                //Unknown datatypes default to instances of "rdfs:Literal"
                default:
                    return new RDFOntologyClass(RDFVocabulary.RDFS.LITERAL);
            }
        }

        /// <summary>
        /// Returns the result of "value1" -> "op" -> "value2"
        /// </summary>
        /// <param name="op"></param>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static dynamic ConvertToOperator(string op, float value1, float value2)
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
