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
    public class ValuedGroupViewModel: BaseViewModel
    {
        public DialogService dialogService = new DialogService();
        private ObservableCollection<Group> data, datalist;
        private Stage currentStage;
        private string stageName;
        private string stageLimitProperty;

        private double stageLimit, stageProgress, stageProgressLabel;
        private double generalLimit, generalProgress, generalProgressLabel;
        private bool hasGeneralLimit, hasStageLimit;

        private double currentLimit;
        private double? elementLimit;
        private bool showDescription;
        private Group lastGroup;


        public ObservableCollection<Group> Data
        {
            get => this.data;
            set => SetProperty(ref this.data, value);
        }

        private ObservableCollection<Group> Datalist
        {
            get => this.datalist;
            set => SetProperty(ref this.datalist, value);
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

        public string StageLimitProperty
        {
            get => this.stageLimitProperty;
            set => SetProperty(ref this.stageLimitProperty, value);
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

        public double CurrentLimit
        {
            get => this.currentLimit;
            set => SetProperty(ref this.currentLimit, value);
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

        public ICommand NextCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand SelectGroupCommand { get; private set; }
        public ICommand GroupInfoCommand { get; private set; }

        public ValuedGroupViewModel()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentCharacter;
            this.CurrentStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
            var stageString = this.CurrentStage.FullName;
            this.StageName = FileService.FormatName(stageString.Split('#').Last());
            this.StageLimitProperty = character.GetLimit(this.CurrentStage.FullName.Split('#').Last(), false, this.CurrentStage.EditGeneralLimit);
            this.StageLimit = character.GetLimitValue(this.stageLimitProperty);
            this.ShowDescription = true;
            this.HasGeneralLimit = this.CurrentStage.EditGeneralLimit;
            this.HasStageLimit = true;
            this.CurrentLimit = this.StageLimit;
            this.StageProgressLabel = this.StageLimit;
            this.StageProgress = this.StageProgressLabel != 0 ? 1 : 0;

            if (StageViewModel.GeneralLimitProperty == null && StageName != "Nivel")
            {
                StageViewModel.GeneralLimitProperty = character.GetLimit(stageString, true);
                StageViewModel.GeneralMaximum = character.GetLimitValue(StageViewModel.GeneralLimitProperty);
                StageViewModel.GeneralLimit = StageViewModel.GeneralMaximum;
                StageViewModel.GeneralProgress = 1;
            }

            if (this.HasGeneralLimit == true)
            {
                this.HasGeneralLimit = true;
                this.GeneralLimit = StageViewModel.GeneralLimit;
                this.GeneralProgress = StageViewModel.GeneralProgress;
                this.GeneralProgressLabel = this.GeneralLimit;
                this.CurrentLimit = Math.Min(this.GeneralLimit, this.StageLimit);

                var gameStageProperty = character.GetString(this.stageLimitProperty);
                var stageProperty = game.Ontology.Model.PropertyModel.SelectProperty($"{game.Context}{this.stageLimitProperty}");
                if(stageProperty != null)
                {
                    var stagePropertyDefinedAnnotations = game.Ontology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(stageProperty);
                    if(stagePropertyDefinedAnnotations.Count() > 0)
                    {
                        var definition = stagePropertyDefinedAnnotations.Single().TaxonomyObject.ToString().Split('^').First();
                        var stageMax = Convert.ToDouble(character.GetValue(definition));
                        this.StageProgress = this.StageProgressLabel / stageMax;
                    }                    
                }
            }                

            Datalist = new ObservableCollection<Group>(character.GetIndividualsGrouped(this.CurrentStage.FullName));
            foreach (var group in Datalist)
            {
                foreach (var groupItem in group.Elements)
                {
                    groupItem.Value = 0;
                    if (string.IsNullOrEmpty(groupItem.Description) || string.IsNullOrWhiteSpace(groupItem.Description))
                        groupItem.HasDescription = false;
                }

                if (string.IsNullOrEmpty(group.Description) || string.IsNullOrWhiteSpace(group.Description))
                    group.HasDescription = false;                
            }                

            Data = new ObservableCollection<Group>(Datalist);

            this.SelectGroupCommand = new Command<Group>(async (group) => await Task.Run(async() => await SelectGroup(group)));

            this.NextCommand = new Command(async () => await Task.Run(() => Next()));

            this.InfoCommand = new Command<Item>(async (item) =>
            {
                await this.dialogService.DisplayAlert(item.FormattedName, item.Description);
            });

            this.GroupInfoCommand = new Command<Group>(async (group) =>
            {
                await this.dialogService.DisplayAlert(group.FormattedTitle, group.Description);
            });

        }

        private async Task SelectGroup(Group group)
        {
            await Device.InvokeOnMainThreadAsync(() => this.IsBusy = true);
            if (lastGroup != group)           
            {
                if (lastGroup != null)
                    lastGroup.Expanded = false;
                group.Expanded = true;
            }
            else
                group.Expanded = !group.Expanded;

            lastGroup = group;
            await RefreshView();
            await Device.InvokeOnMainThreadAsync(() => this.IsBusy = false);
        }

        private async Task Next()
        {
            await Device.InvokeOnMainThreadAsync(() => this.IsBusy = true);
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            foreach (var group in Data)
            {
                foreach (var item in group.Elements)
                {
                    var itemName = item.FullName.Split('#').Last();
                    var characterItemString = $"{character.Context}{itemName}";
                    var itemValue = item.Value.ToString();

                    if (!character.CheckDatatypeProperty(item.FullName, false))
                    {
                        var itemProperties = game.Ontology.Model.PropertyModel.Where(property => property.ToString().Contains(itemName));
                        if (itemProperties.Count() > 0)
                        {
                            var itemTotalPropertyEntries = itemProperties.Where(property => property.ToString().Contains("Total"));
                            if (itemTotalPropertyEntries.Count() > 0)
                            {
                                var itemPropertyName = string.Empty;
                                if (itemTotalPropertyEntries.Count() > 1)
                                {
                                    var name = $"Per_{itemName}_Total";
                                    itemPropertyName = itemTotalPropertyEntries.Where(entry => entry.ToString().Split('#').Last() == name).Single().ToString().Split('#').Last();
                                }
                                else
                                    itemPropertyName = itemTotalPropertyEntries.Single().ToString().Split('#').Last();
                                characterItemString = $"{character.Context}{itemPropertyName}";
                                character.UpdateDatatypeAssertion(characterItemString, itemValue);
                            }
                        }
                    }
                    else
                        character.UpdateDatatypeAssertion(characterItemString, itemValue);
                }
            }

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
                StageViewModel.GeneralProgress = this.GeneralProgressLabel / StageViewModel.GeneralMaximum;
            }

            ++StageViewModel.CurrentStep;
            try
            {
                if (StageViewModel.CurrentStep < StageViewModel.CreationScheme.Count())
                {
                    var nextStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
                    if (nextStage.IsGrouped)
                    {
                        switch (nextStage.Type)
                        {
                            case Stage.StageType.SingleChoice: await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new SingleChoiceGroupView())); break;
                            case Stage.StageType.MultipleChoice: await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new MultipleChoiceGroupView())); break;
                            default: await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new ValuedGroupView())); break;
                        }
                    }
                    else
                    {
                        switch (nextStage.Type)
                        {
                            case Stage.StageType.SingleChoice: await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new SingleChoiceView())); break;
                            case Stage.StageType.MultipleChoice: await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new MultipleChoiceView())); break;
                            default: await Device.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new ValuedView())); break;
                        }
                    }
                }
                else
                {
                    await dialogService.DisplayAlert("Nota informativa", "Proceso de creación finalizado correctamente");
                    await Device.InvokeOnMainThreadAsync(() =>
                    {
                        App.Navigation = DependencyHelper.CurrentContext.AppMainView.Navigation;
                        App.Current.MainPage = DependencyHelper.CurrentContext.AppMainView;
                    });
                }
            }
            catch (Exception e)
            {
                await dialogService.DisplayAlert(this.StageName, e.Message);
                --StageViewModel.CurrentStep;
            }
            finally
            {
                await Device.InvokeOnMainThreadAsync(()=> this.IsBusy = false);
            }
        }
        private async Task RefreshView()
        {
            await Device.InvokeOnMainThreadAsync(()=>RefreshCollection());
        }
    }
}
