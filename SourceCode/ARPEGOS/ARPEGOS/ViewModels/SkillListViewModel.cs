using ARPEGOS.Helpers;
using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class SkillListViewModel: BaseViewModel
    {
        public ObservableCollection<string> Data;

        public ICommand SelectItemCommand { get; private set; }

        public SkillListViewModel()
        {
            this.SelectItemCommand = new Command<string>(item =>  
            {
                this.IsBusy = true;
                var skillViewContext = App.Navigation.NavigationStack[App.Navigation.NavigationStack.Count - 2].BindingContext as SkillViewModel;
                skillViewContext.SkillSelected = item;
                this.IsBusy = false;
            });

            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            Data = new ObservableCollection<string>(character.GetCharacterSkills());
        }
    }
}
