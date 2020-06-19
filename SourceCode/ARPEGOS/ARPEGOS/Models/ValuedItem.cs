using System;
using System.Collections.Generic;
using System.Text;

namespace Arpegos_Test
{
    /// <summary>
    /// ValuedItem represents an item that contains a numeric value
    /// </summary>
    public class ValuedItem : Item
    {
        #region Properties
        public dynamic Value { get; internal set; }
        #endregion

        #region Ctors
        public ValuedItem(string name, dynamic value): base(name)
        {
            Value = value;
        }
        #endregion
    }
}
