using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Models
{
    public class DatatypePropertyElement : GameElement
    {
        #region Properties
        public Uri OriginElement { get; set; }
        public dynamic RelatedValue { get; set; }
        #endregion

        #region Constructor
        public DatatypePropertyElement(Uri propertyUri, Uri originElementUri, dynamic propertyValue)
        {
            var game = SystemControl.ActiveGame;
            URI = propertyUri;
            int index = URI.ToString().LastIndexOf('#');
            Name = URI.ToString().Substring(index);
            FormattedName = Name.Replace('_', ' ').Replace('#', ' ').Trim();
            Type = game.GetElementType(URI);
            OriginElement = originElementUri;
            RelatedValue = propertyValue;
        }
        #endregion
    }
}
