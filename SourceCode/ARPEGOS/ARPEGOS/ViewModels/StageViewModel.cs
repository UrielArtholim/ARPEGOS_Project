using ARPEGOS.Models;
using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public abstract class StageViewModel: BaseViewModel
    {
        public static ObservableCollection<Stage> CreationScheme = new ObservableCollection<Stage>();
        public static int CurrentStep { get; set; }
        public static double GeneralLimit { get; set; } = 1;
        public static double GeneralProgress { get; set; } = 1;
        public static string GeneralLimitProperty { get; set; } = null;
        public static string RootStage { get; set; }
        public static bool ApplyOnCharacter { get; set; } = false;
    }
}
