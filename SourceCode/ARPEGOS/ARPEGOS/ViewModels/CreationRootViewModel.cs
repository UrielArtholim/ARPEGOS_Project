using ARPEGOS.Helpers;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class CreationRootViewModel: BaseViewModel
    {
        public ObservableCollection<Item> Data { get; private set; }

        public ICommand SelectItemCommand { get; private set; }

        public CreationRootViewModel()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            var rootStage = game.GetCreationSchemeRootClass();
            Data = new ObservableCollection<Item>(character.GetIndividuals(rootStage));

            this.SelectItemCommand = new Command<Item>(async (item) => 
            {
                var character = DependencyHelper.CurrentContext.CurrentCharacter;
                character.AddObjectProperty($"{character.Context}{character.Name}", $"{character.Context}{rootStage}", $"{character.Context}{item.Name}");

                await MainThread.InvokeOnMainThreadAsync(async() => await App.Navigation.NavigationStack.Last().Navigation.PushAsync(new StageView(0)));
                await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PopAsync());
            });
        }
    }
}
