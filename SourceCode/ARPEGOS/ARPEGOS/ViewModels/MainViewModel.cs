
namespace ARPEGOS.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using ARPEGOS.Helpers;
    using ARPEGOS.Services;
    using ARPEGOS.Services.Interfaces;
    using ARPEGOS.ViewModels.Base;
    using ARPEGOS.Views;

    using Xamarin.Essentials;
    using Xamarin.Forms;

    public class MainViewModel : BaseViewModel
    {
        private SelectionStatus _selectionStatus;

        private string selectedGame;

        private IDialogService dialogService;

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

        public MainViewModel(IDialogService dialogService)
        {
            this.SelectableElements = new ObservableCollection<string>();
            this.SelectItemCommand = new Command<string>(s => Task.Factory.StartNew(async () => await this.SelectItem(s)));
            this.CurrentStatus = SelectionStatus.SelectingGame;
            this.dialogService = dialogService;
            this.AddGameButtonCommand = new Command(async () => { await App.Navigation.PushAsync(new AddGameView()); });
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
                    DependencyHelper.CurrentContext.CurrentGame = await OntologyService.LoadGame(this.selectedGame, item);
                    this.CurrentStatus = SelectionStatus.SelectingCharacter;
                    this.Load(this.CurrentStatus);
                    break;
                case SelectionStatus.SelectingCharacter:
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        item = await this.dialogService.DisplayTextPrompt("Crear nuevo personaje", "Introduce el nombre:", "Crear");
                        if (string.IsNullOrWhiteSpace(item))
                            break;

                        DependencyHelper.CurrentContext.CurrentCharacter = await OntologyService.CreateCharacter(item, DependencyHelper.CurrentContext.CurrentGame);
                    }
                    else
                    {
                        DependencyHelper.CurrentContext.CurrentCharacter = await OntologyService.LoadCharacter(item, DependencyHelper.CurrentContext.CurrentGame);
                    }
                    
                    this.CurrentStatus = SelectionStatus.Done;
                    this.Load(this.CurrentStatus);
                    break;
                default:
                    this.CurrentStatus = SelectionStatus.SelectingGame;
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
                    #if DEBUG
                    items = new List<string> { "Volver a empezar" };
                    #endif
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

        public ICommand AddGameButtonCommand { get; set; }
    }
}
