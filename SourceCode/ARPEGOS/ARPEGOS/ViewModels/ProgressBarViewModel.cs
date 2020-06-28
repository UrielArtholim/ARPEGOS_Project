using ARPEGOS.ViewModels.Base;
using System;
using System.Linq;

namespace ARPEGOS.ViewModels
{
    public class ProgressBarViewModel: BaseViewModel
    {
        public double Maximum { get; private set; }
        public string Name { get; private set; }
        public string Progress { get; private set; }
        public double Current { get; private set; }
        public string Info { get { return string.Format("{0} / {1}", Current, Maximum); }}

        public ProgressBarViewModel(string name = "", int max = 1250, int progress = 625)
        {
            this.Name = name;
            this.Maximum = max;
            this.Current = progress;
            this.Progress = (progress/this.Maximum).ToString();
        }

    }
}
