using ARPEGOS.Helpers;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARPEGOS.Views
{
    public class SkillViewModel: BaseViewModel
    {
        private int totalValue;
        private int dice, previousDice;
        private string previousSkillSelected, skillSelected;

        public int Dice
        {
            get => dice;
            set => this.SetProperty(ref this.dice, value);
        }

        public int TotalValue
        {
            get => totalValue;
            set => this.SetProperty(ref this.totalValue, value);
        }

        public string SkillSelected
        {
            get => skillSelected;
            set => this.SetProperty(ref this.skillSelected, value);
        }

        public ICommand SelectSkillCommand { get; private set; }
        public ICommand CalculateSkillCommand { get; private set; }

        public SkillViewModel()
        {
            NavigationPage.SetHasBackButton(App.Navigation.NavigationStack.Last(), true);
            this.previousSkillSelected = string.Empty;
            this.SkillSelected = "No se ha seleccionado ninguna habilidad";
            this.Dice = 0;
            this.previousDice = 0;

            this.SelectSkillCommand = new Command(async() => await MainThread.InvokeOnMainThreadAsync(async() => await App.Navigation.PushAsync(new SkillListView())));
            this.CalculateSkillCommand = new Command<string>(async (skill) => 
            {
                if(skill != null || this.previousSkillSelected != skill || this.previousDice != this.Dice)
                {
                    this.previousSkillSelected = skill;
                    this.TotalValue = this.Dice + await Task.Run(() => DependencyHelper.CurrentContext.CurrentCharacter.GetSkillValue(FileService.EscapedName(skill)));
                    previousDice = this.Dice;
                }
            });

            
        }      

    }
}
