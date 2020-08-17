
namespace ARPEGOS.Services
{
    using System.Collections.Generic;
    using System.Linq;

    using RDFSharp.Model;
    using RDFSharp.Semantics;

    public partial class CharacterOntologyService
    {
        /// <summary>
        /// Creates literal for character given its value and type
        /// </summary>
        /// <param name="value">Value of the literal</param>
        /// <param name="type">Semantic datatype of the literal</param>
        /// <returns></returns>
        internal RDFOntologyLiteral CreateLiteral(string value, string type)
        {
            RDFOntologyData CharacterDataModel = CharacterOntology.Data;
            
            RDFOntologyLiteral CharacterLiteral = new RDFOntologyLiteral(new RDFTypedLiteral(value, CheckDatatypeFromString(type)));
            if (!CheckLiteral(value, type))
                CharacterDataModel.AddLiteral(CharacterLiteral);
            //Console.WriteLine("\nLista de literales pertenecientes al personaje");
            IEnumerator<RDFOntologyLiteral> enumerator = CharacterDataModel.LiteralsEnumerator;
            while (enumerator.MoveNext())
            {
                //Console.WriteLine("Literal: " + enumerator.Current.ToString());
            }
            //Console.ReadLine();
            SaveCharacter();
            return CharacterLiteral;
        }

        /// <summary>
        /// Creates fact for character given its name
        /// </summary>
        /// <param name="name">Name of the fact</param>
        /// <returns></returns>
        internal RDFOntologyFact CreateFact(string name)
        {
            RDFOntologyData GameDataModel = GameOntology.Data;
            RDFOntologyData CharacterDataModel = CharacterOntology.Data;
            RDFOntologyFact GameFact = GameDataModel.SelectFact(CurrentGameContext + name);
            RDFOntologyTaxonomy FactCommentEntries = GameDataModel.Annotations.Comment.SelectEntriesBySubject(GameFact);
            RDFOntologyFact CharacterFact = new RDFOntologyFact(new RDFResource(CurrentCharacterContext + name));
            if (!CheckFact(name))
                CharacterDataModel.AddFact(CharacterFact);

            if (FactCommentEntries.EntriesCount > 0)
            {                
                string FactDescription = FactCommentEntries.SingleOrDefault().TaxonomyObject.ToString();
                if (FactDescription.Contains('^'))
                    FactDescription.Substring(0, FactDescription.IndexOf('^'));

                RDFOntologyLiteral DescriptionLiteral = CreateLiteral(FactDescription, "string");
                CharacterDataModel.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, CharacterFact, DescriptionLiteral);

            }
            SaveCharacter();
            return CharacterFact;
        }

        /// <summary>
        /// Creates class for character given its name
        /// </summary>
        /// <param name="name">Name of the class</param>
        /// <returns></returns>
        internal RDFOntologyClass CreateClass(string name)
        {
            RDFOntologyClassModel GameClassModel = GameOntology.Model.ClassModel;
            RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;

            RDFOntologyClass CharacterClass;
            name = Text.ToTitleCase(name.Replace(" ", "_"));
            RDFOntologyClass GameClass = GameClassModel.SelectClass(CurrentGameContext + name);

            CharacterClass = new RDFOntologyClass(new RDFResource(CurrentCharacterContext + name));
            if (!CheckClass(name))
                CharacterClassModel.AddClass(CharacterClass);

            RDFOntologyClass CharacterPreviousClass = null;
            var GameSuperClasses = GameClassModel.GetSuperClassesOf(GameClass);
            List<RDFOntologyClass> UpperClasses = GameSuperClasses.ToList<RDFOntologyClass>();
            UpperClasses.Reverse();
            foreach (RDFOntologyClass item in UpperClasses)
            {
                string CharacterUpperClassName = item.Value.ToString().Substring(item.Value.ToString().LastIndexOf('#') + 1);
                RDFOntologyClass CharacterUpperClass = new RDFOntologyClass(new RDFResource(CurrentCharacterContext + CharacterUpperClassName));
                if (!CheckClass(CharacterUpperClassName))
                    CharacterClassModel.AddClass(CharacterUpperClass);
                if (CharacterPreviousClass != null)
                    CharacterClassModel.AddSubClassOfRelation(CharacterUpperClass, CharacterPreviousClass);
                CharacterPreviousClass = CharacterUpperClass;
            }

            if (CharacterPreviousClass != null)
                CharacterClassModel.AddSubClassOfRelation(CharacterClass, CharacterPreviousClass);

            SaveCharacter();
            return CharacterClass;
        }

        /// <summary>
        /// Creates object property for character given its name
        /// </summary>
        /// <param name="name">Name of the object property</param>
        /// <returns></returns>
        internal RDFOntologyObjectProperty CreateObjectProperty(string name)
        {
            RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;
            RDFOntologyPropertyModel CharacterPropertyModel = CharacterOntology.Model.PropertyModel;
            RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;

            RDFOntologyObjectProperty GameObjectProperty = GamePropertyModel.SelectProperty(CurrentGameContext + name) as RDFOntologyObjectProperty;
            RDFOntologyObjectProperty CharacterPreviousObjectProperty = null;

            RDFOntologyObjectProperty CharacterObjectProperty = new RDFOntologyObjectProperty(new RDFResource(CurrentCharacterContext + name));
            if (!CheckObjectProperty(name))
            {
                if (GameObjectProperty.Domain != null)
                {
                    string DomainName;
                    RDFOntologyClass DomainClass;

                    DomainName = GameObjectProperty.Domain.ToString().Substring(GameObjectProperty.Domain.ToString().LastIndexOf('#') + 1);
                    if (!CheckClass(DomainName))
                        DomainClass = CreateClass(DomainName);
                    else
                        DomainClass = CharacterClassModel.SelectClass(CurrentCharacterContext + DomainName);
                    CharacterObjectProperty.SetDomain(DomainClass);
                }

                if (GameObjectProperty.Range != null)
                {
                    string RangeName;
                    RDFOntologyClass RangeClass;

                    RangeName = GameObjectProperty.Range.ToString().Substring(GameObjectProperty.Range.ToString().LastIndexOf('#') + 1);
                    if (!CheckClass(RangeName))
                        RangeClass = CreateClass(RangeName);
                    else
                        RangeClass = CharacterClassModel.SelectClass(CurrentCharacterContext + RangeName);
                    CharacterObjectProperty.SetRange(RangeClass);
                }

                CharacterObjectProperty.SetFunctional(GameObjectProperty.Functional);
                CharacterPropertyModel.AddProperty(CharacterObjectProperty);
            }

            List<RDFOntologyProperty> GameSuperProperties = GamePropertyModel.GetSuperPropertiesOf(GameObjectProperty).ToList();
            GameSuperProperties.Reverse();
            foreach (RDFOntologyProperty item in GameSuperProperties)
            {
                RDFOntologyObjectProperty superproperty = item as RDFOntologyObjectProperty;
                string superpropertyName = superproperty.ToString().Substring(superproperty.ToString().LastIndexOf('#') + 1);
                RDFOntologyObjectProperty CharacterUpperProperty = new RDFOntologyObjectProperty(new RDFResource(CurrentCharacterContext + superpropertyName));

                if (!CheckObjectProperty(superpropertyName))
                    CharacterPropertyModel.AddProperty(CharacterUpperProperty);
                if (CharacterPreviousObjectProperty != null)
                    CharacterPropertyModel.AddSubPropertyOfRelation(CharacterUpperProperty, CharacterPreviousObjectProperty);
                CharacterPreviousObjectProperty = CharacterUpperProperty;
            }

            if (CharacterPreviousObjectProperty != null)
                CharacterPropertyModel.AddSubPropertyOfRelation(CharacterObjectProperty, CharacterPreviousObjectProperty);

            SaveCharacter();
            return CharacterObjectProperty;
        }

        /// <summary>
        /// Creates datatype property for character given its name
        /// </summary>
        /// <param name="name">Name of the datatype property</param>
        /// <returns></returns>
        internal RDFOntologyDatatypeProperty CreateDatatypeProperty(string name)
        {
            RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;
            RDFOntologyPropertyModel CharacterPropertyModel = CharacterOntology.Model.PropertyModel;
            RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;

            RDFOntologyDatatypeProperty GameDatatypeProperty = GamePropertyModel.SelectProperty(CurrentGameContext + name) as RDFOntologyDatatypeProperty;
            RDFOntologyDatatypeProperty CharacterPreviousDatatypeProperty = null;

            List<RDFOntologyProperty> GameSuperProperties = GamePropertyModel.GetSuperPropertiesOf(GameDatatypeProperty).ToList();
            GameSuperProperties.Reverse();
            // Vincular superpropiedades a propiedades

            RDFOntologyDatatypeProperty CharacterDatatypeProperty = new RDFOntologyDatatypeProperty(new RDFResource(CurrentCharacterContext + name));
            if (!CheckDatatypeProperty(name))
            {
                if(GameDatatypeProperty.Domain != null)
                {
                    RDFOntologyClass DomainClass;

                    string DomainName = GameDatatypeProperty.Domain.ToString().Substring(GameDatatypeProperty.Domain.ToString().LastIndexOf('#') + 1);
                    if (!CheckClass(DomainName))
                        DomainClass = CreateClass(DomainName);
                    else
                        DomainClass = CharacterClassModel.SelectClass(CurrentCharacterContext + DomainName);
                    CharacterDatatypeProperty.SetDomain(DomainClass);
                }

                if (GameDatatypeProperty.Range != null)
                {
                    string RangeName = GameDatatypeProperty.Range.ToString().Substring(GameDatatypeProperty.Range.ToString().LastIndexOf('#') + 1);
                    CharacterDatatypeProperty.SetRange(CheckClassFromDatatype(CheckDatatypeFromString(RangeName)));
                }

                CharacterDatatypeProperty.SetFunctional(GameDatatypeProperty.Functional);
                CharacterPropertyModel.AddProperty(CharacterDatatypeProperty);
            }

            foreach (RDFOntologyProperty item in GameSuperProperties)
            {
                RDFOntologyDatatypeProperty superproperty = item as RDFOntologyDatatypeProperty;
                string superpropertyName = superproperty.ToString().Substring(superproperty.ToString().LastIndexOf('#') + 1);
                RDFOntologyDatatypeProperty CharacterUpperProperty = new RDFOntologyDatatypeProperty(new RDFResource(CurrentCharacterContext + superpropertyName));

                if (!CheckDatatypeProperty(superpropertyName))
                    CharacterPropertyModel.AddProperty(CharacterUpperProperty);
                if (CharacterPreviousDatatypeProperty != null)
                    CharacterPropertyModel.AddSubPropertyOfRelation(CharacterUpperProperty, CharacterPreviousDatatypeProperty);
                CharacterPreviousDatatypeProperty = CharacterUpperProperty;
            }

            if (CharacterPreviousDatatypeProperty != null)
                CharacterPropertyModel.AddSubPropertyOfRelation(CharacterDatatypeProperty, CharacterPreviousDatatypeProperty);

            SaveCharacter();
            return CharacterDatatypeProperty;
        }

        /// <summary>
        /// Creates individual for character given its name
        /// </summary>
        /// <param name="name">Name of the individual</param>
        /// <returns></returns>
        internal RDFOntologyFact CreateIndividual(string name)
        {
            RDFOntologyFact CharacterSubject = null;
            name = name.Replace(" ", "_");
            if (CurrentCharacterName.Contains(name))
            {
                if (!CheckFact(name))
                    CharacterSubject = CreateFact(name);

                RDFOntologyClass subjectClass;
                string subjectClassName = "Personaje Jugador";
                if (!CheckClass(subjectClassName))
                    subjectClass = CreateClass(subjectClassName);
                else
                    subjectClass = CharacterOntology.Model.ClassModel.SelectClass(CurrentCharacterContext + subjectClassName);

                CharacterOntology.Data.AddClassTypeRelation(CharacterSubject, subjectClass);
            }
            else
            {
                RDFOntologyClassModel GameClassModel = GameOntology.Model.ClassModel;
                RDFOntologyClassModel CharacterClassModel = CharacterOntology.Model.ClassModel;
                RDFOntologyPropertyModel GamePropertyModel = GameOntology.Model.PropertyModel;
                RDFOntologyPropertyModel CharacterPropertyModel = CharacterOntology.Model.PropertyModel;
                RDFOntologyData GameDataModel = GameOntology.Data;
                RDFOntologyData CharacterDataModel = CharacterOntology.Data;

                // Comprobar que existe la clase del sujeto 
                RDFOntologyFact GameNamedFact = GameDataModel.SelectFact(CurrentGameContext + name);
                RDFOntologyClass CharacterSubjectClass;
                RDFOntologyTaxonomyEntry GameNamedFactClasstype = GameDataModel.Relations.ClassType.FirstOrDefault(entry => entry.TaxonomySubject.Value.Equals(GameNamedFact));
                string FactClassName = GameNamedFactClasstype.TaxonomyObject.ToString().Substring(GameNamedFactClasstype.TaxonomyObject.ToString().LastIndexOf('#') + 1);
                if (!CheckClass(FactClassName))
                    CharacterSubjectClass = CreateClass(FactClassName);
                else
                    CharacterSubjectClass = CharacterClassModel.SelectClass(CurrentCharacterContext + FactClassName);

                // Comprobar si existe el sujeto
                RDFOntologyFact CharacterObject;
                if (!CheckFact(name))
                {
                    CharacterSubject = CreateFact(name);
                    CharacterDataModel.AddClassTypeRelation(CharacterSubject, CharacterSubjectClass);
                }
                else
                {
                    CharacterSubject = CharacterDataModel.SelectFact(CurrentGameContext + name);
                    CharacterDataModel.AddClassTypeRelation(CharacterSubject, CharacterSubjectClass);
                }

                // Para cada propiedad del objeto existente en la BD
                var GameNamedFactAssertions = GameDataModel.Relations.Assertions.SelectEntriesBySubject(GameNamedFact);
                foreach (var assertion in GameNamedFactAssertions)
                {
                    // Comprobar el tipo de predicado
                    string PredicateType = assertion.TaxonomyPredicate.GetType().ToString();
                    if (PredicateType.Contains("RDFOntologyObjectProperty"))
                    {
                        // Si el predicado es una propiedad de objeto
                        RDFOntologyObjectProperty CharacterPredicate;
                        RDFOntologyObjectProperty GamePredicate = assertion.TaxonomyPredicate as RDFOntologyObjectProperty;
                        string PredicateName = GamePredicate.ToString().Substring(GamePredicate.ToString().LastIndexOf('#') + 1);
                        if (!CheckObjectProperty(PredicateName))
                            CharacterPredicate = CreateObjectProperty(PredicateName);
                        else
                            CharacterPredicate = CharacterPropertyModel.SelectProperty(CurrentCharacterContext + PredicateName) as RDFOntologyObjectProperty;

                        // Comprobar que el objeto existe
                        string ObjectName = assertion.TaxonomyObject.Value.ToString().Substring(assertion.TaxonomyObject.Value.ToString().LastIndexOf('#') + 1);
                        if (!CheckFact(ObjectName))
                            CharacterObject = CreateIndividual(ObjectName);
                        else
                            CharacterObject = CharacterDataModel.SelectFact(CurrentCharacterContext + ObjectName);

                        CharacterDataModel.AddAssertionRelation(CharacterSubject, CharacterPredicate, CharacterObject);
                    }
                    else
                    {
                        // Si el predicado es una propiedad de datos
                        RDFOntologyDatatypeProperty CharacterPredicate;
                        RDFOntologyDatatypeProperty GamePredicate = assertion.TaxonomyPredicate as RDFOntologyDatatypeProperty;
                        string PredicateName = GamePredicate.ToString().Substring(GamePredicate.ToString().LastIndexOf('#') + 1);
                        if (!CheckDatatypeProperty(PredicateName))
                            CharacterPredicate = CreateDatatypeProperty(PredicateName);
                        else
                            CharacterPredicate = GamePropertyModel.SelectProperty(CurrentCharacterContext + PredicateName) as RDFOntologyDatatypeProperty;

                        string value = assertion.TaxonomyObject.Value.ToString().Substring(0, assertion.TaxonomyObject.Value.ToString().IndexOf('^'));
                        int typeIndex = assertion.TaxonomyObject.Value.ToString().LastIndexOf('#') + 1;
                        int typeLength = assertion.TaxonomyObject.Value.ToString().Length;
                        string valuetype = assertion.TaxonomyObject.Value.ToString()[typeIndex..typeLength];

                        RDFOntologyLiteral Literal;
                        if (!CheckLiteral(value, valuetype))
                            Literal = CreateLiteral(value, valuetype);
                        else
                            Literal = CharacterDataModel.SelectLiteral(new RDFOntologyLiteral(new RDFTypedLiteral(value, CheckDatatypeFromString(valuetype))).ToString());

                        CharacterDataModel.AddAssertionRelation(CharacterSubject, CharacterPredicate, Literal);
                    }
                }
                RDFOntologyTaxonomy IndividualAssertions = CharacterDataModel.Relations.Assertions;



                //Console.WriteLine("\tIndividual Assertions: ");
                int assertionCounter = 1;
                foreach (var assertion in IndividualAssertions)
                {
                    var assertionSubject = assertion.TaxonomySubject.ToString().Substring(assertion.TaxonomySubject.ToString().LastIndexOf('#') + 1);
                    var assertionPredicate = assertion.TaxonomyPredicate.ToString().Substring(assertion.TaxonomyPredicate.ToString().LastIndexOf('#') + 1);
                    var assertionObject = assertion.TaxonomyObject.ToString().Substring(assertion.TaxonomyObject.ToString().LastIndexOf('#') + 1);

                    //Console.WriteLine(assertionCounter + " -\t" + assertionSubject + "\t" +assertionPredicate +"\t" + assertionObject + ".");
                    ++assertionCounter;
                }
                //Console.ReadLine();
            }
            SaveCharacter();
            return CharacterSubject;
        }
    }
}
