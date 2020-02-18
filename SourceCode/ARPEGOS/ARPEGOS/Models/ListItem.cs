using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Models
{
    public class ListItem
    {
        #region Properties
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        #endregion

        #region Constructors
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
        #endregion
    }
}
