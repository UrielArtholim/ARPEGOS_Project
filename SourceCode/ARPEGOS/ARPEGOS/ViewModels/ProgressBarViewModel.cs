using ARPEGOS.ViewModels.Base;

namespace ARPEGOS.ViewModels
{
    public class ProgressBarViewModel: BaseViewModel
    {
        public string ProgressBarName { get; private set; }
        public string ProgressBarValue { get; private set; }

        public ProgressBarViewModel(string DefaultName = "PD Disponibles",string DefaultValue = "1")
        {
            ProgressBarName = DefaultName;
            ProgressBarValue = DefaultValue;
        }
    }
}
