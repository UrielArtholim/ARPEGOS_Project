using ARPEGOS.Helpers;
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
        private int skillValue; 
        private int totalValue;
        private uint dice;
        private string previousSkillSelected, skillSelected;

        public uint Dice
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

        public string SkillSelected
        {
            get => skillSelected;
            set => this.SetProperty(ref this.skillSelected, value);
        }

        public ICommand SelectSkillCommand { get; private set; }
        public ICommand CalculateSkillCommand { get; private set; }

        public SkillViewModel()
        {
            this.SelectSkillCommand = new Command(async() => await MainThread.InvokeOnMainThreadAsync(async() => await App.Navigation.PushAsync(new SkillListView())));
            this.CalculateSkillCommand = new Command(async () => 
            {
                if(this.SkillSelected != null && this.previousSkillSelected != this.SkillSelected)
                {
                    this.previousSkillSelected = this.SkillSelected;
                    this.SkillValue = await Task.Run(() => DependencyHelper.CurrentContext.CurrentCharacter.GetSkillValue(this.SkillSelected));
                    this.TotalValue = this.TotalValue = this.SkillValue + Convert.ToInt32(this.Dice);
                }
            });

            this.previousSkillSelected = string.Empty;
            this.SkillSelected = "No se ha seleccionado ninguna habilidad";
            this.SkillValue = 0;
            this.Dice = 0;
        }      

    }
}
