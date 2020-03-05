using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Models
{
    public class IndividualElement : ClassElement
    {
        public List<ObjectPropertyElement> ObjectPropertiesList { get; private set; }
        public List<DatatypePropertyElement> DatatypePropertiesList { get; private set; }

        public IndividualElement(string elementUri) : base(elementUri)
        {
            var game = SystemControl.ActiveGame;
            URI = new Uri(game.GameGraph.Context + elementUri);
            int index = URI.ToString().LastIndexOf('#');
            Name = URI.ToString().Substring(index);
            FormattedName = Name.Replace('_', ' ').Replace('#', ' ').Trim();
            Type = game.GetElementType(URI);
            ObjectPropertiesList = game.GetRelatedObjectProperties(this.URI);
            DatatypePropertiesList = game.GetRelatedDatatypeProperties(this.URI);
        }
    }
}
