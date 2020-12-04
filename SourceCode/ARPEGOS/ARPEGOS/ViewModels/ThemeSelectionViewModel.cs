using ARPEGOS.Helpers;
using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class ThemeSelectionViewModel: BaseViewModel
    {
        private List<string> themes;
        public ICommand NextCommand { get; private set; }
        
        
        public List<string> Themes
        {
            get => this.themes;
            set => SetProperty(ref this.themes, value);
        }

        public ThemeSelectionViewModel()
        {
            this.Themes = new List<string>(DependencyHelper.CurrentContext.Themes.BackgroundThemes.Keys);
            this.NextCommand = new Command(() => App.Navigation.PopModalAsync());
        }
    }
}
