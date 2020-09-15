using ARPEGOS.Models;
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
    public partial class MultipleChoiceView : ContentPage
    {
        public MultipleChoiceView()
        {
            InitializeComponent();
            this.BindingContext = new MultipleChoiceViewModel();
        }
        void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var activeCheckBox = sender as CheckBox;
            var viewModel = this.BindingContext as MultipleChoiceViewModel;
            var activeItem = activeCheckBox.BindingContext as Item;
            var SelectedItems = viewModel.SelectedItems;
            //TERMINAR
            if (activeCheckBox.IsChecked == true)
            {
                if (!SelectedItems.Contains(activeItem))
                {
                    SelectedItems.Add(activeItem);
                    viewModel.StageProgressLabel -= activeItem.Value;
                    viewModel.StageProgress -= (activeItem.Value/viewModel.StageLimit);
                    if (viewModel.HasGeneralLimit == true)
                    {
                        viewModel.GeneralProgressLabel -= activeItem.Value;
                        viewModel.GeneralProgress -= (activeItem.Value / viewModel.GeneralLimit);
                    }
                }
            }
            else
            {
                if(SelectedItems.Contains(activeItem))
                {
                    SelectedItems.Remove(activeItem);
                    viewModel.StageProgressLabel += activeItem.Value;
                    viewModel.StageProgress += (activeItem.Value / viewModel.StageLimit);
                    if (viewModel.HasGeneralLimit == true)
                    {
                        viewModel.GeneralProgressLabel += activeItem.Value;
                        viewModel.GeneralProgress += (activeItem.Value / viewModel.GeneralLimit);
                    }
                }
            }
        }
    }
}