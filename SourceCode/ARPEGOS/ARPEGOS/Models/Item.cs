using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARPEGOS
{
    /// <summary>
    /// Item represents a generic element obtained from an ontology
    /// </summary>
    public class Item 
    {
        #region Properties
        
        /// <summary>
        /// Full name of the element
        /// </summary>
        public string FullName { get; internal set; }

        /// <summary>
        /// Short name of the element
        /// </summary>
        public string ShortName { get; internal set; }


        /// <summary>
        /// Name of the element formatted for UI
        /// </summary>
        public string FormattedName { get; internal set; }

        /// <summary>
        /// Name of the class to which the element belongs
        /// </summary>
        public string Class { get; internal set; }

        /// <summary>
        /// Description of the element
        /// </summary>
        public string Description { get; internal set; }
        #endregion

        #region Constructor
        public Item(string name, string description = "This is a description", string Class = "Classname")
        {
            FullName = name;
            ShortName = name.Split('#').Last();
            this.Class = Class;
            if (ShortName.Contains(Class))
                if (!ShortName.Contains("_de_"))
                    ShortName = name.Replace(Class,"").Trim();
            FormattedName = ShortName.Replace("Per_","").Replace("_Total","").Replace('_',' ').Trim();
            Description = description;
        }
        #endregion

        #region Methods 
        public void ShowItem(string tree)
        {
            Console.WriteLine( tree + "====>" + FormattedName);
        }
        #endregion
    }
}
