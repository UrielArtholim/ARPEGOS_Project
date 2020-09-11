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
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class SkillListViewModel: BaseViewModel
    {
        private ObservableCollection<Item> Items;
        private ObservableCollection<string> data;
        public ObservableCollection<string> Data
        {
            get => data;
            set => this.SetProperty(ref this.data, value);
        }

        public ICommand SelectItemCommand { get; private set; }
        public ICommand ReturnCommand { get; private set; }

        public SkillListViewModel()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            this.Data = new ObservableCollection<string>();
            this.Items = new ObservableCollection<Item>(character.GetCharacterSkills());
            foreach (var item in this.Items)
                this.Data.Add(item.FormattedName);

            this.SelectItemCommand = new Command<string>(selected =>  
            {
                var skillFormattedName = selected;
                var itemSelected = Items.Where(item => item.FormattedName == selected).Single();
                this.IsBusy = true;
                var skillViewContext = App.Navigation.NavigationStack[App.Navigation.NavigationStack.Count - 2].BindingContext as SkillViewModel;
                skillViewContext.SkillSelected = itemSelected;
                skillViewContext.SkillSelectedName = itemSelected.FormattedName;
                this.IsBusy = false;
                ReturnCommand.Execute(null);
            });

            this.ReturnCommand = new Command(async () => await MainThread.InvokeOnMainThreadAsync(async()=> await App.Navigation.PopAsync()));   
        }
    }
}
