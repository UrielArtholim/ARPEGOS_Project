using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace ARPEGOS.Models
{
    public class ItemGroup : ObservableCollection<SimpleListItem>, INotifyPropertyChanged
    {
        private bool groupExpanded;
        public string groupName { get; set; }
        public ItemGroup( string name, bool expanded = false)
        {
            groupName = name;
            groupExpanded = expanded;
        }
    }
}
