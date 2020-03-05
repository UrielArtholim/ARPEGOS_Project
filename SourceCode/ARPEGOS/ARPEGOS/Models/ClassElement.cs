using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Models
{
    public class ClassElement : GameElement
    {
        #region Properties
        public Uri Class { get; protected set; }
        public string Description { get; protected set; }
        #endregion

        #region Constructor
        public ClassElement(string elementUri)
        {
            URI = new Uri(SystemControl.ActiveGame.GameGraph.Context + elementUri);
            int index = URI.ToString().LastIndexOf('#');
            Name = URI.ToString().Substring(index);
            FormattedName = Name.Replace('_', ' ').Replace('#', ' ').Trim();
            Type = SystemControl.ActiveGame.GetElementType(URI);
            Console.WriteLine("Type = " + Type.URI.ToString());
            Class = new Uri(SystemControl.ActiveGame.GetElementClass(URI));
            Description = SystemControl.ActiveGame.GetElementDescription(this);
        }
        #endregion
    }
}
