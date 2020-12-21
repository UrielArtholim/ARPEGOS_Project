using ARPEGOS.Models;
using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public abstract class StageViewModel: INotifyPropertyChanged
    {
        public static ObservableCollection<Stage> CreationScheme = null;
        public static int CurrentStep { get; set; }
        public static double GeneralLimit { get; set; }
        public static double GeneralMaximum { get; set; }
        public static double GeneralProgress { get; set; }
        public static string GeneralLimitProperty { get; set; } = null;
        public static string RootStage { get; set; }
        public static bool ApplyOnCharacter { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public static void Reset()
        {
            CreationScheme = null;
            GeneralLimit = 0;
            GeneralMaximum = 0;
            GeneralProgress = 0;
            GeneralLimitProperty = null;
            RootStage = null;
            ApplyOnCharacter = false;
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
