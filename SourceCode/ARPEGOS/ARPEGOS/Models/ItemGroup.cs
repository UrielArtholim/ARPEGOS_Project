using ProjectSetup.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace ARPEGOS.Models
{
    public class ItemGroup : ObservableCollection<SimpleListItem>
    {
        private bool groupExpanded;
        public event PropertyChangedEventHandler ExpandedPropertyChanged;
        public string GroupName { get; set; }
        public ItemGroup( string name, bool expanded = false)
        {
            GroupName = name;
            groupExpanded = expanded;
        }
        public bool GroupExpanded
        {
            get 
            { 
                return groupExpanded; 
            }
            set 
            { 
                if (groupExpanded != value)
                    groupExpanded = value;
                OnPropertyChanged("Expanded");
            }
        }

        protected void OnPropertyChanged(string groupExpanded)
        {
            ExpandedPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(groupExpanded));
        }
    }
}
