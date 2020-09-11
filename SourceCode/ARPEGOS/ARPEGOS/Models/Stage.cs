using ARPEGOS.Helpers;
using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ARPEGOS.Models
{
    public class Stage
    {
        /// <summary>
        /// Name of the stage
        /// </summary>
        public string FullName { get; private set; }
        public string ShortName { get; private set; }

        public StageType Type { get; private set; }
        public StageModel Model { get; private set; }

        public bool IsGrouped { get; private set; }

        public ObservableCollection<Item> Items { get; private set; }
        public ObservableCollection<Group> Groups { get; private set; }

        public Stage (string elementString, bool grouped = false)
        {
            var characterService = DependencyHelper.CurrentContext.CurrentCharacter;
            this.FullName = elementString;
            this.ShortName = elementString.Split('#').Last();
            this.IsGrouped = grouped;
            this.Items = null;
            this.Groups = null;
            if (this.IsGrouped == true)
            {
                this.Groups = new ObservableCollection<Group>();
                var stageItems = characterService.GetIndividualsGrouped(this.FullName);
                foreach (var item in stageItems)
                    this.Groups.Add(item);
            }
            else
            {
                this.Items = new ObservableCollection<Item>();
                if (characterService.CheckClass(this.FullName, false))
                {
                    var stageItems = characterService.GetIndividuals(this.FullName);
                    foreach (var item in stageItems)
                        this.Items.Add(item);
                }
                else
                    this.Items.Add(new Item(this.FullName));                
            }

            this.Type = StageType.NotDefined;

            var gameService = DependencyHelper.CurrentContext.CurrentGame;
            var stageClass = gameService.Ontology.Model.ClassModel.SelectClass(this.FullName);
            if(stageClass != null)
            {
                while (this.Type == StageType.NotDefined)
                {
                    var stageClassCustomAnnotations = gameService.Ontology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(stageClass);
                    var stageViewTypeAnnotations = stageClassCustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("ViewType"));
                    if (stageViewTypeAnnotations.Count() > 0)
                    {
                        var viewTypeName = stageViewTypeAnnotations.Single().TaxonomyObject.ToString().Split('^').First();
                        switch (viewTypeName)
                        {
                            case "SingleChoiceListView": this.Type = StageType.SingleChoice; this.Model = StageModel.NotRequired; break;
                            case "MultipleChoiceStaticLimitView": this.Type = StageType.MultipleChoice; this.Model = StageModel.StaticLimit; break;
                            case "MultipleChoiceStaticLimitGroupCostView": this.Type = StageType.MultipleChoice; this.Model = StageModel.StaticGroupLimit; break;
                            case "MultipleChoiceDynamicLimitView": this.Type = StageType.MultipleChoice; this.Model = StageModel.DynamicLimit; break;
                            case "ValuedListView": this.Type = StageType.ValuedChoice; this.Model = StageModel.NotRequired; break;
                        }
                    }
                    else
                        stageClass = gameService.Ontology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(stageClass).Single().TaxonomyObject as RDFOntologyClass;
                }
            }
            else
            {
                var stageFact = gameService.Ontology.Data.SelectFact(this.FullName);
                if(stageFact != null)
                {
                    var stageViewAnnotationEntries = gameService.Ontology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(stageFact).Where(entry => entry.TaxonomyPredicate.ToString().Contains("ViewType"));
                    if (stageViewAnnotationEntries.Count() > 0)
                    {
                        var viewTypeName = stageViewAnnotationEntries.Single().TaxonomyObject.ToString().Split('^').First();
                        switch (viewTypeName)
                        {
                            case "SingleChoiceListView": this.Type = StageType.SingleChoice; this.Model = StageModel.NotRequired; break;
                            case "MultipleChoiceStaticLimitView": this.Type = StageType.MultipleChoice; this.Model = StageModel.StaticLimit; break;
                            case "MultipleChoiceStaticLimitGroupCostView": this.Type = StageType.MultipleChoice; this.Model = StageModel.StaticGroupLimit; break;
                            case "MultipleChoiceDynamicLimitView": this.Type = StageType.MultipleChoice; this.Model = StageModel.DynamicLimit; break;
                            case "ValuedListView": this.Type = StageType.ValuedChoice; this.Model = StageModel.NotRequired; break;
                        }
                    }
                }
                else
                {
                    var stageProperty = gameService.Ontology.Model.PropertyModel.SelectProperty(this.FullName);
                    if(stageProperty != null)
                    {
                        var stageViewAnnotationEntries = gameService.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(stageProperty).Where(entry => entry.TaxonomyPredicate.ToString().Contains("ViewType"));
                        if (stageViewAnnotationEntries.Count() > 0)
                        {
                            var viewTypeName = stageViewAnnotationEntries.Single().TaxonomyObject.ToString().Split('^').First();
                            switch (viewTypeName)
                            {
                                case "SingleChoiceListView": this.Type = StageType.SingleChoice; this.Model = StageModel.NotRequired; break;
                                case "MultipleChoiceStaticLimitView": this.Type = StageType.MultipleChoice; this.Model = StageModel.StaticLimit; break;
                                case "MultipleChoiceStaticLimitGroupCostView": this.Type = StageType.MultipleChoice; this.Model = StageModel.StaticGroupLimit; break;
                                case "MultipleChoiceDynamicLimitView": this.Type = StageType.MultipleChoice; this.Model = StageModel.DynamicLimit; break;
                                case "ValuedListView": this.Type = StageType.ValuedChoice; this.Model = StageModel.NotRequired; break;
                            }
                        }
                    }
                }
            }
        }

        public enum StageType
        {
            SingleChoice,
            MultipleChoice,
            ValuedChoice,
            NotDefined
        }

        public enum StageModel
        {
            StaticLimit,
            StaticGroupLimit,
            DynamicLimit,
            NotRequired
        }
    }
}
