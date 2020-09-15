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

namespace ARPEGOS.ViewModels
{
    public class MultipleChoiceViewModel: BaseViewModel
    {
        private DialogService dialogService;
        private Stage currentStage;
        private string stageString;
        private string stageName;

        private double stageLimit, stageProgress, stageProgressLabel;
        private double generalLimit, generalProgress, generalProgressLabel;
        private bool hasGeneralLimit;

        private List<Item> selectedItems;
        public List<Item> SelectedItems
        {
            get => this.selectedItems;
            set => SetProperty(ref this.selectedItems, value);
        }

        public bool _continue;
        public bool Continue
        {
            get => _continue;
            set => SetProperty(ref this._continue, value);
        }

        public Stage CurrentStage
        {
            get => this.currentStage;
            set => SetProperty(ref this.currentStage, value);
        }

        public string StageName
        {
            get => this.stageName;
            set => SetProperty(ref this.stageName, value);
        }

        public double StageLimit
        {
            get => this.stageLimit;
            set => SetProperty(ref this.stageLimit, value);
        }

        public double StageProgress
        {
            get => this.stageProgress;
            set => SetProperty(ref this.stageProgress, value);
        }

        public double StageProgressLabel
        {
            get => this.stageProgressLabel;
            set => SetProperty(ref this.stageProgressLabel, value);
        }

        public double GeneralLimit
        {
            get => this.generalLimit;
            set => SetProperty(ref this.generalLimit, value);
        }

        public double GeneralProgress
        {
            get => this.generalProgress;
            set => SetProperty(ref this.generalProgress, value);
        }

        public double GeneralProgressLabel
        {
            get => this.generalProgressLabel;
            set => SetProperty(ref this.generalProgressLabel, value);
        }

        public bool HasGeneralLimit
        {
            get => this.hasGeneralLimit;
            set => SetProperty(ref this.hasGeneralLimit, value);
        }

        public ObservableCollection<Item> Data { get; private set; }

        public ICommand NextCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand SelectCommand { get; private set; }

        public MultipleChoiceViewModel()
        {
            this.dialogService = new DialogService();
            this.Continue = false;
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            this.stageString = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep).FullName;
            this.StageName = this.stageString.Split('#').Last();

            var datalist = new ObservableCollection<Item>(character.GetIndividuals(stageString));
            foreach (var item in datalist)
                if (string.IsNullOrEmpty(item.Description) || string.IsNullOrWhiteSpace(item.Description))
                    item.HasDescription = false;

            Data = new ObservableCollection<Item>(datalist);

            this.NextCommand = new Command(async () =>
            {
                this.IsBusy = true;
                var character = DependencyHelper.CurrentContext.CurrentCharacter;

                foreach(var item in this.SelectedItems)
                {
                    var ItemFullShortName = item.FullName.Split('#').Last();
                    var predicateString = character.GetObjectPropertyAssociated(this.stageString);
                    var predicateName = predicateString.Split('#').Last();
                    character.UpdateObjectAssertion($"{character.Context}{predicateName}", $"{character.Context}{ItemFullShortName}");
                }
                
                ++StageViewModel.CurrentStep;
                var nextStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);

                this.IsBusy = false;
                if (nextStage.IsGrouped)
                {
                    switch (nextStage.Type)
                    {
                        case Stage.StageType.SingleChoice: await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new SingleChoiceGroupView())); break;
                        case Stage.StageType.MultipleChoice: await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new MultipleChoiceGroupView())); break;
                        default: await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new ValuedGroupView())); break;
                    }
                }
                else
                {
                    switch (nextStage.Type)
                    {
                        case Stage.StageType.SingleChoice: await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new SingleChoiceView())); break;
                        case Stage.StageType.MultipleChoice: await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new MultipleChoiceView())); break;
                        default: await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new ValuedView())); break;
                    }
                }
            });

            this.InfoCommand = new Command<Item>(async (item) =>
            {
                await this.dialogService.DisplayAlert(item.FormattedName, item.Description);
            });
        }
    }
}
