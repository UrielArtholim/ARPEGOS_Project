﻿using ARPEGOS.Helpers;
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
    public class CreationRootViewModel: BaseViewModel 
    {
        private DialogService dialogService;
        private string stageString;
        private string stage;
        public string Stage
        {
            get => stage;
            set => SetProperty(ref this.stage, value);
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

        public ObservableCollection<Item> Data { get; private set; }

        public ICommand NextCommand { get; private set; }
        public ICommand InfoCommand { get; private set; }
        public ICommand SelectCommand { get; private set; }

        public CreationRootViewModel()
        {
            this.dialogService = new DialogService();
            this.Continue = false;
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            this.stageString = game.GetCreationSchemeRootClass();
            this.Stage = this.stageString.Split('#').Last();
            
            Data = new ObservableCollection<Item>(character.GetIndividuals(stageString));

            this.NextCommand = new Command(async () => 
            {
                this.IsBusy = true;
                var character = DependencyHelper.CurrentContext.CurrentCharacter;
                var currentItem = this.SelectedItem;
                var ItemFullShortName = this.SelectedItem.FullName.Split('#').Last();
                var predicateString = character.GetObjectPropertyAssociated(this.stageString);
                var predicateName = predicateString.Split('#').Last();
                character.UpdateObjectAssertion($"{character.Context}{predicateName}", $"{character.Context}{ItemFullShortName}");

                var scheme = character.GetCreationScheme(this.SelectedItem.FullName);
                StageViewModel.CreationScheme = scheme;
                this.IsBusy = false;

                await MainThread.InvokeOnMainThreadAsync(async() => await App.Navigation.NavigationStack.Last().Navigation.PushAsync(new StageView(0)));
                await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PopModalAsync());
            });

            this.InfoCommand = new Command<Item>(async(item)=> 
            { 
                await this.dialogService.DisplayAlert(item.FormattedName, item.Description); 
            });

        }

    }
}
