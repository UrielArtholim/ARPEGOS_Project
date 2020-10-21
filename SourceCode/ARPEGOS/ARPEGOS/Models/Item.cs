using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;

namespace ARPEGOS
{
    /// <summary>
    /// Item represents a generic element obtained from an ontology
    /// </summary>
    public class Item : INotifyPropertyChanged
    {
        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Full name of the element
        /// </summary>
        public string FullName { get; internal set; }

        /// <summary>
        /// Short name of the element
        /// </summary>
        public string ShortName { get; internal set; }


        /// <summary>
        /// Name of the element formatted for UI
        /// </summary>
        public string FormattedName { get; internal set; }

        /// <summary>
        /// Name of the class to which the element belongs
        /// </summary>
        public string Class { get; internal set; }

        /// <summary>
        /// Description of the element
        /// </summary>
        public string Description { get; internal set; }

        public bool HasDescription { get; internal set; }

        private double step, maximum;
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref this.isSelected, value);
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set => SetProperty(ref this.isEnabled, value);
        }
        private double value;
        public double Value
        {
            get => value;
            set => SetProperty(ref this.value, value);
        }

        public double Step
        {
            get => this.step;
            set => SetProperty(ref this.step, value);
        }

        public double Maximum
        {
            get => this.maximum;
            set => SetProperty(ref this.maximum, value);
        }


        #endregion

        #region Constructors
        public Item(string elementString, string description = "This is a description", string Class = "No class available", double elementValue = 1, double elementMaximum = 1)
        {
            FullName = elementString;
            ShortName = elementString.Split('#').Last();
            this.Class = Class;
            IsEnabled = true;
            if(this.Class != string.Empty)
                if (ShortName.Contains(Class))
                    if (!ShortName.Contains("_de_"))
                        ShortName = ShortName.Replace(Class,"").Trim();
            FormattedName = ShortName.Replace("Per_","").Replace("_Total","").Replace('_',' ').Trim();
            Description = description;
            this.Value = elementValue;
            this.Maximum = elementMaximum;
            this.Step = elementValue;
            if (string.IsNullOrEmpty(Description))
                HasDescription = false;
            else
                HasDescription = true;
        }

        public Item(Item i)
        {
            this.FullName = i.FullName;
            this.ShortName = i.ShortName;
            this.FormattedName = i.FormattedName;
            this.Class = i.Class;
            this.Value = i.Value;
            this.Maximum = i.Maximum;
            this.Step = i.Step;
            this.HasDescription = i.HasDescription;
            this.Description = i.Description;
        }
        #endregion

        #region Methods 
        public void ShowItem(string tree)
        {
            Console.WriteLine( tree + "====>" + FormattedName);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            MainThread.BeginInvokeOnMainThread(() => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
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
        #endregion
    }
}
