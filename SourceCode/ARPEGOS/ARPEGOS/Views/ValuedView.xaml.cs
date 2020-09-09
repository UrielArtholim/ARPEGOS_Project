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

        void OnValueChanged(object sender , ValueChangedEventArgs e)
        {

            var viewModel = this.BindingContext as ValuedViewModel;
            var entry = sender as Stepper;
            var item = entry.BindingContext as Item;
            double NewValue, OldValue;

            NewValue = e.NewValue;
            OldValue = e.OldValue;            

            if(NewValue > OldValue)
            {
                item.Value += item.Step;
                Task.Run(async () => await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    viewModel.StageProgressLabel -= item.Step;
                    viewModel.StageProgress -= item.Step;
                    if (viewModel.HasGeneralLimit)
                    {
                        viewModel.GeneralProgressLabel -= item.Step;
                        viewModel.GeneralProgress -= item.Step;
                    }
                }));
                
            }
            else if (OldValue > NewValue)
            {
                Task.Run(async () => await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    item.Value -= item.Step;
                    viewModel.StageProgressLabel += item.Step;
                    viewModel.StageProgress += item.Step;
                    if (viewModel.HasGeneralLimit)
                    {
                        viewModel.GeneralProgressLabel += item.Step;
                        viewModel.GeneralProgress += item.Step;
                    }
                }));
                
            }
            
        }
    }
}