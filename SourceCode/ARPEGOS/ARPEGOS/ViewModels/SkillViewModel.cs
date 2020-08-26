using ARPEGOS.Helpers;
using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARPEGOS.Views
{
    public class SkillViewModel: BaseViewModel
    {
        private int _skillValue; 
        private int _totalValue;
        private uint _dice;
        private string _previousSkillSelected, _skillSelected;

        public uint Dice
        {
            get => _dice;
            set => this.SetProperty(ref this._dice, value);
        }

        public int SkillValue
        {
            get => _skillValue;
            set => this.SetProperty(ref this._skillValue, value);
        }

        public int TotalValue
        {
            get => _totalValue;
            set => this.SetProperty(ref this._totalValue, value);
        }

        public string SkillSelected
        {
            get => _skillSelected;
            set => this.SetProperty(ref this._skillSelected, value);
        }

        public ICommand SelectSkillCommand { get; private set; }
        public ICommand CalculateSkillCommand { get; private set; }

        public SkillViewModel()
        {
            this.SelectSkillCommand = new Command(async() => await App.Navigation.PushAsync(new SkillListView()));
            this.CalculateSkillCommand = new Command(() => 
            {
                if(this._previousSkillSelected != this.SkillSelected)
                {
                    this._previousSkillSelected = this.SkillSelected;
                    this.SkillValue = DependencyHelper.CurrentContext.CurrentCharacter.GetSkillValue(this.SkillSelected);
                    this.TotalValue = this.TotalValue = this.SkillValue + Convert.ToInt32(this.Dice);
                }
            });

            this._previousSkillSelected = string.Empty;
            this.SkillSelected = "No se ha seleccionado ninguna habilidad";
            this.SkillValue = 0;
            this.Dice = 0;
        }      

    }
}
