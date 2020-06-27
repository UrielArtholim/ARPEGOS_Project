using ARPEGOS.ViewModels.Base;
using Arpegos_Test;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public class ValuedItemViewModel: BaseViewModel
    {
        private string DefaultItemValue { get; set; }

        public string ItemName { get; private set; }
        public string ItemValue { get; private set; }
        public ValuedItemViewModel()
        {
            Item item = new Item("Item_Name");
            ItemName = item.FormattedName;
            ItemValue = DefaultItemValue;
        }

    }
}
