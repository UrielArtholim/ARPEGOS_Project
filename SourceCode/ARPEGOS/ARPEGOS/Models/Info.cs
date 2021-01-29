using ARPEGOS.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.Models
{
    public class Info: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string propertyName, formattedName, propertyValue;

        public string PropertyName
        {
            get => this.propertyName;
            set => SetProperty(ref this.propertyName, value);
        }

        public string FormattedName
        {
            get => this.formattedName;
            set => SetProperty(ref this.formattedName, value);
        }

        public string PropertyValue
        {
            get => this.propertyValue;
            set => SetProperty(ref this.propertyValue, value);
        }

        public Info(string name, string value)
        {
            this.PropertyName = name;
            this.FormattedName = FileService.FormatName(name);
            this.PropertyValue = value;
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
