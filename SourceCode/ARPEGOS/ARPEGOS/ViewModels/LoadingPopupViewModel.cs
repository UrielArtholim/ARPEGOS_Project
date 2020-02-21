using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public class LoadingPopupViewModel
    {
        string LoadingText { get; set; }
        public LoadingPopupViewModel(string text)
        {
            LoadingText = text;
        }
    }
}
