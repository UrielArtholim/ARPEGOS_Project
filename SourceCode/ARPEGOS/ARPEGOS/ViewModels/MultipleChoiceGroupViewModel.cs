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
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class MultipleChoiceGroupViewModel: BaseViewModel
    {
        #region Properties
        #region Private
        private DialogService dialogService = new DialogService();
        private ObservableCollection<Group> data;
        private Stage currentStage;
        private string stageName, stageLimitProperty;
        private double stageLimit, stageProgress, stageProgressLabel;
        private double generalLimit, generalProgress, generalProgressLabel;
        private double currentLimit;
        private bool hasGeneralLimit, hasStageLimit, showDescription;
        private Group lastGroup;

        #endregion

        #region Public
        public ObservableCollection<Group> Data
        {
            get => this.data;
            set => SetProperty(ref this.data, value);
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

        public double CurrentLimit
        {
            get => this.currentLimit;
            set => SetProperty(ref this.currentLimit, value);
        }

        public bool HasGeneralLimit
        {
            get => this.hasGeneralLimit;
            set => SetProperty(ref this.hasGeneralLimit, value);
        }
        public bool HasStageLimit
        {
            get => this.hasStageLimit;
            set => SetProperty(ref this.hasStageLimit, value);
        }

        public bool ShowDescription
        {
            get => this.showDescription;
            set => SetProperty(ref this.showDescription, value);
        }
        #endregion
        #endregion

        #region Commands
        public ICommand NextCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand GroupInfoCommand { get; private set; }
        public ICommand SelectCommand { get; private set; }
        public ICommand SelectGroupCommand { get; private set; }
        #endregion

        #region Constructor

        public MultipleChoiceGroupViewModel()
        {
            this.dialogService = new DialogService();
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            this.CurrentStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
            var stageString = this.CurrentStage.FullName;
            this.StageName = FileService.FormatName(stageString.Split('#').Last());
            this.stageLimitProperty = character.GetLimit(this.CurrentStage.FullName.Split('#').Last(), false, this.CurrentStage.EditGeneralLimit);
            this.StageLimit = character.GetLimitValue(this.stageLimitProperty);
            this.StageProgressLabel = this.StageLimit;
            this.StageProgress = 1;
            this.ShowDescription = true;
            this.HasGeneralLimit = this.CurrentStage.EditGeneralLimit;
            this.hasStageLimit = true;

            if (StageViewModel.GeneralLimitProperty == null && StageName != "Nivel")
            {
                StageViewModel.GeneralLimitProperty = character.GetLimit(stageString, true);
                StageViewModel.GeneralLimit = character.GetLimitValue(StageViewModel.GeneralLimitProperty);
                StageViewModel.GeneralProgress = 1;
            }

            if (this.HasGeneralLimit == true)
            {
                this.HasGeneralLimit = true;
                this.GeneralLimit = StageViewModel.GeneralLimit;
                this.GeneralProgress = StageViewModel.GeneralProgress;
                this.GeneralProgressLabel = this.GeneralLimit;
                this.CurrentLimit = Math.Min(this.GeneralLimit, this.StageLimit);
            }

            var datalist = new ObservableCollection<Group>(character.GetIndividualsGrouped(this.CurrentStage.FullName));
            foreach (var item in datalist)
                if (string.IsNullOrEmpty(item.Description) || string.IsNullOrWhiteSpace(item.Description))
                    item.HasDescription = false;

            Data = new ObservableCollection<Group>(datalist);

            this.NextCommand = new Command(async () =>
            {
                this.IsBusy = true;
                var character = DependencyHelper.CurrentContext.CurrentCharacter;

                if (this.CurrentStage.EditStageLimit)
                {
                    var characterStageLimitProperty = $"{character.Context}{this.stageLimitProperty}";
                    character.UpdateDatatypeAssertion(characterStageLimitProperty, Convert.ToString(Convert.ToInt32(this.StageProgressLabel)));
                }

                if (this.CurrentStage.EditGeneralLimit)
                {
                    var characterStageLimitProperty = $"{character.Context}{StageViewModel.GeneralLimitProperty}";
                    character.UpdateDatatypeAssertion(characterStageLimitProperty, Convert.ToString(Convert.ToInt32(this.GeneralProgressLabel)));
                    StageViewModel.GeneralLimit = this.GeneralProgressLabel;
                    StageViewModel.GeneralProgress = this.GeneralProgress;
                }

                this.IsBusy = false;

                ++StageViewModel.CurrentStep;
                if (StageViewModel.CurrentStep < StageViewModel.CreationScheme.Count())
                {
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
                }
                else
                {
                    await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PopAsync());
                }
            });


            this.SelectGroupCommand = new Command<Group>(async (group) => await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (lastGroup == group)
                {
                    group.Expanded = !group.Expanded;
                    UpdateGroup(group);
                }
                else
                {
                    if (lastGroup != null)
                    {
                        lastGroup.Expanded = false;
                        UpdateGroup(lastGroup);
                    }
                    group.Expanded = true;
                    UpdateGroup(group);
                }
                lastGroup = group;
            }));

            this.InfoCommand = new Command<Item>(async (item) =>
            {
                await this.dialogService.DisplayAlert(item.FormattedName, item.Description);
            });

            this.GroupInfoCommand = new Command<Group>(async (group) =>
            {
                await this.dialogService.DisplayAlert(group.FormattedTitle, group.Description);
            });
        }
        #endregion

        #region Methods
        public async Task UpdateView()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var availableItems = await Task.Run(() => character.CheckAvailableOptions(this.CurrentStage.FullName, this.CurrentStage.EditGeneralLimit, StageViewModel.GeneralLimit, this.StageLimit));
            foreach (var group in Data)
                foreach(var element in group)
                    element.IsEnabled = false;

            foreach (var group in Data)
            {
                foreach (var element in group)
                    foreach (var item in availableItems)
                        if (element.ShortName == item.ShortName)
                            element.IsEnabled = true;
                UpdateGroup(group);
            }
            
        }
        private void UpdateGroup(Group g)
        {
            var index = Data.IndexOf(g);
            Data.Remove(g);
            Data.Insert(index, g);
        }
        #endregion
    }
}
