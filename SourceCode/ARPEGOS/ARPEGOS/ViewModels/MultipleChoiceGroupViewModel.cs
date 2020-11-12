using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        public DialogService dialogService = new DialogService();
        private ObservableCollection<Group> data, datalist;
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
            var stageString = this.CurrentStage.FullName; //https://arpegos-project.org/games/anima/characters/1#Arte_Marcial
            this.StageName = FileService.FormatName(stageString.Split('#').Last());
            this.StageLimitProperty = character.GetLimit(this.CurrentStage.FullName.Split('#').Last(), false, this.CurrentStage.EditGeneralLimit);
            this.StageLimit = character.GetLimitValue(this.stageLimitProperty);
            this.StageProgressLabel = this.StageLimit;
            // Actualizar stageProgress
            //this.StageProgress = 1; Buscar limite 1500 y dividir stageprogresslabel / 1500 
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

                var stageProperty = game.Ontology.Model.PropertyModel.SelectProperty(character.GetString(this.stageLimitProperty));
                var stagePropertyDefinedAnnotations = game.Ontology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(stageProperty);
                var definition = stagePropertyDefinedAnnotations.Single().TaxonomyObject.ToString().Split('^').First();
                var stageMax = Convert.ToDouble(character.GetValue(definition));
                this.StageProgress = this.StageProgressLabel / stageMax;
            }
            else
                this.StageProgress = 1; 

            Datalist = new ObservableCollection<Group>(character.GetIndividualsGrouped(this.CurrentStage.FullName));
            foreach (var item in datalist)
                if (string.IsNullOrEmpty(item.Description) || string.IsNullOrWhiteSpace(item.Description))
                    item.HasDescription = false;

            var availableItems = character.CheckAvailableOptions(this.CurrentStage.FullName, this.HasGeneralLimit, StageViewModel.GeneralLimitProperty, this.GeneralLimit, this.StageLimitProperty, this.StageLimit);
            // Add data aux & group
            
            foreach(var group in Datalist)
            {
                var updatedGroup = group;
                foreach(var item in group.Elements)
                {
                    foreach(var availableItem in availableItems)
                    {
                        if (availableItem.FullName == item.FullName)
                            item.IsEnabled = true;
                        else
                            item.IsEnabled = false;
                    }
                }
            }

            Data = new ObservableCollection<Group>(datalist);

            this.NextCommand = new Command(async () => await Task.Run(() => Next()));

            this.SelectGroupCommand = new Command<Group>(async (group) => await Task.Run(async() => await SelectGroup(group)));

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

        private async Task SelectGroup(Group group)
        {
            await MainThread.InvokeOnMainThreadAsync(()=>this.IsBusy = true);
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var availableItems = character.CheckAvailableOptions(this.CurrentStage.FullName, this.HasGeneralLimit, StageViewModel.GeneralLimitProperty, this.GeneralLimit, this.StageLimitProperty, this.StageLimit);

            if (lastGroup == group)
            {
                group.Expanded = !group.Expanded;
                if(group.Expanded == true)
                {
                    if (availableItems.Count > 0)
                    {
                        foreach (var groupItem in group.Elements)
                        {
                            if (availableItems.Count() > 0)
                            {
                                foreach (var item in availableItems)
                                {
                                    if (groupItem.FullName.Split('#').Last() == item.FullName.Split('#').Last())
                                    {
                                        groupItem.IsEnabled = true;
                                        break;
                                    }
                                    else
                                        groupItem.IsEnabled = false;
                                }
                            }
                            else
                                groupItem.IsEnabled = false;                            
                        }
                        foreach (var item in group.Elements)
                        {
                            if (item.IsEnabled == true)
                            {
                                if (!group.Contains(item))
                                    group.Add(item);
                            }
                            else
                                if (group.Contains(item))
                                group.Remove(item);
                        }
                    }
                }
                else
                {
                    var collection = new Collection<Item>(group);
                    var enumerator = collection.GetEnumerator();
                    enumerator.Reset();
                    while(enumerator.MoveNext())
                    {
                        var item = enumerator.Current;
                        if (item.IsSelected == false)
                            group.Remove(item);
                    }                        
                }                
            }
            else
            {
                if (lastGroup != null)
                {
                    lastGroup.Expanded = false;
                    await RefreshView();
                }
                group.Expanded = true;
                if (availableItems.Count() > 0)
                {
                    foreach (var groupItem in group.Elements)
                    {
                        foreach (var item in availableItems)
                        {
                            if (groupItem.FullName.Split('#').Last() == item.FullName.Split('#').Last())
                            {
                                groupItem.IsEnabled = true;
                                break;
                            }
                            else
                                groupItem.IsEnabled = false;
                        }
                    }
                    foreach (var item in group.Elements)
                        if (item.IsEnabled == true)
                        {
                            if (!group.Contains(item))
                                group.Add(item);
                        }
                        else
                            if (group.Contains(item))
                            group.Remove(item);
                }
            }
            await RefreshView();
            lastGroup = group;
            await MainThread.InvokeOnMainThreadAsync(() => this.IsBusy = false);
        }

        private async Task Next()
        {
            await MainThread.InvokeOnMainThreadAsync(() => this.IsBusy = true);
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
                    await dialogService.DisplayAlert("Nota informativa", "Proceso de creación finalizado correctamente");
                    await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PopToRootAsync());
                }
            }
            catch (Exception e)
            {
                await dialogService.DisplayAlert(this.StageName, e.Message);
            }
            finally
            {
                await MainThread.InvokeOnMainThreadAsync(() => this.IsBusy = false);
            }
        }

        public void UpdateView()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var availableItems = character.CheckAvailableOptions(this.CurrentStage.FullName, this.HasGeneralLimit, StageViewModel.GeneralLimitProperty, this.GeneralProgressLabel, this.StageLimitProperty, this.StageProgressLabel);
            var groupEnumerator = Datalist.GetEnumerator();
            groupEnumerator.Reset();

            while(groupEnumerator.MoveNext())
            {
                var elementIndex = 0;
                var group = groupEnumerator.Current;
                var elementEnumerator = group.GetEnumerator();
                elementEnumerator.Reset();

                while(elementEnumerator.MoveNext())
                {
                    var element = elementEnumerator.Current;
                    bool elementFound = false;

                    foreach (var item in availableItems)
                    {
                        var elementName = element.FullName.Split('#').Last();
                        var itemName = item.FullName.Split('#').Last();
                        if (element.FullName == item.FullName)
                        {
                            elementFound = true;
                            if (group.Contains(element) == false)
                            {
                                if (elementIndex >= group.Count())
                                    group.Add(element);
                                else
                                    group.Insert(elementIndex, element);
                                break;
                            }
                        }
                    }
                    if (elementFound == false)
                        if (group.Contains(element))
                        {
                            var item = group.ElementAt(group.IndexOf(element));
                            if (item.IsSelected == false)
                                group.Remove(element);
                        }
                    ++elementIndex;
                }
            }
        }
        private async Task RefreshView()
        {
            await MainThread.InvokeOnMainThreadAsync(() => RefreshCollection()); 
            await MainThread.InvokeOnMainThreadAsync(() => RefreshCollection());
        }
        #endregion
    }
}
