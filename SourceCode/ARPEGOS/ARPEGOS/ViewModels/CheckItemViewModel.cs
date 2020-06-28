using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public class CheckItemViewModel: BaseViewModel
    {
        public bool Selected { get; set; }
        public Item Data { get; set; }
        public CheckItemViewModel(Item i = null, bool DefaultValue = false)
        {
            if (i == null)
                Data = new Item("Item_Name", "Item Class", "Item Description");
            else
                Data = i;
            Selected = DefaultValue;
        }
    }
}
