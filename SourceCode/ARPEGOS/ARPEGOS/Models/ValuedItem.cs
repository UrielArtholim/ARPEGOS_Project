using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Models
{
    public class ValuedItem: Item
    {
        private int step;
        public int Step
        {
            get => this.Step;
            set => SetProperty(ref this.step, value);
        }

        private int maximum;
        public int Maximum
        {
            get => this.maximum;
            set => SetProperty(ref this.maximum, value);
        }

        public ValuedItem(string itemString, string itemDesc, int max, int step = 1) : base(itemString, itemDesc)
        {
            this.Maximum = max;
            this.Step = step;
        }
    }
}
