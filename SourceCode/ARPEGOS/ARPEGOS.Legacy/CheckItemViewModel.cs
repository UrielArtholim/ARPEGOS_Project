using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public class CheckItemViewModel: BaseViewModel
    {
        public bool Selected { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public CheckItemViewModel(Item i = null)
        {
            if(i == null)
                i = new Item("Ganzúa", "Item Class", "Descripción de Ganzúa");
            this.Name = i.Name;
            this.Description = i.Description;
        }
    }
}
