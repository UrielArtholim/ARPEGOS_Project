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
    using Xamarin.Essentials;

    /// <summary>
    /// ItemGroup models a collection of items which belong to a group
    /// </summary>
    public class Group: ObservableCollection<Item>, INotifyPropertyChanged
    {
        #region Properties
        /// <summary>
        /// State of expansion of the group
        /// </summary>
        private bool expanded;
        private List<Item> elements;
        public List<Item> Elements
        {
            get => this.elements;
            set => SetProperty(ref this.elements, value);
        }

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
        /// EventHandler to know when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Graphic element which shows the state of expansion of the group
        /// </summary>
        public string StateIcon => Expanded ? "collapse.png" : "expand.png";

        public string Description { get; internal set; }

        public int Value { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Ctor that builds a group given the name and the state of expansion
        /// </summary>
        /// <param name="groupTitle">Name of the group</param>
        /// <param name="expanded">State of expansion of the group</param>
        public Group (string groupString, IEnumerable<Item> groupList = null, int groupValue = 0): base(groupList)
        {
            GroupString = groupString;
            Title = groupString.Split('#').Last();
            FormattedTitle = Title.Replace('_', ' ').Trim();
            Expanded = false;
            Value = groupValue;
            Description = DependencyHelper.CurrentContext.CurrentCharacter.GetElementDescription(groupString, StageViewModel.ApplyOnCharacter);
            if (string.IsNullOrEmpty(Description) || string.IsNullOrWhiteSpace(Description))
                this.HasDescription = false;
            else
                this.HasDescription = true;
            Elements = groupList.ToList();
        }

        public Group(Group g)
        {
            this.GroupString = g.GroupString;
            this.Title = g.Title;
            this.FormattedTitle = g.FormattedTitle;
            this.Description = g.Description;
            this.Elements = new List<Item>(g.Elements);
            this.Value = g.Value;
            this.Expanded = g.Expanded;            
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
                if(expanded == true)
                {
                    foreach(var item in this.Elements)
                        this.Add(item);
                }
                else
                    this.Clear();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            MainThread.BeginInvokeOnMainThread(() => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
