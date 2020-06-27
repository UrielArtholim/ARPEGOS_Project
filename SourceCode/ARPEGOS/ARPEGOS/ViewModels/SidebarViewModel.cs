using ARPEGOS.ViewModels.Base;

namespace ARPEGOS.ViewModels
{
    public class SidebarViewModel: BaseViewModel
    {
        public string SidebarName { get; private set; }
        public string SidebarValue { get; private set; }

        public SidebarViewModel(string DefaultName = "PD Disponibles",string DefaultValue = "1")
        {
            SidebarName = DefaultName;
            SidebarValue = DefaultValue;
        }
    }
}
