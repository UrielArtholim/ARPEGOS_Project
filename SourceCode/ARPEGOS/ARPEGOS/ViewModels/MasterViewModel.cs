using ARPEGOS.Helpers;
using ARPEGOS.Services;
using ARPEGOS.Themes;
using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class MasterViewModel: BaseViewModel
    {
        private DialogService dialogService;
        private ObservableCollection<string> themes;
        public ObservableCollection<string> Themes
        {
            get => this.themes;
            set => SetProperty(ref this.themes, value);
        }
        public ICommand HomeCommand { get; private set; }
        public ICommand ShowCharactersCommand { get; private set; }
        public ICommand SelectThemeCommand { get; private set; }
        public MasterViewModel()
        {
            this.dialogService = new DialogService();
            var themeList = new List<string>(DependencyHelper.CurrentContext.Themes.BackgroundThemes.Keys);
            this.Themes = new ObservableCollection<string>(themeList);
            this.HomeCommand = new Command(async() =>
            {
                var navigationStack = App.Current.MainPage.Navigation.NavigationStack;
                if(navigationStack.Count > 0)
                {
                   var currentPage = App.Current.MainPage.Navigation.NavigationStack.Last() as NavigationPage;
                   await currentPage.PopToRootAsync();
                }
                var mainPage = App.Current.MainPage as MasterDetailPage;
                mainPage.IsPresented = false;
            });

            this.ShowCharactersCommand = new Command(async() =>
            {
                if (DependencyHelper.CurrentContext.CurrentGame != null)
                {
                    await App.Current.MainPage.Navigation.PopToRootAsync();
                    var viewModel = App.Current.MainPage.Navigation.NavigationStack[0].BindingContext as MainViewModel;
                    viewModel.Load(MainViewModel.SelectionStatus.SelectingGame);
                }
                else
                    await this.dialogService.DisplayAlert("Error", "No hay ningún juego seleccionado. Por favor, seleccione un juego");                
            });

            this.SelectThemeCommand = new Command(async() => 
            {
                await App.Current.MainPage.Navigation.PushModalAsync(new ThemeSelectionView());
            });
        }
    }
}
