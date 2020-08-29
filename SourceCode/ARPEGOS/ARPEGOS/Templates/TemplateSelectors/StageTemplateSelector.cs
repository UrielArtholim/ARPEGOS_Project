using ARPEGOS.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ARPEGOS.Templates.TemplateSelectors
{
    public class StageTemplateSelector: DataTemplateSelector
    {
        public DataTemplate SingleChoiceListTemplate { get;set; }
        public DataTemplate MultipleChoiceStaticLimitTemplate { get; set; }
        public DataTemplate MultipleChoiceStaticLimitGroupCostTemplate { get; set; }
        public DataTemplate MultipleChoiceDynamicLimitTemplate { get; set; }
        public DataTemplate ValuedListTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate (object item, BindableObject container)
        {
            var stage = item as Stage;
            switch(stage.Type)
            {
                case Stage.StageType.MultipleChoiceStaticLimitView: return MultipleChoiceStaticLimitTemplate;
                case Stage.StageType.MultipleChoiceStaticLimitGroupCostView: return MultipleChoiceStaticLimitGroupCostTemplate;
                case Stage.StageType.MultipleChoiceDynamicLimitView: return MultipleChoiceDynamicLimitTemplate;
                case Stage.StageType.ValuedListView: return ValuedListTemplate;
                default: return SingleChoiceListTemplate;
            }

        }
    }
}
