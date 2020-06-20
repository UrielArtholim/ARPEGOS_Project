using System;
using System.ComponentModel;

namespace Arpegos_Test
{
    /// <summary>
    /// ItemGroup models a collection of items which belong to a group
    /// </summary>
    public class Group : INotifyPropertyChanged
    {
        #region Properties
        /// <summary>
        /// State of expansion of the group
        /// </summary>
        private bool expanded;

        /// <summary>
        /// Name of the group
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Name of the group formatted for UI
        /// </summary>
        public string FormattedTitle { get; internal set; }

        /// <summary>
        /// Collection of elements of the group
        /// </summary>
        public dynamic GroupList { get; internal set; }

        /// <summary>
        /// EventHandler to know when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Graphic element which shows the state of expansion of the group
        /// </summary>
        public string StateIcon => Expanded ? "collapse_icon.png" : "expand_icon.png";

        public string Description { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Ctor that builds a group given the name and the state of expansion
        /// </summary>
        /// <param name="groupTitle">Name of the group</param>
        /// <param name="expanded">State of expansion of the group</param>
        public Group(string groupTitle, bool expanded = false)
        {
            Title = groupTitle;
            FormattedTitle = groupTitle.Replace('_', ' ').Trim();
            Expanded = expanded;
            Description = Program.Game.GetElementDescription(groupTitle);
            GroupList = Program.Game.GetIndividualsGrouped(groupTitle);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Check the state of expansion of the group
        /// </summary>
        public bool Expanded
        {
            get => this.expanded;
            set
            {
                if (expanded != value)
                {
                    expanded = value;
                    OnPropertyChanged("Expanded");
                    OnPropertyChanged("StateIcon");
                }
            }
        }

        /// <summary>
        /// Notify the change of a property to the handler
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ShowGroup(string tree = " ")
        {
            Console.WriteLine(tree + "====[" + this.FormattedTitle + "]");
            
            string nextElement = tree + "----";

            int textcounter = ((this.FormattedTitle.Length / 2) + this.FormattedTitle.Length % 2)+4;
            for(int i = ((this.FormattedTitle.Length / 2) + this.FormattedTitle.Length % 2); i > 0; --i)
            {
                nextElement += "-";
                --textcounter;
            }
            nextElement += "|";

            Console.WriteLine(nextElement +"\n" + nextElement);
            foreach (var element in GroupList)
            {
                Type elementType = element.GetType();
                Type groupType = this.GetType();
                if(elementType == groupType)
                {
                    Group currentGroup = (Group)element;
                    currentGroup.ShowGroup(nextElement);
                }
                else 
                {
                    Item currentItem = (Item) element;
                    currentItem.ShowItem(nextElement);
                }
            }
            Console.WriteLine(tree);
            Console.WriteLine(tree);
        }
        #endregion

    }
}
