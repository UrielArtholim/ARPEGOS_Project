namespace ARPEGOS.ViewModels
{
    using System.Collections.ObjectModel;

    using ARPEGOS.ViewModels.Base;

    public class CreationViewModel : BaseViewModel
    {
        public ObservableCollection<string> Data { get; set; }

        public CreationViewModel()
        {
            this.Data = new ObservableCollection<string>() { "primer", "segundo", "tercero" };
            this.Title = "Creation";
        }
    }
}
