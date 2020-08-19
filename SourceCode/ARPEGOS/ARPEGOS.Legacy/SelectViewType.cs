using Arpegos_Test.Views;
using ARPEGOS_Test.Models;
using RDFSharp.Semantics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Arpegos_Test.ViewModels
{
    public class SelectViewType
    {
        public SelectViewType(List<string> CreationScheme, string stage, int Counter)
        {
            RDFOntologyTaxonomyEntry StageViewTypeAnnotationEntry = null;
            RDFOntologyProperty StageDefinitionProperty = Program.Game.GameOntology.Model.PropertyModel.SelectProperty(Program.Game.CurrentGameContext + stage);
            if(StageDefinitionProperty == null)
            {
                RDFOntologyClass StageClass;
                List<string> StageWords = stage.Split('_').ToList();
                bool annotationFound = false;
                int wordsCompared = StageWords.Count();
                while (annotationFound == false && wordsCompared > 0)
                {
                    string ClassName = "";
                    
                    if (wordsCompared > 1)
                    {
                        for (int index = 0; index < wordsCompared-1; ++index)
                            ClassName += StageWords.ElementAtOrDefault(index) + "_";
                        ClassName += StageWords.ElementAt(wordsCompared-1);
                        wordsCompared -= 1;
                    }
                    else
                    {
                        ClassName += StageWords.FirstOrDefault();
                        wordsCompared -= 1;
                    }
                    
                    StageClass = Program.Game.GameOntology.Model.ClassModel.SelectClass(Program.Game.CurrentGameContext + ClassName);
                    if(StageClass != null)
                    {
                        RDFOntologyTaxonomy CustomAnnotations = Program.Game.GameOntology.Model.ClassModel.Annotations.CustomAnnotations;
                        RDFOntologyTaxonomy StageCustomAnnotations = CustomAnnotations.SelectEntriesBySubject(StageClass);
                        if (StageCustomAnnotations.Count() > 1)
                        {
                            IEnumerable<RDFOntologyTaxonomyEntry> StageViewTypeAnnotations = StageCustomAnnotations.Where(item => item.TaxonomyPredicate.ToString().Contains("ViewType"));
                            if (StageViewTypeAnnotations.Count() == 1)
                            {
                                annotationFound = true;
                                StageViewTypeAnnotationEntry = StageViewTypeAnnotations.SingleOrDefault();
                            }
                        }
                        else if(StageCustomAnnotations.Count() == 1)
                        {
                            IEnumerable<RDFOntologyTaxonomyEntry> StageViewTypeAnnotations = StageCustomAnnotations.Where(item => item.TaxonomyPredicate.ToString().Contains("ViewType"));
                            if (StageViewTypeAnnotations.Count() == 1)
                            {
                                annotationFound = true;
                                StageViewTypeAnnotationEntry = StageViewTypeAnnotations.SingleOrDefault();
                            }
                        }
                    }
                }
            }
            else
                StageViewTypeAnnotationEntry = Program.Game.GameOntology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(StageDefinitionProperty).Where(item => item.TaxonomyPredicate.ToString().Contains("ViewType")).SingleOrDefault();

            Program.Game.GetLimit(stage, out float? contextValue);

            bool ViewSelected = false;
            while (ViewSelected == false)
            {
                string StageViewType = null;
                if (StageViewTypeAnnotationEntry != null)
                    StageViewType = StageViewTypeAnnotationEntry.TaxonomyObject.ToString().Replace("^^http://www.w3.org/2001/XMLSchema#string", "");                
                
                switch (StageViewType)
                {
                    case "SingleChoiceListView": Program.ShowStage(stage, Counter); new SingleChoiceViewModel(stage, out _); ViewSelected = true; break;
                    case "MultipleChoiceStaticLimitView": Program.ShowStage(stage, Counter); new MultipleChoiceStaticLimitViewModel(stage, contextValue, out contextValue); ViewSelected = true; break;
                    case "MultipleChoiceStaticLimitGroupCostViewModel": Program.ShowStage(stage, Counter); new MultipleChoiceStaticLimitGroupCostViewModel(stage, contextValue, out contextValue); ViewSelected = true; break;
                    case "MultipleChoiceDynamicLimitView": Program.ShowStage(stage, Counter); new MultipleChoiceDynamicLimitViewModel(stage, contextValue, out contextValue); ViewSelected = true; break;
                    case "ValuedListView": Program.ShowStage(stage, Counter); new ValuedListViewModel(stage, contextValue, out contextValue); ViewSelected = true; break;
                    default:
                                SortedList<int, string> OrderedSubstages = Program.Game.GetOrderedSubstages(stage);                                 
                                foreach(string substage in OrderedSubstages.Values)
                                {
                                    new SelectViewType(CreationScheme, substage, Counter);
                                    ++Program.Counter;
                                }
                                ViewSelected = true;
                                break;
                }
            }
        }
    }
}
