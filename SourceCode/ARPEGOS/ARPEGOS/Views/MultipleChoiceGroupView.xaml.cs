using ARPEGOS.Helpers;
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
    public partial class MultipleChoiceGroupView : ContentPage
    {
        public MultipleChoiceGroupView()
        {
            InitializeComponent();
            this.BindingContext = new MultipleChoiceGroupViewModel();
        }

        protected override bool OnBackButtonPressed()
        {
            if (StageViewModel.CreationScheme.Count() > 0 && StageViewModel.CurrentStep > 1)
                --StageViewModel.CurrentStep;
            else
                StageViewModel.CurrentStep = 0;
            return base.OnBackButtonPressed();
        }

        async void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            await Task.Run(() => OperateCheck(sender, e));    
        }

        private async Task OperateCheck(object sender, CheckedChangedEventArgs e)
        {
            var activeCheckBox = sender as CheckBox;
            var viewModel = this.BindingContext as MultipleChoiceGroupViewModel;
            await MainThread.InvokeOnMainThreadAsync(() => viewModel.IsBusy = true);

            var activeItem = activeCheckBox.BindingContext as Item;
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var predicate = character.GetObjectPropertyAssociated(viewModel.CurrentStage.FullName, activeItem, StageViewModel.ApplyOnCharacter);
            if (activeCheckBox.IsChecked == true)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    viewModel.StageProgressLabel -= activeItem.Value;
                    viewModel.StageProgress -= activeItem.Value / viewModel.StageLimit;
                    if (viewModel.HasGeneralLimit == true)
                    {
                        viewModel.GeneralProgressLabel -= activeItem.Value;
                        viewModel.GeneralProgress -= activeItem.Value / viewModel.StageLimit;
                    }
                });
                character.UpdateObjectAssertion(predicate, activeItem.FullName);
            }
            // Update assertion of item selected
            else
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    viewModel.StageProgressLabel += activeItem.Value;
                    viewModel.StageProgress += activeItem.Value / viewModel.StageLimit;
                    if (viewModel.HasGeneralLimit == true)
                    {
                        viewModel.GeneralProgressLabel += activeItem.Value;
                        viewModel.GeneralProgress += activeItem.Value / viewModel.StageLimit;
                    }
                });

                // Remove assertion of item selected
                var characterAssertions = character.GetCharacterProperties();
                var predicateName = predicate.Split('#').Last();
                var itemName = activeItem.FullName.Split('#').Last();
                if (characterAssertions.ContainsKey(predicateName))
                {
                    characterAssertions.TryGetValue(predicateName, out var valueList);
                    if (valueList.Contains(itemName))
                        character.RemoveObjectProperty(predicate, activeItem.FullName);
                }
            }
            await viewModel.UpdateView();
            await MainThread.InvokeOnMainThreadAsync(() => viewModel.IsBusy = false);
        }
    }
}