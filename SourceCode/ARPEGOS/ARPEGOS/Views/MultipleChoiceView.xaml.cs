using ARPEGOS.Helpers;
using ARPEGOS.Models;
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
    public partial class MultipleChoiceView : ContentPage
    {
        public MultipleChoiceView()
        {
            InitializeComponent();
            this.BindingContext = new MultipleChoiceViewModel();
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
            await Task.Run(async () => await OperateCheck(sender, e));
        }

        private async Task OperateCheck(object sender, CheckedChangedEventArgs e)
        {
            var viewModel = this.BindingContext as MultipleChoiceViewModel;
            await MainThread.InvokeOnMainThreadAsync(() => viewModel.IsBusy = true);
            var activeCheckBox = sender as CheckBox;
            var activeItem = activeCheckBox.BindingContext as Item;
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var predicate = character.GetObjectPropertyAssociated(viewModel.CurrentStage.FullName, activeItem, StageViewModel.ApplyOnCharacter);
            if (activeCheckBox.IsChecked == true)
            {
                activeItem.IsSelected = true;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    viewModel.StageProgressLabel -= activeItem.Value;
                    viewModel.StageProgress -= activeItem.Value / viewModel.StageLimit;
                    if (viewModel.HasGeneralLimit == true)
                    {
                        viewModel.GeneralProgressLabel -= activeItem.Value;
                        viewModel.GeneralProgress -= activeItem.Value / StageViewModel.GeneralMaximum;
                    }
                });
                character.UpdateObjectAssertion(predicate, activeItem.FullName);
            }
            // Update assertion of item selected
            else
            {
                activeItem.IsSelected = false;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    viewModel.StageProgressLabel += activeItem.Value;
                    viewModel.StageProgress += activeItem.Value / viewModel.StageLimit;
                    if (viewModel.HasGeneralLimit == true)
                    {
                        viewModel.GeneralProgressLabel += activeItem.Value;
                        viewModel.GeneralProgress += activeItem.Value / StageViewModel.GeneralMaximum;
                    }
                });
                // Remove assertion of item selected
                var characterAssertions = character.GetCharacterProperties();
                var predicateName = predicate.Split('#').Last();
                var characterPredicate = $"{character.Context}{predicateName}";
                var itemName = activeItem.FullName.Split('#').Last();
                var characterItem = $"{character.Context}{itemName}";

                if (characterAssertions.ContainsKey(characterPredicate))
                {
                    characterAssertions.TryGetValue(characterPredicate, out var valueList);
                    if (valueList.Contains(characterItem))
                        character.RemoveObjectProperty(characterPredicate, characterItem);
                }
            }
            await viewModel.UpdateView();
            await MainThread.InvokeOnMainThreadAsync(() => viewModel.IsBusy = false);
        }
    }
}