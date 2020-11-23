using ARPEGOS.Helpers;
using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARPEGOS.ViewModels
{
    public class SkillViewModel: BaseViewModel
    {
        private int skillValue; 
        private int totalValue;
        private int previousDice, dice;
        private string skillSelectedName;
        private Item previousSkillSelected, skillSelected;

        public int Dice
        {
            get => dice;
            set => this.SetProperty(ref this.dice, value);
        }

        public int SkillValue
        {
            get => skillValue;
            set => this.SetProperty(ref this.skillValue, value);
        }

        public int TotalValue
        {
            get => totalValue;
            set => this.SetProperty(ref this.totalValue, value);
        }

        public Item SkillSelected
        {
            get => skillSelected;
            set => this.SetProperty(ref this.skillSelected, value);
        }

        public string SkillSelectedName
        {
            get => this.skillSelectedName;
            set => SetProperty(ref this.skillSelectedName, value);
        }

        public ICommand SelectSkillCommand { get; private set; }
        public ICommand CalculateSkillCommand { get; private set; }

        public SkillViewModel()
        {
            this.SelectSkillCommand = new Command(async() => await MainThread.InvokeOnMainThreadAsync(async()=> await App.Navigation.PushAsync(new SkillListView())));
            this.CalculateSkillCommand = new Command(async () => 
            {
                if(this.SkillSelected != null || this.previousSkillSelected != this.SkillSelected || this.previousDice != this.Dice)
                {
                    this.previousSkillSelected = this.SkillSelected;
                    this.SkillValue = await Task.Run(() => DependencyHelper.CurrentContext.CurrentCharacter.GetSkillValue(this.SkillSelected.FullName.Split('#').Last()));
                    this.TotalValue= this.SkillValue + Convert.ToInt32(this.Dice);
                    this.previousDice = this.Dice;
                }
            });

            this.previousSkillSelected = null;
            this.SkillSelected = null;
            this.SkillValue = 0;
            this.Dice = 0;
            this.previousDice = 0;
        }      

    }
}
