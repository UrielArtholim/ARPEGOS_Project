using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels.Templates
{
    public class StageSingleChoiceViewModel : BaseViewModel
    {
        private DialogService dialogService;
        private Stage currentStage;
        private string stageString;
        private string stageName;
        public Stage CurrentStage
        {
            get => currentStage;
            set => SetProperty(ref this.currentStage, value);
        }
        public string StageName
        {
            get => stageName;
            set => SetProperty(ref this.stageName, value);
        }
        private Item selectedItem;
        public Item SelectedItem
        {
            get => selectedItem;
            set => SetProperty(ref this.selectedItem, value);
        }

        public bool _continue;
        public bool Continue
        {
            get => _continue;
            set => SetProperty(ref this._continue, value);
        }

        public ObservableCollection<Item> Data { get; private set; }

        public ICommand NextCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand SelectCommand { get; private set; }

        public StageSingleChoiceViewModel(int stageNumber)
        {
            this.dialogService = new DialogService();
            this.Continue = false;
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;

            this.CurrentStage = StageViewModel.CreationScheme.ElementAt(stageNumber);
            this.StageName = FileService.FormatName(this.CurrentStage.ShortName);
            Data = new ObservableCollection<Item>(character.GetIndividuals(stageString));

            this.NextCommand = new Command(async () =>
            {
                this.IsBusy = true;
                var character = DependencyHelper.CurrentContext.CurrentCharacter;
                var currentItem = this.SelectedItem;
                var ItemFullShortName = this.SelectedItem.FullName.Split('#').Last();
                var predicateString = character.GetObjectPropertyAssociated(this.CurrentStage.ShortName);
                var predicateName = predicateString.Split('#').Last();

                character.UpdateObjectAssertion($"{character.Context}{predicateName}", $"{character.Context}{ItemFullShortName}");
                this.IsBusy = false;

                await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.NavigationStack.Last().Navigation.PushAsync(new StageView(++stageNumber)));
            });

            this.InfoCommand = new Command<Item>(async (item) =>
            {
                await this.dialogService.DisplayAlert(item.FormattedName, item.Description);
            });

        }
    }
}
