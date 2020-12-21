using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.Models
{
    public class ThemeItem: INotifyPropertyChanged
    {
        private string theme;
        private bool isSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Theme
        {
            get => this.theme;
            set => SetProperty(ref this.theme, value);
        }
        public bool IsSelected
        {
            get => this.isSelected;
            set => SetProperty(ref this.isSelected, value);
        }

        public ThemeItem(string name, bool selected = false)
        {
            this.Theme = name;
            this.IsSelected = selected;
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
