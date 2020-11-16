using ARPEGOS.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Essentials;

namespace ARPEGOS.Models
{
    public class InfoGroup: ObservableCollection<Info>, INotifyPropertyChanged
    {

        #region Properties

        /// <summary>
        /// Short name of the group
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Name of the group formatted for UI
        /// </summary>
        public string FormattedTitle { get; internal set; }

        /// <summary>
        /// EventHandler to know when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public int Value { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Ctor that builds a group given the name and the state of expansion
        /// </summary>
        /// <param name="groupTitle">Name of the group</param>
        /// <param name="expanded">State of expansion of the group</param>
        public InfoGroup(string groupString, IEnumerable<Info> groupList = null) : base(groupList)
        {
            this.Title = groupString.Split('#').Last();
            this.FormattedTitle = FileService.FormatName(this.Title);
        }

        public InfoGroup(InfoGroup g)
        {
            this.Title = g.Title;
            this.FormattedTitle = g.FormattedTitle;
        }
        #endregion

        #region Methods       

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
