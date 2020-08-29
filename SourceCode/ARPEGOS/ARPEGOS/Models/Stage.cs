using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
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

        public ObservableCollection<Item> Items { get; private set; }

        public Stage(string name, IEnumerable<Item> items, StageType stageType = StageType.NotDefined)
        {
            this.Name = name;
            this.Type = stageType;
            this.Items = new ObservableCollection<Item>();
            foreach (var item in items)
                this.Items.Add(item);
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
