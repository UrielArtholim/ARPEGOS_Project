using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public class SliderItemViewModel: BaseViewModel
    {
        public string Value { get; set; }
        public Item Data { get; set; }
        public SliderItemViewModel(Item i = null, string DefaultValue = "0")
        {
            if (i == null)
                Data = new Item("Item_Name", "Item Class", "Item Description");
            else
                Data = i;
            Value = DefaultValue;
        }

    }
}
