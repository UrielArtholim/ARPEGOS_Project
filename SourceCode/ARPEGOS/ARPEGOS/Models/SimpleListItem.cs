using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Models
{
    class SimpleListItem
    {
        public string DisplayName { get; set; }
        public string DisplayDescription { get; set; }
        public SimpleListItem(string item)
        {
            DisplayName = item;
        }

        public SimpleListItem(string itemName, string ItemDescription)
        {
            DisplayName = itemName;
            DisplayDescription = ItemDescription;
        }
    }
}
