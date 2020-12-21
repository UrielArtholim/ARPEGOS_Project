using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.Helpers
{
    public class ThemeHelper : INotifyPropertyChanged
    {
        private string backgroundSource, addSource, removeSource, currentTheme;
        public string CurrentThemeBackground 
        { 
            get => backgroundSource; 
            set => SetProperty(ref this.backgroundSource, value); 
        }

        public string CurrentThemeAddButton
        {
            get => addSource;
            set => SetProperty(ref this.addSource, value);
        }
        public string CurrentThemeRemoveButton
        {
            get => removeSource;
            set => SetProperty(ref this.removeSource, value);
        }
        public string CurrentTheme
        {
            get => currentTheme;
            set => SetProperty(ref this.currentTheme, value);
        }

        public Dictionary<string, string> BackgroundThemes;

        public Dictionary<string, string> AddButtonThemes;

        public Dictionary<string, string> RemoveButtonThemes;


        public event PropertyChangedEventHandler PropertyChanged;

        public ThemeHelper()
        {
            this.BackgroundThemes = new Dictionary<string, string>
            {
                {"Día", "day.png"},
                {"Noche", "night.png"},
                {"Bosque", "forest.png"},
                {"Desierto", "desert.png"},
                {"Tundra", "tundra.png"},
                {"Valle", "valley.png"},
                {"Oceano", "ocean.png"}
            };
            this.AddButtonThemes = new Dictionary<string, string>
            {
                {"Día", "day_add.png"},
                {"Noche", "night_add.png"},
                {"Bosque", "forest_add.png"},
                {"Desierto", "desert_add.png"},
                {"Tundra", "tundra_add.png"},
                {"Valle", "valley_add.png"},
                {"Oceano", "ocean_add.png"}
            };
            this.RemoveButtonThemes = new Dictionary<string, string>
            {
                {"Día", "day_delete.png"},
                {"Noche", "night_delete.png"},
                {"Bosque", "forest_delete.png"},
                {"Desierto", "desert_delete.png"},
                {"Tundra", "tundra_delete.png"},
                {"Valle", "valley_delete.png"},
                {"Oceano", "ocean_delete.png"}
            };
            this.CurrentTheme = this.BackgroundThemes.Keys.First();
            this.SetBackground(this.CurrentTheme);
            this.SetAddImage(this.CurrentTheme);
            this.SetRemoveImage(this.CurrentTheme);
        }

        public void SetCurrentTheme(string theme)
        {
            this.CurrentTheme = theme;
        }

        public void SetBackground(string theme)
        {
            BackgroundThemes.TryGetValue(theme, out var background);
            this.CurrentThemeBackground = background;
        }

        public void SetAddImage(string theme)
        {
            AddButtonThemes.TryGetValue(theme, out var add);
            this.CurrentThemeAddButton = add;
        }

        public void SetRemoveImage(string theme)
        {
            RemoveButtonThemes.TryGetValue(theme, out var remove);
            this.CurrentThemeRemoveButton = remove;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Device.BeginInvokeOnMainThread(() => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

    }
}
