
namespace ARPEGOS.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
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
        internal RDFOntologyFact CreateFact (string name)
        {
            var GameDataModel = this.Game.Ontology.Data;
            var CharacterDataModel = this.Ontology.Data;
            var GameFact = GameDataModel.SelectFact(this.Game.Context + name);
            var CharacterFact = new RDFOntologyFact(new RDFResource(this.Context + name));
            if (!CheckFact(name))
                CharacterDataModel.AddFact(CharacterFact);
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
                            var type = (RDFSemanticsEnums.RDFOntologyStandardAnnotation) Enum.Parse(typeof(RDFSemanticsEnums.RDFOntologyStandardAnnotation), propertyInfo.Name);
                            CharacterDataModel.AddStandardAnnotation(type, CharacterFact, entry.TaxonomyObject);
                        }
                    }
                    else
                    {
                        foreach (var entry in AnnotationsList)
                        {
                            var AnnotationType = entry.TaxonomyPredicate.ToString().Split('#').Last();
                            RDFOntologyAnnotationProperty AnnotationProperty = this.Ontology.Model.PropertyModel.SelectProperty(this.Context + AnnotationType) as RDFOntologyAnnotationProperty;
                            if (AnnotationProperty == null)
                            {
                                var annotation = new RDFOntologyAnnotationProperty(new RDFResource(this.Context + AnnotationType));
                                this.Ontology.Model.PropertyModel.AddProperty(annotation);
                                AnnotationProperty = annotation;
                            }
                            CharacterDataModel.AddCustomAnnotation(AnnotationProperty, CharacterFact, entry.TaxonomyObject);
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
        internal RDFOntologyClass CreateClass(string name)
        {
            var GameClassModel = this.Game.Ontology.Model.ClassModel;
            var CharacterClassModel = this.Ontology.Model.ClassModel;
            name = FileService.FormatName(FileService.EscapedName(name));
            var GameClass = GameClassModel.SelectClass(this.Game.Context + name);
            var CharacterClass = new RDFOntologyClass(new RDFResource(this.Context + name));
            if (!CheckClass(name))
                CharacterClassModel.AddClass(CharacterClass);
            RDFOntologyClass CharacterPreviousClass = null;
            var GameSuperClasses = GameClassModel.GetSuperClassesOf(GameClass);
            var UpperClasses = GameSuperClasses.ToList<RDFOntologyClass>();
            UpperClasses.Reverse();
            foreach (var item in UpperClasses)
            {
                var CharacterUpperClassName = item.Value.ToString().Split('#').Last();
                var CharacterUpperClass = new RDFOntologyClass(new RDFResource(this.Context + CharacterUpperClassName));
                if (!CheckClass(CharacterUpperClassName))
                    CharacterClassModel.AddClass(CharacterUpperClass);
                if (CharacterPreviousClass != null)
                    CharacterClassModel.AddSubClassOfRelation(CharacterUpperClass, CharacterPreviousClass);
                CharacterPreviousClass = CharacterUpperClass;
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
                            CharacterClassModel.AddStandardAnnotation(type, CharacterClass, entry.TaxonomyObject);
                        }
                    }
                    else
                    {
                        foreach (var entry in AnnotationsList)
                        {
                            var AnnotationType = entry.TaxonomyPredicate.ToString().Substring(entry.TaxonomyPredicate.ToString().LastIndexOf('#') + 1);
                            RDFOntologyAnnotationProperty AnnotationProperty = this.Ontology.Model.PropertyModel.SelectProperty(this.Context + AnnotationType) as RDFOntologyAnnotationProperty;
                            if (AnnotationProperty == null)
                            {
                                var annotation = new RDFOntologyAnnotationProperty(new RDFResource(this.Context + AnnotationType));
                                this.Ontology.Model.PropertyModel.AddProperty(annotation);
                                AnnotationProperty = annotation;
                            }
                            CharacterClassModel.AddCustomAnnotation(AnnotationProperty, CharacterClass, entry.TaxonomyObject);
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
        internal RDFOntologyObjectProperty CreateObjectProperty(string name)
        {
            var GamePropertyModel = this.Game.Ontology.Model.PropertyModel;
            var CharacterPropertyModel = this.Ontology.Model.PropertyModel;
            var CharacterClassModel = this.Ontology.Model.ClassModel;
            var GameObjectProperty = GamePropertyModel.SelectProperty(this.Game.Context + name) as RDFOntologyObjectProperty;
            var CharacterObjectProperty = new RDFOntologyObjectProperty(new RDFResource(this.Context + name));
            if (!CheckObjectProperty(name))
            {
                if (GameObjectProperty.Domain != null)
                {
                    RDFOntologyClass DomainClass;
                    var DomainName = GameObjectProperty.Domain.ToString().Split('#').Last();;
                    if (!CheckClass(DomainName))
                        DomainClass = CreateClass(DomainName);
                    else
                        DomainClass = CharacterClassModel.SelectClass(this.Context + DomainName);
                    CharacterObjectProperty.SetDomain(DomainClass);
                }

                if (GameObjectProperty.Range != null)
                {
                    RDFOntologyClass RangeClass;
                    var RangeName = GameObjectProperty.Range.ToString().Split('#').Last();
                    if (!CheckClass(RangeName))
                        RangeClass = CreateClass(RangeName);
                    else
                        RangeClass = CharacterClassModel.SelectClass(this.Context + RangeName);
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
                var superpropertyName = superproperty.ToString().Split('#').Last();
                var CharacterUpperProperty = new RDFOntologyObjectProperty(new RDFResource(this.Context + superpropertyName));
                if (!CheckObjectProperty(superpropertyName))
                    CharacterPropertyModel.AddProperty(CharacterUpperProperty);
                if (CharacterPreviousObjectProperty != null)
                    CharacterPropertyModel.AddSubPropertyOfRelation(CharacterUpperProperty, CharacterPreviousObjectProperty);
                CharacterPreviousObjectProperty = CharacterUpperProperty;
            }
            if (CharacterPreviousObjectProperty != null)
                CharacterPropertyModel.AddSubPropertyOfRelation(CharacterObjectProperty, CharacterPreviousObjectProperty);
            var GameAnnotations = GamePropertyModel.Annotations;
            foreach (var propertyInfo in GameAnnotations.GetType().GetProperties())
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
                            var AnnotationType = entry.TaxonomyPredicate.ToString().Substring(entry.TaxonomyPredicate.ToString().LastIndexOf('#') + 1);
                            RDFOntologyAnnotationProperty AnnotationProperty = CharacterPropertyModel.SelectProperty(this.Context + AnnotationType) as RDFOntologyAnnotationProperty;
                            if (AnnotationProperty == null)
                            {
                                var annotation = new RDFOntologyAnnotationProperty(new RDFResource(this.Context + AnnotationType));
                                CharacterPropertyModel.AddProperty(annotation);
                                AnnotationProperty = annotation;
                            }
                            CharacterPropertyModel.AddCustomAnnotation(AnnotationProperty, CharacterObjectProperty, entry.TaxonomyObject);
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
        internal RDFOntologyDatatypeProperty CreateDatatypeProperty(string name)
        {
            var GamePropertyModel = this.Game.Ontology.Model.PropertyModel;
            var CharacterPropertyModel = this.Ontology.Model.PropertyModel;
            var CharacterClassModel = this.Ontology.Model.ClassModel;
            var GameDatatypeProperty = GamePropertyModel.SelectProperty(this.Game.Context + name) as RDFOntologyDatatypeProperty;
            var GameSuperProperties = GamePropertyModel.GetSuperPropertiesOf(GameDatatypeProperty).ToList();
            GameSuperProperties.Reverse();
            var CharacterDatatypeProperty = new RDFOntologyDatatypeProperty(new RDFResource(this.Context + name));
            if (!CheckDatatypeProperty(name))
            {
                if(GameDatatypeProperty.Domain != null)
                {
                    RDFOntologyClass DomainClass;
                    var DomainName = GameDatatypeProperty.Domain.ToString().Split('#').Last();
                    if (!CheckClass(DomainName))
                        DomainClass = CreateClass(DomainName);
                    else
                        DomainClass = CharacterClassModel.SelectClass(this.Context + DomainName);
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
                var superpropertyName = superproperty.ToString().Split('#').Last();
                var CharacterUpperProperty = new RDFOntologyDatatypeProperty(new RDFResource(this.Context + superpropertyName));
                if (!CheckDatatypeProperty(superpropertyName))
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
                            var AnnotationType = entry.TaxonomyPredicate.ToString().Substring(entry.TaxonomyPredicate.ToString().LastIndexOf('#') + 1);
                            RDFOntologyAnnotationProperty AnnotationProperty = CharacterPropertyModel.SelectProperty(this.Context + AnnotationType) as RDFOntologyAnnotationProperty;
                            if (AnnotationProperty == null)
                            {
                                var annotation = new RDFOntologyAnnotationProperty(new RDFResource(this.Context + AnnotationType));
                                CharacterPropertyModel.AddProperty(annotation);
                                AnnotationProperty = annotation;
                            }
                            CharacterPropertyModel.AddCustomAnnotation(AnnotationProperty, CharacterDatatypeProperty, entry.TaxonomyObject);
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
        internal RDFOntologyFact CreateIndividual(string name)
        {
            RDFOntologyFact CharacterSubject = null;
            name = FileService.EscapedName(name);
            if (this.Name.Contains(name))
            {
                if (!CheckFact(name))
                    CharacterSubject = CreateFact(name);
                RDFOntologyClass subjectClass;
                var subjectClassName = "Personaje Jugador";
                if (!CheckClass(subjectClassName))
                    subjectClass = CreateClass(subjectClassName);
                else
                    subjectClass = this.Ontology.Model.ClassModel.SelectClass(this.Context + subjectClassName);
                this.Ontology.Data.AddClassTypeRelation(CharacterSubject, subjectClass);
            }
            else
            {
                var GameClassModel = this.Game.Ontology.Model.ClassModel;
                var CharacterClassModel = this.Ontology.Model.ClassModel;
                var GamePropertyModel = this.Game.Ontology.Model.PropertyModel;
                var CharacterPropertyModel = this.Ontology.Model.PropertyModel;
                var GameDataModel = this.Game.Ontology.Data;
                var CharacterDataModel = this.Ontology.Data;
                var GameNamedFact = GameDataModel.SelectFact(this.Game.Context + name);
                var GameNamedFactClasstype = GameDataModel.Relations.ClassType.FirstOrDefault(entry => entry.TaxonomySubject.Value.Equals(GameNamedFact));
                var FactClassName = GameNamedFactClasstype.TaxonomyObject.ToString().Split('#').Last();
                RDFOntologyClass CharacterSubjectClass;
                if (!CheckClass(FactClassName))
                    CharacterSubjectClass = CreateClass(FactClassName);
                else
                    CharacterSubjectClass = CharacterClassModel.SelectClass(this.Context + FactClassName);
                RDFOntologyFact CharacterObject;
                if (!CheckFact(name))
                {
                    CharacterSubject = CreateFact(name);
                    CharacterDataModel.AddClassTypeRelation(CharacterSubject, CharacterSubjectClass);
                }
                else
                {
                    CharacterSubject = CharacterDataModel.SelectFact(this.Game.Context + name);
                    CharacterDataModel.AddClassTypeRelation(CharacterSubject, CharacterSubjectClass);
                }
                var GameNamedFactAssertions = GameDataModel.Relations.Assertions.SelectEntriesBySubject(GameNamedFact);
                foreach (var assertion in GameNamedFactAssertions)
                {
                    var PredicateType = assertion.TaxonomyPredicate.GetType().ToString();
                    if (PredicateType.Contains("RDFOntologyObjectProperty"))
                    {
                        var GamePredicate = assertion.TaxonomyPredicate as RDFOntologyObjectProperty;
                        var PredicateName = GamePredicate.ToString().Split('#').Last();
                        RDFOntologyObjectProperty CharacterPredicate;
                        if (!CheckObjectProperty(PredicateName))
                            CharacterPredicate = CreateObjectProperty(PredicateName);
                        else
                            CharacterPredicate = CharacterPropertyModel.SelectProperty(this.Context + PredicateName) as RDFOntologyObjectProperty;
                        var ObjectName = assertion.TaxonomyObject.Value.ToString().Split('#').Last();
                        if (!CheckFact(ObjectName))
                            CharacterObject = CreateIndividual(ObjectName);
                        else
                            CharacterObject = CharacterDataModel.SelectFact(this.Context + ObjectName);
                        CharacterDataModel.AddAssertionRelation(CharacterSubject, CharacterPredicate, CharacterObject);
                    }
                    else
                    {
                        RDFOntologyDatatypeProperty CharacterPredicate;
                        var GamePredicate = assertion.TaxonomyPredicate as RDFOntologyDatatypeProperty;
                        var PredicateName = GamePredicate.ToString().Split('#').Last();
                        if (!CheckDatatypeProperty(PredicateName))
                            CharacterPredicate = CreateDatatypeProperty(PredicateName);
                        else
                            CharacterPredicate = GamePropertyModel.SelectProperty(this.Context + PredicateName) as RDFOntologyDatatypeProperty;
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
