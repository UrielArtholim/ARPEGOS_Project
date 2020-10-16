using ARPEGOS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARPEGOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SingleChoiceView : ContentPage
    {
        RadioButton lastChecked;
        public SingleChoiceView()
        {
            InitializeComponent();
            this.BindingContext = new SingleChoiceViewModel();
        }
        protected override bool OnBackButtonPressed()
        {
            if (StageViewModel.CreationScheme.Count() > 0 && StageViewModel.CurrentStep > 1)
                --StageViewModel.CurrentStep;
            else
                StageViewModel.CurrentStep = 0;
            return base.OnBackButtonPressed();
        }

        void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var activeRadioButton = sender as RadioButton;
            if (lastChecked != null)
                lastChecked.TextColor = Color.LightGreen;
            activeRadioButton.TextColor = Color.Black;
            if (activeRadioButton != lastChecked && lastChecked != null)
                lastChecked.IsChecked = false;
            lastChecked = activeRadioButton.IsChecked ? activeRadioButton : null;
            var viewModel = this.BindingContext as SingleChoiceViewModel;
            viewModel.SelectedItem = activeRadioButton.BindingContext as Item;
            if (lastChecked != null)
                viewModel.Continue = true;
            else
                viewModel.Continue = false;
        }
    }
}