﻿using ARPEGOS.Helpers;
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
    public class SingleChoiceGroupViewModel : BaseViewModel
    {
        private DialogService dialogService;
        private string stageString;
        private string stageName;
        private Group lastGroup;
        private Stage currentStage;
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

        public ObservableCollection<Group> Data { get; private set; }

        public ICommand NextCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand GroupInfoCommand { get; private set; }
        public ICommand SelectItemCommand { get; private set; }
        public ICommand SelectGroupCommand { get; private set; }

        public SingleChoiceGroupViewModel()
        {
            this.dialogService = new DialogService();
            this.Continue = false;
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;

            var currentStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
            this.stageString = currentStage.FullName;
            this.StageName = FileService.FormatName(this.stageString.Split('#').Last());

            if (StageViewModel.GeneralLimitProperty == null && StageName != "Nivel")
            {
                StageViewModel.GeneralLimitProperty = character.GetLimit(this.stageString.Split('#').Last(), true);
                StageViewModel.GeneralLimit = character.GetLimitValue(StageViewModel.GeneralLimitProperty);
                StageViewModel.GeneralProgress = 1;
            }

            var datalist = character.GetIndividualsGrouped(stageString);
            foreach (var item in datalist)
                if (string.IsNullOrEmpty(item.Description) || string.IsNullOrWhiteSpace(item.Description))
                    item.HasDescription = false;

            Data = currentStage.Groups;

            this.SelectGroupCommand = new Command<Group>(async (group) => await MainThread.InvokeOnMainThreadAsync(() => SelectGroup(group)));

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

        private void SelectGroup(Group group)
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
        }

        private async Task Next()
        {
            await MainThread.InvokeOnMainThreadAsync(() => this.IsBusy = true);
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var currentItem = this.SelectedItem;
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
                await dialogService.DisplayAlert("Exception", e.Message);
            }
            finally
            {
                await MainThread.InvokeOnMainThreadAsync(() => this.IsBusy = false);
            }
        }
        private void UpdateGroup(Group g)
        {
            var index = Data.IndexOf(g);
            Data.Remove(g);
            Data.Insert(index, g);
        }
    }
}