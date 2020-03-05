using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Models
{
    public class ObjectPropertyElement : GameElement
    {
        #region Properties
        public Uri OriginElement { get; set; }
        public Uri DestinyElement { get; set; }
        #endregion

        #region Constructor
        public ObjectPropertyElement(Uri elementUri, Uri originElementUri, Uri destinyElementUri)
        {
            var game = new Game(); //Eliminar en ARPEGOS
            URI = elementUri;
            int index = URI.ToString().LastIndexOf('#');
            Name = URI.ToString().Substring(index);
            FormattedName = Name.Replace('_', ' ').Replace('#', ' ').Trim();
            Type = game.GetElementType(URI);
            OriginElement = originElementUri;
            DestinyElement = destinyElementUri;
        }
        #endregion
    }
}
