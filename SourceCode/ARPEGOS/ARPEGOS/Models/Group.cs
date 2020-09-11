using System;
using System.ComponentModel;

namespace ARPEGOS
{
    using ARPEGOS.Helpers;
    using ARPEGOS.ViewModels;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ItemGroup models a collection of items which belong to a group
    /// </summary>
    public class Group: INotifyPropertyChanged
    {
        #region Properties
        /// <summary>
        /// State of expansion of the group
        /// </summary>
        private bool expanded;

        /// <summary>
        /// 
        /// </summary>
        public bool HasDescription { get; set; }

        /// <summary>
        /// String of the group
        /// </summary>
        public string GroupString { get; internal set; }
        
        /// <summary>
        /// Short name of the group
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Name of the group formatted for UI
        /// </summary>
        public string FormattedTitle { get; internal set; }

        /// <summary>
        /// Collection of elements of the group
        /// </summary>
        public IEnumerable<Item> GroupList { get; internal set; }

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
        public Group (string groupString, IEnumerable<Item> groupList = null)
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            GroupString = groupString;
            Title = groupString.Split('#').Last();
            FormattedTitle = Title.Replace('_', ' ').Trim();
            Expanded = false;
            Description = character.GetElementDescription(groupString, StageViewModel.ApplyOnCharacter);
            if (string.IsNullOrEmpty(Description) || string.IsNullOrWhiteSpace(Description))
                this.HasDescription = false;
            else
                this.HasDescription = true;
            GroupList = groupList;
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
        protected virtual void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
