using ARPEGOS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public class ItemListViewModel
    {
        public ObservableCollection<CheckItemViewModel> Data { get; set; }
        public ItemListViewModel()
        {
            Data = new ObservableCollection<CheckItemViewModel>
            {
                new CheckItemViewModel( new Item("Item 1")),
                new CheckItemViewModel( new Item("Item 2")),
                new CheckItemViewModel( new Item("Item 3")),
                new CheckItemViewModel( new Item("Item 4")),
                new CheckItemViewModel( new Item("Item 5")),
                new CheckItemViewModel( new Item("Item 6")),
                new CheckItemViewModel( new Item("Item 7")),
                new CheckItemViewModel( new Item("Item 8")),
                new CheckItemViewModel( new Item("Item 9")),
                new CheckItemViewModel( new Item("Item 10"))
            };
        }
    }
}
