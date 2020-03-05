namespace ARPEGOS.ViewModels
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using ARPEGOS.Models;
    public class ItemGroupViewModel : ObservableCollection<SimpleListItem>, INotifyPropertyChanged
    {
        private bool _expanded;
        public string Title { get; set; }

        public new event PropertyChangedEventHandler PropertyChanged;

        public ItemGroupViewModel(string groupTitle, bool expanded = false)
        {
            Title = groupTitle;
            Expanded = expanded;
        }

        public string StateIcon => Expanded ? "collapse_icon.png" : "expand_icon.png";

        public bool Expanded
        {
            get => this._expanded;
            set
            {
                if (_expanded != value)
                {
                    _expanded = value;
                    OnPropertyChanged("Expanded");
                    OnPropertyChanged("StateIcon");
                }
            }
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
