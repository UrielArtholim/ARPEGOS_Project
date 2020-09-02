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
        public string Name { get; private set; }

        public StageType Type { get; private set; }
        public StageModel Model { get; private set; }

        public bool IsGrouped { get; private set; }

        public ObservableCollection<Item> Items { get; private set; }

        public Stage (string name, bool grouped = false)
        {
            var characterService = DependencyHelper.CurrentContext.CurrentCharacter;
            this.Name = name;
            this.IsGrouped = grouped;
            this.Items = new ObservableCollection<Item>();
            if (this.IsGrouped == true)
            {
                var stageItems = characterService.GetIndividualsGrouped(Name);
                foreach (var item in stageItems)
                    this.Items.Add(item);
            }
            else
            {
                var stageItems = characterService.GetIndividuals(Name);
                foreach (var item in stageItems)
                    this.Items.Add(item);
            }

            this.Type = StageType.NotDefined;

            var gameService = DependencyHelper.CurrentContext.CurrentGame;
            var stageClass = gameService.Ontology.Model.ClassModel.SelectClass($"{gameService.Ontology}{this.Name}");
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
