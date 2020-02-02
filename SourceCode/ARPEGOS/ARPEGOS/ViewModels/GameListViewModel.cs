using ARPEGOS.Interfaces;
using ARPEGOS.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    class GameListViewModel
    {
        public ObservableCollection<ListItem> GameList { get; private set; }
        public GameListViewModel() 
        {
            SystemControl.UpdateGames();
            GameList = SystemControl.GetGameList();
        }
    }
}
