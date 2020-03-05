using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Models
{
    public class SimpleListItem
    {
        #region Properties
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        #endregion

        #region Constructors
        public SimpleListItem() { }
        public SimpleListItem(string name)
        {
            ItemName = name;
        }
        public SimpleListItem(string name, string description)
        {
            ItemName = name;
            ItemDescription = description;
        }
        #endregion
    }
}
