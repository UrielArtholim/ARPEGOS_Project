using ARPEGOS.Helpers;
using ARPEGOS.Models;
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
        private List<ThemeItem> themes;
        public ICommand NextCommand { get; private set; }
        
        
        public List<ThemeItem> Themes
        {
            get => this.themes;
            set => SetProperty(ref this.themes, value);
        }

        public ThemeSelectionViewModel()
        {
            this.Themes = new List<ThemeItem>();
            foreach (var item in DependencyHelper.CurrentContext.Themes.BackgroundThemes.Keys)
            {
                var isCurrentTheme = (item == DependencyHelper.CurrentContext.Themes.CurrentTheme) ? true : false;
                this.Themes.Add(new ThemeItem(item,isCurrentTheme));
            }

            this.NextCommand = new Command(() => 
            { 
                var mainpage = App.Current.MainPage as MasterDetailPage;
                mainpage.IsPresented = false;
                App.Navigation.PopModalAsync(); 
            });
        }
    }
}
