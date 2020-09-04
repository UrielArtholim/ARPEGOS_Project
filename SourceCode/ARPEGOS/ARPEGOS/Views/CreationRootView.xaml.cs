using ARPEGOS.Helpers;
using ARPEGOS.ViewModels;
using Autofac;
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
    public partial class CreationRootView: ContentPage
    {
        CheckBox lastChecked;
        public CreationRootView ()
        {
            InitializeComponent();
            this.BindingContext = new CreationRootViewModel();
        }
        void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var activeCheckbox = sender as CheckBox;
            if (activeCheckbox != lastChecked && lastChecked != null)
                lastChecked.IsChecked = false;
            lastChecked = activeCheckbox.IsChecked ? activeCheckbox : null;
            var viewModel = this.BindingContext as CreationRootViewModel;
            viewModel.SelectedItem = activeCheckbox.BindingContext as Item;
            if (lastChecked != null)
                viewModel.Continue = true;
            else
                viewModel.Continue = false;
        }
    }
}