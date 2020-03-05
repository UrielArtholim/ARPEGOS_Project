namespace ARPEGOS.Models
{
    using RDFSharp.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public abstract class GameElement
    {
        #region Properties
        public Uri URI { get; protected set; }
        public string Name { get; protected set; }
        public string FormattedName { get; protected set; }
        public RDFResource Type { get; protected set; }

        /*public Uri Class { get; private set; }
        public string Description { get; private set; }
        public Uri RelatedElement { get; set; }
        public float RelatedFloatValue { get; set; }
        public int RelatedIntegerValue { get; set; }
        */
        #endregion

        /*#region Constructors

        public GameElement(string elementUri)
        {
            URI = new Uri(SystemControl.ActiveGame.GameGraph.Context+elementUri);
            int index = URI.ToString().LastIndexOf('#');
            Name = URI.ToString().Substring(index);
            FormattedName = Name.Replace('_', ' ').Replace("#Per ","").Replace('#', ' ').Trim();
            Type = SystemControl.ActiveGame.GetElementType(URI);
            Console.WriteLine("Type = " + Type.URI.ToString());
            if (Type == RDFVocabulary.OWL.INDIVIDUAL || Type == RDFVocabulary.OWL.CLASS)
            {
                Class = new Uri(SystemControl.ActiveGame.GetElementClass(URI));
                Description = SystemControl.ActiveGame.GetElementDescription(this);
            }              
        }

        #endregion
        */

        #region Methods
        public SimpleListItem ConvertToSimpleListItem()
        {
            return new SimpleListItem(this.FormattedName);
        }
        #endregion

    }
}
