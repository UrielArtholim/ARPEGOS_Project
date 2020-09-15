using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
using FFImageLoading.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class ValuedViewModel: BaseViewModel
    {
        #region Properties
        #region Private
        private DialogService dialogService = new DialogService();
        private ObservableCollection<Item> data;
        private Stage currentStage;
        private string stageName, stageLimitProperty;
        private double stageLimit, stageProgress, stageProgressLabel;
        private double generalLimit, generalProgress, generalProgressLabel;
        private double sliderLimit;
        private double? elementLimit;
        private bool hasGeneralLimit, hasStageLimit, showDescription;
        
        #endregion

        #region Public
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
        public bool HasStageLimit
        {
            get => this.hasStageLimit;
            set => SetProperty(ref this.hasStageLimit, value);
        }
        public double SliderLimit
        {
            get => this.sliderLimit;
            set => SetProperty(ref this.sliderLimit, value);
        }

        public double? ElementLimit
        {
            get => this.elementLimit;
            set => SetProperty(ref this.elementLimit, value);
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
        #endregion

        #region Constructor
        public ValuedViewModel()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;

            this.CurrentStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
            this.StageName = FileService.FormatName(this.CurrentStage.ShortName);
            this.stageLimitProperty = character.GetLimit(this.CurrentStage.FullName.Split('#').Last());
            this.StageLimit = character.GetLimitValue(this.stageLimitProperty);
            this.SliderLimit = this.StageLimit;
            this.StageProgressLabel = this.StageLimit;
            this.StageProgress = 1;
            this.ShowDescription = true;
            this.HasGeneralLimit = this.CurrentStage.EditGeneralLimit;
            this.hasStageLimit = true;
            this.ElementLimit = null;
            this.Data = new ObservableCollection<Item>();

            if(this.HasGeneralLimit == true)
            {
                this.HasGeneralLimit = true;
                this.GeneralLimit = StageViewModel.GeneralLimit;
                this.GeneralProgress = StageViewModel.GeneralProgress;
                this.GeneralProgressLabel = this.GeneralLimit;
            }
            else
            {
                this.GeneralLimit = 1;
            }

            if (this.HasGeneralLimit == true)
                this.SliderLimit = Math.Min(Convert.ToInt32(StageViewModel.GeneralLimit), this.StageLimit);

            if(character.CheckClass(this.CurrentStage.FullName, false))
            {
                var subjectClass = game.Ontology.Model.ClassModel.SelectClass(this.CurrentStage.FullName);
                var subjectClassAnnotationEntries = game.Ontology.Model.ClassModel.Annotations.CustomAnnotations.SelectEntriesBySubject(subjectClass);
                var elementLimitAnnotationEntries = subjectClassAnnotationEntries.Where(entry => entry.TaxonomyPredicate.ToString().Contains("ElementLimit"));
                if(elementLimitAnnotationEntries.Count() > 0)
                {
                    var definition = elementLimitAnnotationEntries.Single().TaxonomyObject.ToString().Split('^').First();
                    this.ElementLimit = character.GetValue(definition);
                    this.SliderLimit = Convert.ToDouble(this.ElementLimit);
                }
            }


            if (character.CheckDatatypeProperty(this.CurrentStage.FullName, StageViewModel.ApplyOnCharacter))
            {
                this.ShowDescription = false;
                var itemStep = character.GetStep(this.StageName);
                if (this.SliderLimit > itemStep * 100)
                    this.SliderLimit = itemStep * 100;

                List<Item> DataList = new List<Item>();
                var newItem = new Item(this.CurrentStage.FullName, string.Empty, string.Empty, itemStep, this.SliderLimit);
                newItem.IsEnabled = true;
                Data.Add(newItem);
            }
            else
            {
                var Items = character.GetIndividuals(this.CurrentStage.FullName);
                List<Item> DataList = new List<Item>();

                foreach (var item in Items)
                {
                    var itemStep = character.GetStep(item.FullName.Split('#').Last());

                    if (this.SliderLimit > item.Step * 100)
                        this.SliderLimit = item.Step * 100;

                    var itemClassName = string.Empty;
                    var itemFact = DependencyHelper.CurrentContext.CurrentGame.Ontology.Data.SelectFact(item.FullName);
                    var itemClassAssertion = DependencyHelper.CurrentContext.CurrentGame.Ontology.Data.Relations.ClassType.SelectEntriesBySubject(itemFact);
                    if (itemClassAssertion.EntriesCount > 0)
                        itemClassName = itemClassAssertion.Single().TaxonomyObject.ToString().Split('#').Last();

                    var newItem = new Item(item.FullName, item.Description, itemClassName, itemStep, this.SliderLimit);
                    Data.Add(newItem);
                }
            }


            this.NextCommand = new Command(async () => 
            {
                var character = DependencyHelper.CurrentContext.CurrentCharacter;
                var game = DependencyHelper.CurrentContext.CurrentGame;
                foreach(var item in Data)
                {
                    var itemName = item.FullName.Split('#').Last();
                    var characterItemString = $"{character.Context}{itemName}";
                    var itemValue = item.Value.ToString();

                    if (!character.CheckDatatypeProperty(item.FullName, false))
                    {
                        var itemProperties = game.Ontology.Model.PropertyModel.Where(property => property.ToString().Contains(itemName));
                        if(itemProperties.Count() > 0)
                        {
                            var itemTotalPropertyEntries = itemProperties.Where(property => property.ToString().Contains("Total"));
                            if(itemTotalPropertyEntries.Count() > 0)
                            {
                                var itemPropertyName = itemTotalPropertyEntries.Single().ToString().Split('#').Last();
                                characterItemString = $"{character.Context}{itemPropertyName}";
                                character.UpdateDatatypeAssertion(characterItemString, itemValue);
                            }
                        }
                    }
                    else
                        character.UpdateDatatypeAssertion(characterItemString, itemValue);
                }

                if (StageViewModel.GeneralLimitProperty == null)
                {
                    StageViewModel.GeneralLimitProperty = character.GetLimit(this.CurrentStage.FullName, true);
                    StageViewModel.GeneralLimit = character.GetLimitValue(StageViewModel.GeneralLimitProperty);
                    StageViewModel.GeneralProgress = 1;
                }
                else
                    StageViewModel.GeneralLimit = this.GeneralProgress;

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

                ++StageViewModel.CurrentStep;
                var nextStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);

                if (nextStage.IsGrouped)
                {
                    switch (nextStage.Type)
                    {
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
        #endregion         
    }
}