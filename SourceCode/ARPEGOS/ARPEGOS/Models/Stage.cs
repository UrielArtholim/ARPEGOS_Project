using ARPEGOS.Helpers;
using ARPEGOS.Services;
using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xamarin.Forms;

namespace ARPEGOS.Models
{
    public class Stage
    {
        /// <summary>
        /// Name of the stage
        /// </summary>
        public string Name { get; private set; }

        public StageType Type { get; private set; }

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
                    switch(viewTypeName)
                    {
                        case "SingleChoiceListView": this.Type = StageType.SingleChoiceListView; break;
                        case "MultipleChoiceStaticLimitView": this.Type = StageType.MultipleChoiceStaticLimitView; break;
                        case "MultipleChoiceStaticLimitGroupCostView": this.Type = StageType.MultipleChoiceStaticLimitGroupCostView; break;
                        case "MultipleChoiceDynamicLimitView": this.Type = StageType.MultipleChoiceDynamicLimitView; break;
                        case "ValuedListView": this.Type = StageType.ValuedListView; break;
                    }
                }
                else
                    stageClass = gameService.Ontology.Model.ClassModel.Relations.SubClassOf.SelectEntriesBySubject(stageClass).Single().TaxonomyObject as RDFOntologyClass;
            }
        }
        public enum StageType
        {
            SingleChoiceListView,
            MultipleChoiceStaticLimitView,
            MultipleChoiceStaticLimitGroupCostView,
            MultipleChoiceDynamicLimitView,
            ValuedListView,
            NotDefined
        }       
    }
}
