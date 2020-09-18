using ARPEGOS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARPEGOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ValuedView : ContentPage
    {
        public ValuedView()
        {
            InitializeComponent();
            this.BindingContext = new ValuedViewModel();
        }

        void OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            var viewModel = this.BindingContext as ValuedViewModel;
            var entry = sender as Stepper;
            var item = entry.BindingContext as Item;
            double NewValue, OldValue;

            NewValue = e.NewValue;
            OldValue = e.OldValue;

            if (viewModel.ElementLimit != null)
            {
                var lowerLimit = viewModel.StageProgressLabel;
                if (viewModel.HasGeneralLimit == true)
                    lowerLimit = Math.Min(viewModel.GeneralProgressLabel, lowerLimit);
                lowerLimit = Math.Min(lowerLimit, Convert.ToDouble(viewModel.ElementLimit));
                foreach (var element in viewModel.Data)
                {
                    if (lowerLimit > 0)
                    {
                        if (lowerLimit == 1)
                        {
                            element.IsEnabled = true;
                            element.Maximum = element.Value + 1;
                        }
                    }
                    else if (lowerLimit == 0)
                    {
                        if (element.Value != 0)
                            element.Maximum = element.Value;
                        else
                            element.IsEnabled = false;
                    }
                }
            }

            if (NewValue > OldValue)
            {
                ++item.Value;
                if (viewModel.CurrentStage.EditStageLimit == true)
                {
                    Task.Run(async () => await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        viewModel.StageProgressLabel -= Convert.ToDouble(item.Step);
                        viewModel.StageProgress -= Convert.ToDouble(item.Step / viewModel.StageLimit);
                        if (viewModel.CurrentStage.EditGeneralLimit == true)
                        {
                            viewModel.GeneralProgressLabel -= Convert.ToDouble(item.Step);
                            viewModel.GeneralProgress -= Convert.ToDouble(item.Step / viewModel.GeneralLimit);
                        }
                    }));
                }
            }
            else if (OldValue > NewValue)
            {
                if (viewModel.CurrentStage.EditStageLimit == true)
                {
                    Task.Run(async () => await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        --item.Value;
                        viewModel.StageProgressLabel += Convert.ToDouble(item.Step);
                        viewModel.StageProgress += Convert.ToDouble(item.Step / viewModel.StageLimit);
                        if (viewModel.CurrentStage.EditGeneralLimit == true)
                        {
                            viewModel.GeneralProgressLabel += Convert.ToDouble(item.Step);
                            viewModel.GeneralProgress += Convert.ToDouble(item.Step / viewModel.GeneralLimit);
                        }
                    }));
                }
            }
        }
    }
}