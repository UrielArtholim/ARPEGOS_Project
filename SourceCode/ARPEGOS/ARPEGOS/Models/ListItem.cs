using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Models
{
    public class ListItem
    {
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public ListItem() { }
        public ListItem(string name)
        {
            ItemName = name;
        }
        public ListItem(string name, string description)
        {
            ItemName = name;
            ItemDescription = description;
        }
    }
}
