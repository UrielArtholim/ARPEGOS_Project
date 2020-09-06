using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
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
        private ObservableCollection<ValuedItem> data;
        private Stage currentStage;
        private string stageName;
        private int stageLimit;
        private string stageLimitProperty;
        private int currentLimit;
        private bool showDescription;

        public ObservableCollection<ValuedItem> Data
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

        public int StageLimit
        {
            get => this.stageLimit;
            set => SetProperty(ref this.stageLimit, value);
        }

        public int CurrentLimit
        {
            get => this.currentLimit;
            set => SetProperty(ref this.currentLimit, value);
        }

        public bool ShowDescription
        {
            get => this.showDescription;
            set => SetProperty(ref this.showDescription, value);
        }

        public ICommand NextCommand { get; private set; }

        public ValuedViewModel()
        {
            this.CurrentStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
            this.StageName = FileService.FormatName(this.CurrentStage.ShortName);
            this.Data = new ObservableCollection<ValuedItem>();
            this.ShowDescription = true;

            var character = DependencyHelper.CurrentContext.CurrentCharacter;

            this.stageLimitProperty = character.GetLimit(this.CurrentStage.ShortName);
            this.StageLimit = Convert.ToInt32(character.GetLimitValue(stageLimitProperty));
            if (StageViewModel.GeneralLimit != null)
                this.CurrentLimit = Math.Min(Convert.ToInt32(StageViewModel.GeneralLimit), this.StageLimit);
            else
                this.CurrentLimit = this.StageLimit;

            if (character.CheckDatatypeProperty(this.CurrentStage.FullName, StageViewModel.ApplyOnCharacter))
            {
                this.ShowDescription = false;
                var step = character.GetStep(this.StageName);
                if (this.CurrentLimit > step * 100)
                    this.CurrentLimit = step * 100;
                Data.Add(new ValuedItem(this.CurrentStage.FullName, string.Empty, this.CurrentLimit));
            }
            else
            {
                var Items = character.GetIndividuals(this.CurrentStage.FullName);
                foreach(var item in Items)
                {
                    var step = character.GetStep(item.FullName.Split('#').Last());
                    if (this.CurrentLimit > step * 100)
                        this.CurrentLimit = step * 100;
                    Data.Add(new ValuedItem(item.FullName, item.Description, this.CurrentLimit));
                }
            }

            this.NextCommand = new Command(async () =>
            {

                if(StageViewModel.GeneralLimitProperty == null)
                StageViewModel.GeneralLimitProperty = character.GetLimit(StageViewModel.RootStage, true);
                StageViewModel.GeneralLimit = character.GetLimitValue(StageViewModel.GeneralLimitProperty);                

                if (currentStage.IsGrouped)
                {
                    switch (currentStage.Type)
                    {
                        case Stage.StageType.MultipleChoice: await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new MultipleChoiceGroupView())); break;
                        default: await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new ValuedGroupView())); break;
                    }
                }
                else
                {
                    switch (currentStage.Type)
                    {
                        case Stage.StageType.SingleChoice: await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new SingleChoiceView())); break;
                        case Stage.StageType.MultipleChoice: await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new MultipleChoiceView())); break;
                        default: await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new ValuedView())); break;
                    }
                }
                await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PopModalAsync());
            });

            /*this.InfoCommand = new Command<Item>(async (item) =>
            {
                await this.dialogService.DisplayAlert(item.FormattedName, item.Description);
            });*/

        }
    }
}
