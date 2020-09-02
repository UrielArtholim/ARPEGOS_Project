using ARPEGOS.Models;
using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public class StageViewModel: BaseViewModel
    {
        private int stageCounter;
        public static ObservableCollection<Stage> CreationScheme = new ObservableCollection<Stage>();

        public StageViewModel(int counter)
        {
            this.stageCounter = counter;
        }
    }
}
