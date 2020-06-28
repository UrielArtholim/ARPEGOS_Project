using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARPEGOS.ViewModels
{
    public class SliderItemViewModel: BaseViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Maximum { get; set; }
        public int Step { get; set; }
        public SliderItemViewModel(Item i = null, int max = 625, int step = 10)
        {
            if (i == null)
                i = new Item("Ganzúa", "Item Class", "Descripción de Ganzúa");
            this.Name = i.FormattedName;
            this.Description = i.Description;
            this.Maximum = max;
            this.Step = step;
        }
          

    }
}
