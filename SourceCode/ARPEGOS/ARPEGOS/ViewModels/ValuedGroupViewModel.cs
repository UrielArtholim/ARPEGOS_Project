using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ARPEGOS.ViewModels
{
    public class ValuedGroupViewModel: BaseViewModel
    {
        private DialogService dialogService = new DialogService();
        private ObservableCollection<Item> data;
        private Stage currentStage;
        private string stageName;
        private string stageLimitProperty;

        private double stageLimit, stageProgress, stageProgressLabel;
        private double generalLimit, generalProgress, generalProgressLabel;
        private bool hasGeneralLimit;

        private double sliderLimit;
        private bool showDescription;

        public ObservableCollection<Item> Data
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

        public bool HasGeneralLimit
        {
            get => this.hasGeneralLimit;
            set => SetProperty(ref this.hasGeneralLimit, value);
        }

        public double SliderLimit
        {
            get => this.sliderLimit;
            set => SetProperty(ref this.sliderLimit, value);
        }

        public bool ShowDescription
        {
            get => this.showDescription;
            set => SetProperty(ref this.showDescription, value);
        }

        public ICommand NextCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand GroupInfoCommand { get; private set; }

        public ValuedGroupViewModel()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            this.CurrentStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
            this.StageName = FileService.FormatName(this.CurrentStage.ShortName);
            this.stageLimitProperty = character.GetLimit(this.CurrentStage.FullName.Split('#').Last());
            this.StageLimit = character.GetLimitValue(this.stageLimitProperty);
            this.ShowDescription = true;
            this.SliderLimit = this.StageLimit;
            this.StageProgress = 1;
            this.StageProgressLabel = this.StageLimit;


        }
    }
}
