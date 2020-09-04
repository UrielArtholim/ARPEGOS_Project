using ARPEGOS.Helpers;
using ARPEGOS.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ARPEGOS.Views.Templates
{
    public class StageTemplateSelector: DataTemplateSelector
    {
        public DataTemplate StageSingleChoiceTemplate { get; set; }
        public DataTemplate StageMultipleChoiceTemplate { get; set; }
        public DataTemplate StageValuedChoiceTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var stage = item as Stage;
            DataTemplate stageTemplate;
            switch(stage.Type)
            {
                case Stage.StageType.MultipleChoice: stageTemplate = StageMultipleChoiceTemplate;
                    break;
                case Stage.StageType.ValuedChoice: stageTemplate = StageValuedChoiceTemplate;
                    break;
                default:
                    stageTemplate = StageSingleChoiceTemplate;
                    break;
            }
            return stageTemplate;
        }
    }
}
