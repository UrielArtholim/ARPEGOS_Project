﻿using ARPEGOS.Controls;
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
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class SingleChoiceViewModel: BaseViewModel
    {
        private DialogService dialogService;
        private Stage currentStage;
        private string stageString;
        private string stageName;

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
        public Stage CurrentStage
        {
            get => currentStage;
            set => SetProperty(ref this.currentStage, value);
        }

        public ObservableCollection<Item> Data { get; private set; }

        public ICommand NextCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand SelectCommand { get; private set; }
        public ICommand BackButtonCommand { get; private set; }

        public SingleChoiceViewModel()
        {
            this.dialogService = new DialogService();
            this.Continue = false;
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            this.CurrentStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
            this.stageString = this.CurrentStage.FullName;
            this.StageName = FileService.FormatName(this.stageString.Split('#').Last());

            if (StageViewModel.GeneralLimitProperty == null && StageName != "Nivel")
            {
                StageViewModel.GeneralLimitProperty = character.GetLimit(stageString, true);
                StageViewModel.GeneralMaximum = character.GetLimitValue(StageViewModel.GeneralLimitProperty);
                StageViewModel.GeneralLimit = StageViewModel.GeneralMaximum;
                StageViewModel.GeneralProgress = 1;
            }

            var datalist = new ObservableCollection<Item>(character.GetIndividuals(stageString));
            foreach (var item in datalist)
                if (string.IsNullOrEmpty(item.Description) || string.IsNullOrWhiteSpace(item.Description))
                    item.HasDescription = false;

            Data = new ObservableCollection<Item>(datalist);
            this.NextCommand = new Command(async () => await Task.Run(() => Next()));

            this.InfoCommand = new Command<Item>(async (item) =>
            {
                await this.dialogService.DisplayAlert(item.FormattedName, item.Description);
            });

            this.BackButtonCommand = new Command(async () => await CreationProcessBack());
            
        }
        private async Task CreationProcessBack()
        {
            if (StageViewModel.CreationScheme != null && StageViewModel.CurrentStep > 0)
                --StageViewModel.CurrentStep;
            await App.Navigation.PopAsync();
        }

        private async Task Next()
        {
            await Device.InvokeOnMainThreadAsync(() => this.IsBusy = true);
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var ItemFullShortName = this.SelectedItem.FullName.Split('#').Last();
            var predicateString = character.GetObjectPropertyAssociated(this.stageString);
            var predicateName = predicateString.Split('#').Last();
            character.UpdateObjectAssertion($"{character.Context}{predicateName}", $"{character.Context}{ItemFullShortName}");

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
                        App.Navigation = DependencyHelper.CurrentContext.AppMainView?.Detail?.Navigation;
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
                await Device.InvokeOnMainThreadAsync(() => this.IsBusy = false);
            }
        }
    }
}
