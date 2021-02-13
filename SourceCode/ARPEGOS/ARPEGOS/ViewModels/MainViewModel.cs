namespace ARPEGOS.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using ARPEGOS.Converter;
    using ARPEGOS.Helpers;
    using ARPEGOS.Models;
    using ARPEGOS.Services;
    using ARPEGOS.Services.Interfaces;
    using ARPEGOS.ViewModels.Base;
    using ARPEGOS.Views;

    using Xamarin.Essentials;
    using Xamarin.Forms;

    public class MainViewModel : BaseViewModel
    {
        private SelectionStatus _selectionStatus,_previousStatus;
        private string selectedGame;
        private IDialogService dialogService;
        private bool deleteMode;

        public ICommand AddButtonCommand { get; set; }
        public ICommand ClearCheckCommand { get; }
        public ICommand DeleteButtonCommand { get; }
        public ICommand CancelButtonCommand { get; }
        public ICommand SelectItemCommand { get; }
        public ICommand PushSkillViewCommand { get; }
        public ICommand LoadNewStateCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        public string SelectedGame
        {
            get => this.selectedGame;
            set => this.SetProperty(ref this.selectedGame, value);
        }

        public bool DeleteMode
        {
            get => this.deleteMode;
            set => this.SetProperty(ref this.deleteMode, value);
        }

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

        public SelectionStatus PreviousStatus
        {
            get => this._previousStatus;
            set
            {
                this.SetProperty(ref this._previousStatus, value);
                this.OnPropertyChanged(nameof(this.Title));
            }
        }

        public new string Title
        {
            get
            {
                return this.CurrentStatus switch
                    {
                        SelectionStatus.SelectingGame => "Inicio",
                        SelectionStatus.DeletingGame => "Inicio",
                        SelectionStatus.SelectingCharacter => "Selección de Personaje",
                        SelectionStatus.DeletingCharacter => "Selección de Personaje",
                        _ => "Unknown Status"
                    };
            }
        }


        public MainViewModel ()
        {
            this.DeleteMode = false;
            this.SelectableElements = new ObservableCollection<string>();
            this.SelectItemCommand = new Command<string>(s => Task.Factory.StartNew(async () => await this.SelectItem(s)));
            this.CurrentStatus = SelectionStatus.SelectingGame;
            this.PreviousStatus = this.CurrentStatus;
            this.dialogService = DependencyHelper.CurrentContext.Dialog;
            this.AddButtonCommand = new Command(async() => 
            {
                switch(this.CurrentStatus)
                {
                    case SelectionStatus.SelectingGame:
                        DeleteMode = false;
                        await Device.InvokeOnMainThreadAsync(() => this.IsBusy = true);
                        await App.Navigation.PushAsync(new AddGameView());
                        await Device.InvokeOnMainThreadAsync(() => this.IsBusy = false);
                        this.Load(this.CurrentStatus);
                        break;
                    case SelectionStatus.DeletingGame:
                        DeleteMode = false;
                        await Device.InvokeOnMainThreadAsync(() => this.IsBusy = true);
                        await App.Navigation.PushAsync(new AddGameView());
                        await Device.InvokeOnMainThreadAsync(() => this.IsBusy = false);
                        this.Load(this.CurrentStatus);
                        break;
                    case SelectionStatus.SelectingCharacter:
                        DeleteMode = false;
                        var item = await this.dialogService.DisplayTextPrompt("Crear nuevo personaje", "Introduce el nombre:", "Crear");
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            await Device.InvokeOnMainThreadAsync(()=> this.IsBusy = true);
                            DependencyHelper.CurrentContext.CurrentCharacter = await OntologyService.CreateCharacter(item, DependencyHelper.CurrentContext.CurrentGame);
                            App.Current.MainPage = new NavigationPage(new CreationRootView());
                            App.Navigation = App.Current.MainPage.Navigation;
                            await Device.InvokeOnMainThreadAsync(() => this.IsBusy = false);
                        }
                        this.Load(this.CurrentStatus);
                        break;
                    case SelectionStatus.DeletingCharacter:
                        DeleteMode = false;
                        item = await this.dialogService.DisplayTextPrompt("Crear nuevo personaje", "Introduce el nombre:", "Crear");
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            await Device.InvokeOnMainThreadAsync(() => this.IsBusy = true);
                            DependencyHelper.CurrentContext.CurrentCharacter = await OntologyService.CreateCharacter(item, DependencyHelper.CurrentContext.CurrentGame);
                            App.Current.MainPage = new NavigationPage(new CreationRootView());
                            App.Navigation = App.Current.MainPage.Navigation;
                            await Device.InvokeOnMainThreadAsync(() => this.IsBusy = false);
                        }
                        this.Load(this.CurrentStatus);
                        break;
                }
            });

            this.DeleteButtonCommand = new Command(() => 
            {
                this.PreviousStatus = this.CurrentStatus;
                switch(this.PreviousStatus)
                {
                    case SelectionStatus.SelectingGame: this.CurrentStatus = SelectionStatus.DeletingGame; break;
                    case SelectionStatus.SelectingCharacter: this.CurrentStatus = SelectionStatus.DeletingCharacter; break;
                    case SelectionStatus.DeletingGame: this.CurrentStatus = SelectionStatus.SelectingGame; break;
                    case SelectionStatus.DeletingCharacter: this.CurrentStatus = SelectionStatus.SelectingCharacter; break;
                }
                this.Load(CurrentStatus); 
            });


            this.LoadNewStateCommand = new Command<SelectionStatus>(status => {this.PreviousStatus = this.CurrentStatus; this.CurrentStatus = status;  this.Load(status); });

            this.ExportCommand = new Command(async () =>
            {
                if (DependencyHelper.CurrentContext.CurrentGame != null)
                    await FileService.ExportCharacters(DependencyHelper.CurrentContext.CurrentGame);
            });
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

                    this.SelectedGame = item;
                    Device.BeginInvokeOnMainThread(async() => await App.Navigation.PushAsync(new SelectVersionView(this.SelectedGame)));
                    this.PreviousStatus = this.CurrentStatus;
                    this.CurrentStatus = SelectionStatus.SelectingGame;
                    this.Load(this.CurrentStatus);
                    break;

                case SelectionStatus.SelectingCharacter:
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        DeleteMode = false;
                        var characterName = await this.dialogService.DisplayTextPrompt("Crear nuevo personaje", "Introduce el nombre:", "Crear");
                        if (!string.IsNullOrWhiteSpace(characterName))
                        {
                            await Device.InvokeOnMainThreadAsync(() => this.IsBusy = true);
                            DependencyHelper.CurrentContext.CurrentCharacter = await OntologyService.CreateCharacter(characterName, DependencyHelper.CurrentContext.CurrentGame);
                            App.Current.MainPage = new NavigationPage(new CreationRootView());
                            App.Navigation = App.Current.MainPage.Navigation;
                            await Device.InvokeOnMainThreadAsync(() => this.IsBusy = false);
                        }
                        this.Load(this.CurrentStatus);
                        break;
                    }
                    else
                    {
                        DependencyHelper.CurrentContext.CurrentCharacter = await OntologyService.LoadCharacter(item, DependencyHelper.CurrentContext.CurrentGame);
                        await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new OptionsView()));
                    }
                    this.Load(this.CurrentStatus);
                    break;

                case SelectionStatus.DeletingGame:
                    var confirmation = await this.dialogService.DisplayAcceptableAlert("Advertencia", $"¿Desea eliminar {item}? Una vez hecho no podrá ser recuperado", "Confirmar", "Cancelar");
                    if (confirmation == true)
                        await Device.InvokeOnMainThreadAsync(()=> FileService.DeleteGame(this.selectedGame));
                    this.Load(CurrentStatus);
                    break;                    

                case SelectionStatus.DeletingCharacter:
                    confirmation = await this.dialogService.DisplayAcceptableAlert("Advertencia", $"¿Desea eliminar {item}? Una vez hecho no podrá ser recuperado", "Confirmar", "Cancelar");
                    if(confirmation == true)
                        await Device.InvokeOnMainThreadAsync(() => FileService.DeleteCharacter(item, this.selectedGame));                    
                    this.Load(this.CurrentStatus);
                    break;


                default:
                    this.PreviousStatus = this.CurrentStatus;
                    this.CurrentStatus = SelectionStatus.SelectingGame;
                    this.Load(this.CurrentStatus);
                    break;
            }

            this.IsBusy = false;
        }

        public void Load(SelectionStatus status)
        {
            IEnumerable<string> items;
            List<string> updatedItems;
            switch (status)
            {
                case SelectionStatus.AddGame:
                    DeleteMode = false;
                    items = FileService.ListGames();
                    break;
                case SelectionStatus.SelectingGame:
                    DeleteMode = false;
                    items = FileService.ListGames();
                    break;
                case SelectionStatus.DeletingGame:
                    DeleteMode = true;
                    updatedItems = FileService.ListGames().ToList();
                    if (updatedItems.Count() == 0)
                        this.CurrentStatus = SelectionStatus.SelectingGame;
                    items = updatedItems;
                    break;
                case SelectionStatus.SelectingCharacter:
                    DeleteMode = false;
                    items = FileService.ListCharacters(this.SelectedGame);
                    break;
                case SelectionStatus.DeletingCharacter:
                    DeleteMode = true;
                    updatedItems = FileService.ListCharacters(this.SelectedGame).ToList();
                    if (updatedItems.Count() == 0)
                        this.CurrentStatus = SelectionStatus.SelectingCharacter;
                    items = updatedItems;
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
            Device.BeginInvokeOnMainThread(() =>
            {
                if(items != null)
                {
                    this.SelectableElements.Clear();
                    this.SelectableElements.AddRange(items);
                    // Uncomment to get "Crear" option in Character selection
                    //if (status == SelectionStatus.SelectingCharacter)
                    //    this.SelectableElements.Add(string.Empty);
                }
            });
        }

        public enum SelectionStatus
        {
            AddGame,
            AddCharacter,
            SelectingGame,
            SelectingVersion,
            SelectingCharacter,
            DeletingGame,
            DeletingCharacter,
            Done
        }
    }
}
