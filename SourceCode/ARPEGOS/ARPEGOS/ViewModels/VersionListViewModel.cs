using ARPEGOS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ARPEGOS.ViewModels
{
    class VersionListViewModel
    {
        public ObservableCollection<SimpleListItem> gameVersions { get; private set; }

        public VersionListViewModel()
        {
            gameVersions = new ObservableCollection<SimpleListItem>();

        }
    }
}
