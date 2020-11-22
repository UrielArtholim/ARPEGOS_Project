using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class MultipleChoiceViewModel: BaseViewModel
    {
        #region Properties
        #region Private
        private DialogService dialogService = new DialogService();
        private ObservableCollection<Item> data, elements;
        private Stage currentStage;
        private string stageName, stageLimitProperty;
        private double stageLimit, stageProgress, stageProgressLabel;
        private double generalLimit, generalProgress, generalProgressLabel;
        private double currentLimit;
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

        public ObservableCollection<Item> Elements
        {
            get => this.elements;
            set => SetProperty(ref this.elements, value);
        }
        #endregion
        #endregion

        #region Commands
        public ICommand NextCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand SelectCommand { get; private set; }
        #endregion

        #region Constructor

        public MultipleChoiceViewModel()
        {
            this.dialogService = new DialogService();
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            this.CurrentStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
            var stageString = this.CurrentStage.FullName;
            this.StageName = FileService.FormatName(stageString.Split('#').Last());
            this.StageLimitProperty = character.GetLimit(this.CurrentStage.FullName.Split('#').Last(), false, this.CurrentStage.EditGeneralLimit);
            this.StageLimit = character.GetLimitValue(this.stageLimitProperty);
            this.StageProgressLabel = this.StageLimit;
            this.StageProgress = this.StageProgressLabel != 0 ? 1 : 0;
            this.ShowDescription = true;
            this.HasGeneralLimit = this.CurrentStage.EditGeneralLimit;
            this.hasStageLimit = true;

            if (StageViewModel.GeneralLimitProperty == null && StageName != "Nivel")
            {
                StageViewModel.GeneralLimitProperty = character.GetLimit(stageString, true);
                StageViewModel.GeneralMaximum = character.GetLimitValue(StageViewModel.GeneralLimitProperty);
                StageViewModel.GeneralLimit = StageViewModel.GeneralMaximum;
                StageViewModel.GeneralProgress = 1;
            }

            if (this.HasGeneralLimit == true)
            {
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

            this.Elements = new ObservableCollection<Item>(character.GetIndividuals(this.CurrentStage.FullName));
            var availableItems = character.CheckAvailableOptions(this.CurrentStage.FullName, this.HasGeneralLimit, StageViewModel.GeneralLimitProperty, this.GeneralLimit, this.StageLimitProperty, this.StageLimit);

            var datalist = new ObservableCollection<Item>();

            if (availableItems.Count() > 0)
            {
                foreach (var item in this.Elements)
                {
                    foreach (var availableItem in availableItems)
                    {
                        if (availableItem.FullName.Split('#').Last() == item.FullName.Split('#').Last())
                        {
                            datalist.Add(item);
                            break;
                        }
                        else if (datalist.Contains(item))
                            datalist.Remove(item);
                    }
                }
            }            

            Data = new ObservableCollection<Item>(datalist);

            this.NextCommand = new Command(async () => await Task.Run(() => Next()));

            this.InfoCommand = new Command<Item>(async (item) =>
            {
                await this.dialogService.DisplayAlert(item.FormattedName, item.Description);
            });
        }
        #endregion

        #region Methods
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
                    await MainThread.InvokeOnMainThreadAsync(() =>
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
                await MainThread.InvokeOnMainThreadAsync(() => this.IsBusy = false);
            }
        }
        public async Task UpdateView()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var availableItems = await Task.Run(()=> character.CheckAvailableOptions(this.CurrentStage.FullName, this.HasGeneralLimit, StageViewModel.GeneralLimitProperty, this.GeneralProgressLabel, this.StageLimitProperty, this.StageProgressLabel));

            int elementIndex = 0;
            foreach(var element in this.Elements)
            {                
                bool elementFound = false;
                foreach(var item in availableItems)
                {
                    var elementName = element.FullName.Split('#').Last();
                    var itemName = item.FullName.Split('#').Last();
                    if (element.FullName == item.FullName)
                    {
                        elementFound = true;
                        if (Data.Contains(element) == false)
                        {
                            if (elementIndex >= Data.Count())
                                Data.Add(element);
                            else
                                Data.Insert(elementIndex, element);
                            break;
                        }
                    }
                }
                if (elementFound == false)
                    if (Data.Contains(element))
                    {
                        var item = Data.ElementAt(Data.IndexOf(element));
                        if (item.IsSelected == false)
                            Data.Remove(element);
                    }
                ++elementIndex;
            }
            /*
            foreach (var item in this.Elements)
            {
                foreach (var availableItem in availableItems)
                {
                    if (availableItem.ShortName == item.ShortName)
                    {
                        if(!updatedDatalist.Contains(item))
                        {
                            updatedDatalist.Add(item);
                            break;
                        }
                    }
                    else
                    {
                        if (updatedDatalist.Contains(item))
                            updatedDatalist.Remove(item);
                    }
                        
                }
            }*/

        }
        #endregion
    }
}
