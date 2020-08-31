using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class OptionsViewModel: BaseViewModel
    {
        public ICommand InfoCommand { get; private set; }
        public ICommand SkillCommand { get; private set; }

        public OptionsViewModel ()
        {
            NavigationPage.SetHasBackButton(App.Navigation.NavigationStack.Last(), false);
            this.InfoCommand = new Command(async () => await MainThread.InvokeOnMainThreadAsync(async () => await App.Navigation.PushAsync(new CharacterInfoView())));
            this.SkillCommand = new Command(async () => await MainThread.InvokeOnMainThreadAsync(() => App.Navigation.PushAsync(new SkillView())));
        }
    }
}
