
namespace ARPEGOS.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using ARPEGOS.Helpers;
    using ARPEGOS.Services;
    using ARPEGOS.ViewModels.Base;
    using ARPEGOS.Views;

    using Xamarin.Essentials;
    using Xamarin.Forms;

    public class MainViewModel : BaseViewModel
    {
        private SelectionStatus _selectionStatus;

        private string selectedGame;

        public ICommand SelectItemCommand { get; }

        public ObservableCollection<string> SelectableElements { get; }

        public SelectionStatus CurrentStatus
        {
            get => this._selectionStatus;
            set
            {
                this.SetProperty(ref this._selectionStatus, value);
                this.OnPropertyChanged(nameof(this.Title));
            }
        }

        public new string Title
        {
            get
            {
                return this.CurrentStatus switch
                    {
                        SelectionStatus.SelectingGame => "Selecciona el juego",
                        SelectionStatus.SelectingVersion => "Selecciona la version",
                        SelectionStatus.SelectingCharacter => "Selecciona el personaje",
                        _ => "Carga completada!"
                    };
            }
        }

        public MainViewModel()
        {
            this.SelectableElements = new ObservableCollection<string>();
            this.SelectItemCommand = new Command<string>(s => Task.Factory.StartNew(async () => await this.SelectItem(s)));
            this.CurrentStatus = SelectionStatus.SelectingGame;
        }

        public async Task Init()
        {
            if (!base.initialized)
            {
                this.IsBusy = true;
                try
                {
                    this.CurrentStatus = SelectionStatus.SelectingGame;
                    this.Load(this.CurrentStatus);
                }
                finally
                {
                    this.IsBusy = false;
                    this.initialized = true;
                }
            }
        }

        private async Task SelectItem(string item)
        {
            if (this.IsBusy)
                return;

            this.IsBusy = true;
            switch (this.CurrentStatus)
            {
                case SelectionStatus.SelectingGame:
                    this.selectedGame = item;
                    this.CurrentStatus = SelectionStatus.SelectingVersion;
                    this.Load(this.CurrentStatus);
                    break;
                case SelectionStatus.SelectingVersion:
                    App.CurrentContext.CurrentGame = await OntologyService.LoadGame(this.selectedGame, item);
                    this.CurrentStatus = SelectionStatus.SelectingCharacter;
                    this.Load(this.CurrentStatus);
                    break;
                case SelectionStatus.SelectingCharacter:
                    if (item == string.Empty)
                    {
                        MainThread.BeginInvokeOnMainThread(
                            async() =>
                                {
                                    item = await (Application.Current.MainPage as MainView).Detail.DisplayPromptAsync("Crear nuevo personaje", "Introduce el nombre:");
                                });
                    }
                    App.CurrentContext.CurrentCharacter = await OntologyService.LoadCharacter(item, App.CurrentContext.CurrentGame);
                    this.CurrentStatus = SelectionStatus.Done;
                    this.Load(this.CurrentStatus);
                    break;
            }

            this.IsBusy = false;
        }

        private void Load(SelectionStatus status)
        {
            IEnumerable<string> items;
            switch (status)
            {
                case SelectionStatus.SelectingGame:
                    items = FileService.ListGames();
                    break;
                case SelectionStatus.SelectingVersion:
                    items = FileService.ListVersions(this.selectedGame);
                    break;
                case SelectionStatus.SelectingCharacter:
                    items = FileService.ListCharacters(this.selectedGame);
                    break;
                case SelectionStatus.Done:
                    items = new string[0];
                    break;
                default:
                    return;
            }
            MainThread.BeginInvokeOnMainThread(
                () =>
                    {
                        this.SelectableElements.Clear();
                        this.SelectableElements.AddRange(items);
                        if (status == SelectionStatus.SelectingCharacter)
                            this.SelectableElements.Add(string.Empty);
                    });
        }

        public enum SelectionStatus
        {
            SelectingGame,
            SelectingVersion,
            SelectingCharacter,
            Done
        }
    }
}
