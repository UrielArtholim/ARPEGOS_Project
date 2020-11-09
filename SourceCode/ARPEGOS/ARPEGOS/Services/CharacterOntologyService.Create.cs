
namespace ARPEGOS.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using ARPEGOS.Helpers;
    using RDFSharp.Model;
    using RDFSharp.Semantics.OWL;
    using Xamarin.Essentials;

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
            var CharacterDataModel = this.Ontology.Data;
            var CharacterLiteral = new RDFOntologyLiteral(new RDFTypedLiteral(value, CheckDatatypeFromString(type)));
            if (!CheckLiteral(value, type))
                CharacterDataModel.AddLiteral(CharacterLiteral);
            this.Save();
            return CharacterLiteral;
        }

        /// <summary>
        /// Creates fact for character given its name
        /// </summary>
        /// <param name="name">Name of the fact</param>
        /// <returns></returns>
        internal RDFOntologyFact CreateFact (string elementName)
        {            
            var GameDataModel = DependencyHelper.CurrentContext.CurrentGame.Ontology.Data;
            var CharacterDataModel = this.Ontology.Data;
            var GameFactString = GetString(elementName);
            var GameFact = GameDataModel.SelectFact(GameFactString);
            var CharacterFact = new RDFOntologyFact(new RDFResource($"{this.Context}{elementName}"));
            if (!CheckFact(GetString(elementName, true)))
                this.Ontology.Data.AddFact(CharacterFact);

            if(elementName != this.Name)
            {
                var GameAnnotations = GameDataModel.Annotations;
                foreach (var propertyInfo in GameAnnotations.GetType().GetProperties())
                {
                    var AnnotationsList = new List<RDFOntologyTaxonomyEntry>();
                    var propertyTaxonomy = GameAnnotations.GetType().GetProperty(propertyInfo.Name).GetValue(GameAnnotations) as RDFOntologyTaxonomy;
                    if (propertyTaxonomy.EntriesCount > 0)
                    {
                        foreach (var entry in propertyTaxonomy)
                            if (entry.TaxonomySubject == GameFact)
                                AnnotationsList.Add(entry);

                        if (propertyInfo.Name != "CustomAnnotations")
                        {
                            foreach (var entry in AnnotationsList)
                            {
                                var type = (RDFSemanticsEnums.RDFOntologyStandardAnnotation)Enum.Parse(typeof(RDFSemanticsEnums.RDFOntologyStandardAnnotation), propertyInfo.Name);
                                CharacterDataModel.AddStandardAnnotation(type, CharacterFact, entry.TaxonomyObject);
                            }
                        }
                        else
                        {
                            foreach (var entry in AnnotationsList)
                            {
                                var annotationShortName = entry.TaxonomyPredicate.ToString().Split('#').Last();
                                var annotationEntries = this.Ontology.Data.Annotations.CustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains(annotationShortName));
                                RDFOntologyAnnotationProperty annotation = null;
                                if(annotationEntries.Count() > 0)
                                    annotation = annotationEntries.First().TaxonomyPredicate as RDFOntologyAnnotationProperty;                                    
                                if (annotation == null)
                                {
                                    annotation = new RDFOntologyAnnotationProperty(new RDFResource($"{this.Context}{annotationShortName}"));
                                    this.Ontology.Model.PropertyModel.AddProperty(annotation);
                                }
                                CharacterDataModel.AddCustomAnnotation(annotation, CharacterFact, entry.TaxonomyObject);
                            }
                        }
                    }
                }
            }
            this.Save();
            return CharacterFact;
        }

        /// <summary>
        /// Creates class for character given its name
        /// </summary>
        /// <param name="name">Name of the class</param>
        /// <returns></returns>
        internal RDFOntologyClass CreateClass(string elementName)
        {            
            var GameClassModel = DependencyHelper.CurrentContext.CurrentGame.Ontology.Model.ClassModel;
            var CharacterClassModel = this.Ontology.Model.ClassModel;
            elementName = FileService.EscapedName(elementName.Split('#').Last());
            var elementString = GetString(elementName);
            var GameClass = GameClassModel.SelectClass(elementString);
            elementString = $"{this.Context}{elementName}";
            var CharacterClass = new RDFOntologyClass(new RDFResource(elementString));
            if (!CheckClass(elementString))
                CharacterClassModel.AddClass(CharacterClass);
            RDFOntologyClass CharacterPreviousClass = null;
            var GameSuperClasses = GameClassModel.GetSuperClassesOf(GameClass);
            if(GameSuperClasses.ClassesCount > 0)
            {
                var UpperClasses = GameSuperClasses.ToList<RDFOntologyClass>();
                UpperClasses.Reverse();
                foreach (var item in UpperClasses)
                {
                    var CharacterUpperClassName = item.Value.ToString().Split('#').Last();
                    var CharacterUpperClassString = $"{this.Context}{CharacterUpperClassName}";
                    var CharacterUpperClass = new RDFOntologyClass(new RDFResource(CharacterUpperClassString));
                    if (!CheckClass(CharacterUpperClassName))
                        CharacterClassModel.AddClass(CharacterUpperClass);
                    if (CharacterPreviousClass != null)
                        CharacterClassModel.AddSubClassOfRelation(CharacterUpperClass, CharacterPreviousClass);
                    CharacterPreviousClass = CharacterUpperClass;
                }
            }
            
            if (CharacterPreviousClass != null)
                CharacterClassModel.AddSubClassOfRelation(CharacterClass, CharacterPreviousClass);

            var GameAnnotations = GameClassModel.Annotations;
            foreach (var propertyInfo in GameAnnotations.GetType().GetProperties())
            {
                var AnnotationsList = new List<RDFOntologyTaxonomyEntry>();
                var propertyTaxonomy = GameAnnotations.GetType().GetProperty(propertyInfo.Name).GetValue(GameAnnotations) as RDFOntologyTaxonomy;
                if (propertyTaxonomy.EntriesCount > 0)
                {
                    foreach (var entry in propertyTaxonomy)
                        if (entry.TaxonomySubject == GameClass)
                            AnnotationsList.Add(entry);
                    if (propertyInfo.Name != "CustomAnnotations")
                    {
                        foreach (var entry in AnnotationsList)
                        {
                            var type = (RDFSemanticsEnums.RDFOntologyStandardAnnotation) Enum.Parse(typeof(RDFSemanticsEnums.RDFOntologyStandardAnnotation), propertyInfo.Name);
                            var propertyShortName = propertyInfo.Name.Split('#').Last();
                            var characterPropertyString = $"{this.Context}{propertyShortName}";
                            var entryObject = entry.TaxonomyObject;
                            CharacterClassModel.AddStandardAnnotation(type, CharacterClass, entry.TaxonomyObject);
                        }
                    }
                    else
                    {
                        foreach (var entry in AnnotationsList)
                        {
                            var annotationShortName = entry.TaxonomyPredicate.ToString().Split('#').Last();
                            var annotationEntries = this.Ontology.Model.ClassModel.Annotations.CustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains(annotationShortName));
                            if(annotationEntries.Count() > 0)
                            {
                                var annotation = annotationEntries.First().TaxonomyPredicate as RDFOntologyAnnotationProperty;
                                if (annotation == null)
                                {
                                    annotation = new RDFOntologyAnnotationProperty(new RDFResource($"{this.Context}{annotationShortName}"));
                                    this.Ontology.Model.PropertyModel.AddProperty(annotation);
                                }
                                CharacterClassModel.AddCustomAnnotation(annotation, CharacterClass, entry.TaxonomyObject);
                            }                                
                        }
                    }
                }
            }
            this.Save();
            return CharacterClass;
        }

        /// <summary>
        /// Creates object property for character given its name
        /// </summary>
        /// <param name="name">Name of the object property</param>
        /// <returns></returns>
        internal RDFOntologyObjectProperty CreateObjectProperty(string elementName)
        {            
            var GamePropertyModel = DependencyHelper.CurrentContext.CurrentGame.Ontology.Model.PropertyModel;
            var CharacterPropertyModel = this.Ontology.Model.PropertyModel;
            var CharacterClassModel = this.Ontology.Model.ClassModel;
            var elementString = GetString(elementName);
            var GameObjectProperty = GamePropertyModel.SelectProperty(elementString) as RDFOntologyObjectProperty;
            elementString = $"{this.Context}{elementName}";
            var CharacterObjectProperty = new RDFOntologyObjectProperty(new RDFResource(elementString));
            if (!CheckObjectProperty(elementString))
            {
                if (GameObjectProperty.Domain != null)
                {
                    RDFOntologyClass DomainClass;
                    var DomainString = GameObjectProperty.Domain.ToString();
                    var DomainName = DomainString.Split('#').Last();
                    var CharacterDomainString = $"{this.Context}{DomainName}";
                    if (!CheckClass(CharacterDomainString))
                    {
                        DomainClass = CreateClass(DomainName);
                        this.Save();
                    }
                    else
                        DomainClass = CharacterClassModel.SelectClass(DomainString);
                    CharacterObjectProperty.SetDomain(DomainClass);
                }

                if (GameObjectProperty.Range != null)
                {
                    RDFOntologyClass RangeClass;
                    var RangeString = GameObjectProperty.Range.ToString();
                    var RangeName = RangeString.Split('#').Last();
                    var CharacterRangeString = $"{this.Context}{RangeName}";
                    if (!CheckClass(CharacterRangeString))
                    {
                        RangeClass = CreateClass(RangeName);
                        this.Save();
                    }
                    else
                        RangeClass = CharacterClassModel.SelectClass(CharacterRangeString);
                    CharacterObjectProperty.SetRange(RangeClass);
                }
                CharacterObjectProperty.SetFunctional(GameObjectProperty.Functional);
                CharacterPropertyModel.AddProperty(CharacterObjectProperty);
            }
            var GameSuperProperties = GamePropertyModel.GetSuperPropertiesOf(GameObjectProperty).ToList();
            GameSuperProperties.Reverse();
            RDFOntologyObjectProperty CharacterPreviousObjectProperty = null;
            foreach (var item in GameSuperProperties)
            {
                var superproperty = item as RDFOntologyObjectProperty;
                var superpropertyString = superproperty.ToString();
                var superpropertyName = superpropertyString.Split('#').Last();
                var CharacterUpperProperty = new RDFOntologyObjectProperty(new RDFResource($"{this.Context}{superpropertyName}"));
                if (!CheckObjectProperty(superpropertyString))
                    CharacterPropertyModel.AddProperty(CharacterUpperProperty);
                if (CharacterPreviousObjectProperty != null)
                    CharacterPropertyModel.AddSubPropertyOfRelation(CharacterUpperProperty, CharacterPreviousObjectProperty);
                CharacterPreviousObjectProperty = CharacterUpperProperty;
            }
            if (CharacterPreviousObjectProperty != null)
                CharacterPropertyModel.AddSubPropertyOfRelation(CharacterObjectProperty, CharacterPreviousObjectProperty);
            var GameAnnotations = GamePropertyModel.Annotations;
            var GameAnnotationsProperties = GameAnnotations.GetType().GetProperties();
            foreach (var propertyInfo in GameAnnotationsProperties)
            {
                var AnnotationsList = new List<RDFOntologyTaxonomyEntry>();
                var propertyTaxonomy = GameAnnotations.GetType().GetProperty(propertyInfo.Name).GetValue(GameAnnotations) as RDFOntologyTaxonomy;
                if (propertyTaxonomy.EntriesCount > 0)
                {
                    foreach (var entry in propertyTaxonomy)
                        if (entry.TaxonomySubject == GameObjectProperty)
                            AnnotationsList.Add(entry);
                    if (propertyInfo.Name != "CustomAnnotations")
                    {
                        foreach (var entry in AnnotationsList)
                        {
                            RDFSemanticsEnums.RDFOntologyStandardAnnotation type = (RDFSemanticsEnums.RDFOntologyStandardAnnotation) Enum.Parse(typeof(RDFSemanticsEnums.RDFOntologyStandardAnnotation), propertyInfo.Name);
                            CharacterPropertyModel.AddStandardAnnotation(type, CharacterObjectProperty, entry.TaxonomyObject);
                        }
                    }
                    else
                    {
                        foreach (var entry in AnnotationsList)
                        {
                            var annotationShortName = entry.TaxonomyPredicate.ToString().Split('#').Last();
                            RDFOntologyAnnotationProperty annotation;
                            var CharacterPropertyCustomAnnotations = this.Ontology.Model.PropertyModel.Annotations.CustomAnnotations;
                            if (CharacterPropertyCustomAnnotations.EntriesCount > 0)
                            {
                                var annotationPropertyEntries = CharacterPropertyCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains(annotationShortName));
                                if (annotationPropertyEntries.Count() > 0)
                                    annotation = annotationPropertyEntries.First().TaxonomyPredicate as RDFOntologyAnnotationProperty;
                                else
                                {
                                    annotation = new RDFOntologyAnnotationProperty(new RDFResource($"{this.Context}{annotationShortName}"));
                                    CharacterPropertyModel.AddProperty(annotation);
                                }
                            }
                            else
                            {
                                annotation = new RDFOntologyAnnotationProperty(new RDFResource($"{this.Context}{annotationShortName}"));
                                CharacterPropertyModel.AddProperty(annotation);
                            }
                            CharacterPropertyModel.AddCustomAnnotation(annotation, CharacterObjectProperty, entry.TaxonomyObject);
                        }
                    }
                }
            }
            this.Save();
            return CharacterObjectProperty;
        }

        /// <summary>
        /// Creates datatype property for character given its name
        /// </summary>
        /// <param name="name">Name of the datatype property</param>
        /// <returns></returns>
        internal RDFOntologyDatatypeProperty CreateDatatypeProperty(string elementName)
        {            
            var GamePropertyModel = DependencyHelper.CurrentContext.CurrentGame.Ontology.Model.PropertyModel;
            var CharacterPropertyModel = this.Ontology.Model.PropertyModel;
            var CharacterClassModel = this.Ontology.Model.ClassModel;
            var elementString = GetString(elementName);
            var GameDatatypeProperty = GamePropertyModel.SelectProperty(elementString) as RDFOntologyDatatypeProperty;
            var GameSuperProperties = GamePropertyModel.GetSuperPropertiesOf(GameDatatypeProperty).ToList();
            GameSuperProperties.Reverse();
            elementString = $"{this.Context}{elementName}";
            var CharacterDatatypeProperty = new RDFOntologyDatatypeProperty(new RDFResource(elementString));
            if (!CheckDatatypeProperty(elementString))
            {
                if(GameDatatypeProperty.Domain != null)
                {
                    RDFOntologyClass DomainClass;
                    var DomainString = GameDatatypeProperty.Domain.ToString();
                    var DomainName = DomainString.Split('#').Last();
                    var CharacterDomainString = $"{this.Context}{DomainName}";
                    if (!CheckClass(CharacterDomainString))
                    {
                        DomainClass = CreateClass(DomainName);
                        this.Save();
                    }
                    else
                        DomainClass = CharacterClassModel.SelectClass(CharacterDomainString);
                    CharacterDatatypeProperty.SetDomain(DomainClass);
                }
                if (GameDatatypeProperty.Range != null)
                {
                    var RangeName = GameDatatypeProperty.Range.ToString().Split('#').Last();
                    CharacterDatatypeProperty.SetRange(CheckClassFromDatatype(CheckDatatypeFromString(RangeName)));
                }
                CharacterDatatypeProperty.SetFunctional(GameDatatypeProperty.Functional);
                CharacterPropertyModel.AddProperty(CharacterDatatypeProperty);
            }
            RDFOntologyDatatypeProperty CharacterPreviousDatatypeProperty = null;
            foreach (var item in GameSuperProperties)
            {
                var superproperty = item as RDFOntologyDatatypeProperty;
                var superpropertyString = superproperty.ToString();
                var superpropertyName = superpropertyString.Split('#').Last();
                var CharacterUpperProperty = new RDFOntologyDatatypeProperty(new RDFResource(superpropertyString));
                if (!CheckDatatypeProperty(superpropertyString))
                    CharacterPropertyModel.AddProperty(CharacterUpperProperty);
                if (CharacterPreviousDatatypeProperty != null)
                    CharacterPropertyModel.AddSubPropertyOfRelation(CharacterUpperProperty, CharacterPreviousDatatypeProperty);
                CharacterPreviousDatatypeProperty = CharacterUpperProperty;
            }
            if (CharacterPreviousDatatypeProperty != null)
                CharacterPropertyModel.AddSubPropertyOfRelation(CharacterDatatypeProperty, CharacterPreviousDatatypeProperty);
            var GameAnnotations = GamePropertyModel.Annotations;
            foreach (var propertyInfo in GameAnnotations.GetType().GetProperties())
            {
                var AnnotationsList = new List<RDFOntologyTaxonomyEntry>();
                var propertyTaxonomy = GameAnnotations.GetType().GetProperty(propertyInfo.Name).GetValue(GameAnnotations) as RDFOntologyTaxonomy;
                if (propertyTaxonomy.EntriesCount > 0)
                {
                    foreach (var entry in propertyTaxonomy)
                    {
                        if (entry.TaxonomySubject == GameDatatypeProperty)
                            AnnotationsList.Add(entry);
                    }

                    if (propertyInfo.Name != "CustomAnnotations")
                    {
                        foreach (var entry in AnnotationsList)
                        {
                            var type = (RDFSemanticsEnums.RDFOntologyStandardAnnotation) Enum.Parse(typeof(RDFSemanticsEnums.RDFOntologyStandardAnnotation), propertyInfo.Name);
                            CharacterPropertyModel.AddStandardAnnotation(type, CharacterDatatypeProperty, entry.TaxonomyObject);
                        }
                    }
                    else
                    {
                        foreach (var entry in AnnotationsList)
                        {
                            var annotationShortName = entry.TaxonomyPredicate.ToString().Split('#').Last();
                            RDFOntologyAnnotationProperty annotation;
                            var CharacterPropertyCustomAnnotations = this.Ontology.Model.PropertyModel.Annotations.CustomAnnotations;
                            if(CharacterPropertyCustomAnnotations.EntriesCount > 0)
                            {
                                var annotationPropertyEntries = CharacterPropertyCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains(annotationShortName));
                                if(annotationPropertyEntries.Count() > 0)
                                    annotation = annotationPropertyEntries.First().TaxonomyPredicate as RDFOntologyAnnotationProperty;
                                else
                                {
                                    annotation = new RDFOntologyAnnotationProperty(new RDFResource($"{this.Context}{annotationShortName}"));
                                    CharacterPropertyModel.AddProperty(annotation);
                                }
                            }
                            else
                            {
                                annotation = new RDFOntologyAnnotationProperty(new RDFResource($"{this.Context}{annotationShortName}"));
                                CharacterPropertyModel.AddProperty(annotation);
                            }
                            CharacterPropertyModel.AddCustomAnnotation(annotation, CharacterDatatypeProperty, entry.TaxonomyObject);
                        }
                    }
                }
            }
            this.Save();
            return CharacterDatatypeProperty;
        }

        /// <summary>
        /// Creates individual for character given its name
        /// </summary>
        /// <param name="name">Name of the individual</param>
        /// <returns></returns>
        internal RDFOntologyFact CreateIndividual(string elementName)
        {
            
            RDFOntologyFact CharacterSubject = null;
            elementName = FileService.EscapedName(elementName);
            if (elementName.Contains(FileService.EscapedName(this.Name)))
            {
                if (!CheckFact($"{this.Context}{this.Name}"))
                    CharacterSubject = CreateFact(FileService.EscapedName(this.Name));
                RDFOntologyClass subjectClass;
                var subjectClassName = "Personaje_Jugador";
                var subjectClassString = GetString(subjectClassName);
                if (!CheckClass(subjectClassString))
                    subjectClass = CreateClass(subjectClassName);
                else
                    subjectClass = this.Ontology.Model.ClassModel.SelectClass(subjectClassString);
                this.Ontology.Data.AddClassTypeRelation(CharacterSubject, subjectClass);
            }
            else
            {
                var GameClassModel = DependencyHelper.CurrentContext.CurrentGame.Ontology.Model.ClassModel;
                var CharacterClassModel = this.Ontology.Model.ClassModel;
                var GamePropertyModel = DependencyHelper.CurrentContext.CurrentGame.Ontology.Model.PropertyModel;
                var CharacterPropertyModel = this.Ontology.Model.PropertyModel;
                var GameDataModel = DependencyHelper.CurrentContext.CurrentGame.Ontology.Data;
                var CharacterDataModel = this.Ontology.Data;
                var GameFactString = GetString(elementName);
                var GameNamedFact = GameDataModel.SelectFact(GameFactString);
                var GameNamedFactClasstype = GameDataModel.Relations.ClassType.SelectEntriesBySubject(GameNamedFact).Single();
                var FactClassString = GameNamedFactClasstype.TaxonomyObject.ToString();
                var FactClassName = FactClassString.Split('#').Last();
                RDFOntologyClass CharacterSubjectClass;
                if (!CheckClass(FactClassString))
                    CharacterSubjectClass = CreateClass(FactClassName);
                else
                    CharacterSubjectClass = CharacterClassModel.SelectClass(FactClassString);
                RDFOntologyFact CharacterObject;
                var FactString = $"{this.Context}{elementName}";
                if (!CheckFact(FactString))
                {
                    CharacterSubject = CreateFact(elementName);
                    CharacterDataModel.AddClassTypeRelation(CharacterSubject, CharacterSubjectClass);
                }
                else
                {
                    CharacterSubject = CharacterDataModel.SelectFact(FactString);
                    CharacterDataModel.AddClassTypeRelation(CharacterSubject, CharacterSubjectClass);
                }
                var GameNamedFactAssertions = GameDataModel.Relations.Assertions.SelectEntriesBySubject(GameNamedFact);
                foreach (var assertion in GameNamedFactAssertions)
                {
                    var PredicateType = assertion.TaxonomyPredicate.GetType().ToString();
                    if (PredicateType.Contains("RDFOntologyObjectProperty"))
                    {
                        var GamePredicate = assertion.TaxonomyPredicate as RDFOntologyObjectProperty;
                        var PredicateString = GamePredicate.ToString();
                        var PredicateName = PredicateString.Split('#').Last();
                        RDFOntologyObjectProperty CharacterPredicate;
                        if (!CheckObjectProperty(PredicateString))
                            CharacterPredicate = CreateObjectProperty(PredicateName);
                        else
                            CharacterPredicate = CharacterPropertyModel.SelectProperty(PredicateString) as RDFOntologyObjectProperty;
                        var ObjectString = assertion.TaxonomyObject.Value.ToString();
                        var ObjectName = ObjectString.Split('#').Last();
                        if (!CheckFact(ObjectString))
                            CharacterObject = CreateIndividual(ObjectName);
                        else
                            CharacterObject = CharacterDataModel.SelectFact(ObjectString);
                        CharacterDataModel.AddAssertionRelation(CharacterSubject, CharacterPredicate, CharacterObject);
                    }
                    else
                    {
                        RDFOntologyDatatypeProperty CharacterPredicate;
                        var GamePredicate = assertion.TaxonomyPredicate as RDFOntologyDatatypeProperty;
                        var PredicateString = GamePredicate.ToString();
                        var PredicateName = PredicateString.Split('#').Last();
                        if (!CheckDatatypeProperty(PredicateString))
                            CharacterPredicate = CreateDatatypeProperty(PredicateName);
                        else
                            CharacterPredicate = GamePropertyModel.SelectProperty(PredicateString) as RDFOntologyDatatypeProperty;
                        var value = assertion.TaxonomyObject.Value.ToString().Split('^').First();
                        var valuetype = assertion.TaxonomyObject.Value.ToString().Split('#').Last();
                        RDFOntologyLiteral Literal;
                        if (!CheckLiteral(value, valuetype))
                            Literal = CreateLiteral(value, valuetype);
                        else
                            Literal = CharacterDataModel.SelectLiteral(new RDFOntologyLiteral(new RDFTypedLiteral(value, CheckDatatypeFromString(valuetype))).ToString());
                        CharacterDataModel.AddAssertionRelation(CharacterSubject, CharacterPredicate, Literal);
                    }
                }
                var IndividualAssertions = CharacterDataModel.Relations.Assertions;
                int assertionCounter = 1;
                foreach (var assertion in IndividualAssertions)
                {
                    var assertionSubject = assertion.TaxonomySubject.ToString().Split('#').Last();
                    var assertionPredicate = assertion.TaxonomyPredicate.ToString().Split('#').Last();
                    var assertionObject = assertion.TaxonomyObject.ToString().Split('#').Last();
                    ++assertionCounter;
                }
            }
            this.Save();
            return CharacterSubject;
        }
    }
}
