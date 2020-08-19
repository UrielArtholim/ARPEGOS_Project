namespace ARPEGOS.ViewModels
{
    using System.Collections.ObjectModel;

    using ARPEGOS.ViewModels.Base;
    using ARPEGOS.Views;

    public class CreationViewModel : BaseViewModel
    {
        public ObservableCollection<ItemListViewModel> Data { get; set; }

        public CreationViewModel()
        {
            this.Data = new ObservableCollection<ItemListViewModel>()
            {
                new ItemListViewModel(),
                new ItemListViewModel(),
                new ItemListViewModel(),
                new ItemListViewModel(),
                new ItemListViewModel(),
            };
            this.Title = "Creation";
        }
    }
}
