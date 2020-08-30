using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;

namespace ARPEGOS.Controls
{
    public class CustomProgressBar: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string name;
        private double maximum, progress, current;

        public string Name
        {
            get => name;
            set => this.SetProperty(ref this.name, value);
        }
        public double Maximum 
        {
            get => maximum;
            set => this.SetProperty(ref this.maximum, value); 
        }
        
        public double Progress
        {
            get => progress;
            set => this.SetProperty(ref this.progress, value);
        }
        public double Current
        {
            get => current;
            set => this.SetProperty(ref this.current, value);
        }

        public string Info { get { return string.Format("{0} / {1}", Current, Maximum); } }

        public CustomProgressBar (string name = "Point Counter", double max = 0, double progress = 0)
        {
            this.Name = name;
            this.Maximum = max;
            this.Current = progress;
            this.Progress = (progress / max);
        }

        public CustomProgressBar ()
        {
            this.Name = "ProgressBar";
            this.Maximum = 0;
            this.Current = 0;
            this.Progress = 0; //(progress / max);
        }

        protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
        {
            MainThread.BeginInvokeOnMainThread(() => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        protected bool SetProperty<T> (ref T storage, T value, [CallerMemberName] string propertyName = null)
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
