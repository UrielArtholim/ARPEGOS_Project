
namespace ARPEGOS.ViewModels.Base
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    using Xamarin.Essentials;
    using Xamarin.Forms;

    public class BaseViewModel : INotifyPropertyChanged, INotifyCollectionChanged
    {
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private bool _isBusy;

        private string _title;

        protected bool initialized;

        /// <summary>
        /// Gets or Sets IsBusy property
        /// </summary>
        public bool IsBusy
        {
            get => this._isBusy;
            set => this.SetProperty(ref this._isBusy, value);
        }

        /// <summary>
        /// Gets or Sets the Title
        /// </summary>
        public string Title
        {
            get => this._title;
            set => this.SetProperty(ref this._title, value); 
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Device.BeginInvokeOnMainThread(() => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        protected virtual void OnCollectionChanged()
        {
            Device.BeginInvokeOnMainThread(() => this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace)));
        }

        protected void RefreshCollection()
        {
            this.OnCollectionChanged();
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
