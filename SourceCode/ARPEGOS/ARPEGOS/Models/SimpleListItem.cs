using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Models
{
    class SimpleListItem
    {
        public string DisplayName { get; set; }
        public SimpleListItem(string item)
        {
            DisplayName = item;
        }
    }
}
