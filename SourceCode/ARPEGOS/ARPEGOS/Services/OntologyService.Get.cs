using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.ViewModels;
using RDFSharp.Model;
using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARPEGOS.Services
{
    public abstract partial class OntologyService
    {
        /// <summary>
        /// Returns the root class of the creation scheme of the active game
        /// </summary>
        /// <returns></returns>
        public string GetCreationSchemeRootClass()
        {
            string rootClassName = string.Empty;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            var ontology_class_annotations = game.Ontology.Model.ClassModel.Annotations.CustomAnnotations;
            var entries = ontology_class_annotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("CreationSchemeRoot"));
            if (entries.Count() > 0)
                rootClassName = entries.Single().TaxonomySubject.ToString();
            return rootClassName;
        }

        public ObservableCollection<Stage> GetCreationScheme(string objectString)
        {
            var game = DependencyHelper.CurrentContext.CurrentGame;
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var objectFact = game.Ontology.Data.SelectFact(objectString);
            var dataCustomAnnotations = game.Ontology.Data.Annotations.CustomAnnotations;
            var stageCreationSchemeAnnotation = dataCustomAnnotations.SelectEntriesBySubject(objectFact).Where(entry => entry.TaxonomyPredicate.ToString().Contains("CreationScheme")).Single();
            var schemeStages = stageCreationSchemeAnnotation.TaxonomyObject.ToString().Split('^').First().Replace("\r", "").Replace("\n","").Split(',').ToList();
            var editGeneralLimit = false;
            var editStageLimit = false;
            ObservableCollection<Stage> Scheme = new ObservableCollection<Stage>();
            foreach(var name in schemeStages)
            {
                editStageLimit = false;
                editGeneralLimit = false;
                var stepStageString = character.GetString(name, StageViewModel.ApplyOnCharacter);

                RDFOntologyTaxonomy gameCustomAnnotations;
                if(character.CheckDatatypeProperty(stepStageString,false))
                    gameCustomAnnotations = game.Ontology.Model.PropertyModel.Annotations.CustomAnnotations;
                else
                    gameCustomAnnotations = game.Ontology.Model.ClassModel.Annotations.CustomAnnotations;

                var StageCustomAnnotations = gameCustomAnnotations.Where(entry => entry.TaxonomySubject.ToString() == stepStageString);
                if(StageCustomAnnotations.Count() > 0)
                {
                    var EditStageLimitAnnotationEntries = StageCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("EditStageLimit"));
                    if (EditStageLimitAnnotationEntries.Count() > 0)
                        editStageLimit = true;


                    var EditGeneralLimitAnnotationEntries = StageCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("EditGeneralLimit"));
                    if (EditGeneralLimitAnnotationEntries.Count() > 0)
                        editGeneralLimit = true;
                }

                if(stepStageString != null)
                {
                    var isClass = character.CheckClass(stepStageString, StageViewModel.ApplyOnCharacter);
                    if (isClass == true)
                    {
                        var stepStageClass = game.Ontology.Model.ClassModel.SelectClass(stepStageString);
                        bool isGrouped = false;
                        var subclasses = game.Ontology.Model.ClassModel.GetSubClassesOf(stepStageClass);
                        if (subclasses != null)
                        {
                            if(subclasses.ClassesCount > 0)
                            {
                                isGrouped = true;
                                Scheme.Add(new Stage(stepStageString, isGrouped, editGeneralLimit, editStageLimit));
                            }
                            else
                                Scheme.Add(new Stage(stepStageString, false, editGeneralLimit, editStageLimit));

                        }                            
                        else
                            Scheme.Add(new Stage(stepStageString, false, editGeneralLimit, editStageLimit));
                    }
                    else
                    {
                        Scheme.Add(new Stage(stepStageString, false, editGeneralLimit, editStageLimit));
                    }
                }
            }
            return Scheme;
        }
    }
}
