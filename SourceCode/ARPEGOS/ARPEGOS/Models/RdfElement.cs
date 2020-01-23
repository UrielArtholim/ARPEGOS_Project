using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Models
{
    class RdfElement
    {
        public string DisplayName { get; set; }
        public RdfElement(string item)
        {
            //uri = new Uri(item);
            //int index = item.LastIndexOf('#');
            //name = item.Substring(index + 1).Replace("_", " ").Trim();
            DisplayName = item;
        }
    }
}
