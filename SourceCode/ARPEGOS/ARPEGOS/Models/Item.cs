using System;
using System.Collections.Generic;
using System.Text;

namespace Arpegos_Test
{
    /// <summary>
    /// Item represents a generic element obtained from an ontology
    /// </summary>
    public class Item 
    {
        #region Properties
        /// <summary>
        /// Name of the element
        /// </summary>
        public string Name { get; internal set; }

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
        public Item(string name)
        {
            Name = name;
            Class = Program.Game.GetElementClass(name);
            if (name.Contains(Class))
                name = name.Replace(Class,"").Trim();
            FormattedName = name.Replace('_',' ').Trim();
            Description = Program.Game.GetElementDescription(name);
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
