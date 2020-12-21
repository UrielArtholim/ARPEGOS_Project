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
        private bool _cancelEnabled;
        public bool CancelEnabled
        { 
            get => _cancelEnabled;
            set => this.SetProperty(ref this._cancelEnabled, value);
        }

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
                        SelectionStatus.SelectingGame => "Selecciona el juego",
                        SelectionStatus.DeletingGame => "Selecciona el juego",
                        SelectionStatus.SelectingCharacter => "Selecciona el personaje",
                        SelectionStatus.DeletingCharacter => "Selecciona el personaje",
                        _ => "Carga completada!"
                    };
            }
        }
        public MainViewModel ()
        {
            this.SelectableElements = new ObservableCollection<string>();
            this.SelectItemCommand = new Command<string>(s => Task.Factory.StartNew(async () => await this.SelectItem(s)));
            this.CurrentStatus = SelectionStatus.SelectingGame;
            this.PreviousStatus = this.CurrentStatus;
            this.dialogService = DependencyHelper.CurrentContext.Dialog;
            this.CancelEnabled = false;
            this.AddButtonCommand = new Command(async() => 
            {
                switch(this.CurrentStatus)
                {
                    case SelectionStatus.SelectingGame:
                        await Device.InvokeOnMainThreadAsync(() => this.IsBusy = true);
                        await App.Navigation.PushAsync(new AddGameView());
                        await Device.InvokeOnMainThreadAsync(() => this.IsBusy = false);
                        this.Load(this.CurrentStatus);
                        break;
                    case SelectionStatus.DeletingGame:
                        await Device.InvokeOnMainThreadAsync(() => this.IsBusy = true);
                        await App.Navigation.PushAsync(new AddGameView());
                        await Device.InvokeOnMainThreadAsync(() => this.IsBusy = false);
                        this.Load(this.CurrentStatus);
                        break;
                    case SelectionStatus.SelectingCharacter:
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

            this.CancelButtonCommand = new Command(() =>
            { 
                this.CurrentStatus = this.PreviousStatus;  
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
                        item = await this.dialogService.DisplayTextPrompt("Crear nuevo personaje", "Introduce el nombre:", "Crear");
                        if (string.IsNullOrWhiteSpace(item))
                            break;
                        await Device.InvokeOnMainThreadAsync(() => this.IsBusy = true);
                        DependencyHelper.CurrentContext.CurrentCharacter = await OntologyService.CreateCharacter(item, DependencyHelper.CurrentContext.CurrentGame);
                        App.Current.MainPage = new NavigationPage(new CreationRootView());
                        App.Navigation = App.Current.MainPage.Navigation;
                        await Device.InvokeOnMainThreadAsync(() => this.IsBusy = false);

                    }
                    else
                    {
                        DependencyHelper.CurrentContext.CurrentCharacter = await OntologyService.LoadCharacter(item, DependencyHelper.CurrentContext.CurrentGame);
                        await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new OptionsView()));
                    }
                    this.Load(this.CurrentStatus);
                    break;

                case SelectionStatus.DeletingGame:
                    this.CancelEnabled = true;
                    var confirmation = await this.dialogService.DisplayAcceptableAlert("Advertencia", $"¿Desea eliminar {item}? Una vez hecho no podrá ser recuperado", "Confirmar", "Cancelar");
                    if (confirmation == true)
                        await Device.InvokeOnMainThreadAsync(()=> FileService.DeleteGame(this.selectedGame));
                    this.Load(CurrentStatus);
                    break;                    

                case SelectionStatus.DeletingCharacter:
                    this.CancelEnabled = true;
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
                    this.CancelEnabled = false;
                    items = FileService.ListGames();
                    break;
                case SelectionStatus.SelectingGame:
                    this.CancelEnabled = false;
                    items = FileService.ListGames();
                    break;
                case SelectionStatus.DeletingGame:
                    this.CancelEnabled = true;
                    updatedItems = FileService.ListGames().ToList();
                    if (updatedItems.Count() == 0)
                        this.CurrentStatus = SelectionStatus.SelectingGame;
                    items = updatedItems;
                    break;
                case SelectionStatus.SelectingCharacter:
                    this.CancelEnabled = false;
                    items = FileService.ListCharacters(this.SelectedGame);
                    break;
                case SelectionStatus.DeletingCharacter:
                    this.CancelEnabled = true;
                    updatedItems = FileService.ListCharacters(this.SelectedGame).ToList();
                    if (updatedItems.Count() == 0)
                        this.CurrentStatus = SelectionStatus.SelectingCharacter;
                    items = updatedItems;
                    break;
                case SelectionStatus.Done:
                    this.CancelEnabled = false;
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
                    this.CancelEnabled = false;
                    this.SelectableElements.AddRange(items);
                    if (status == SelectionStatus.SelectingCharacter)
                        this.SelectableElements.Add(string.Empty);
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
