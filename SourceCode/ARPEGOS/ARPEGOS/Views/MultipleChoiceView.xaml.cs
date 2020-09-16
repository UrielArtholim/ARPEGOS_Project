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
        void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var activeCheckBox = sender as CheckBox;
            var viewModel = this.BindingContext as MultipleChoiceViewModel;
            var activeItem = activeCheckBox.BindingContext as Item;
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var predicate = character.GetObjectPropertyAssociated(viewModel.CurrentStage.FullName, StageViewModel.ApplyOnCharacter);
            
            if(activeCheckBox.IsChecked == true)
            {

                Task.Run(async () => await MainThread.InvokeOnMainThreadAsync(() => 
                {
                    viewModel.StageProgressLabel -= activeItem.Value;
                    viewModel.StageProgress -= activeItem.Value / viewModel.StageLimit;
                    if (viewModel.HasGeneralLimit == true)
                    {
                        viewModel.GeneralProgressLabel -= activeItem.Value;
                        viewModel.GeneralProgress -= activeItem.Value / viewModel.StageLimit;
                    }
                }));                
                character.UpdateObjectAssertion(predicate, activeItem.FullName);
            }
            // Update assertion of item selected
            else
            {
                Task.Run(async () => await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    viewModel.StageProgressLabel += activeItem.Value;
                    viewModel.StageProgress += activeItem.Value / viewModel.StageLimit;
                    if (viewModel.HasGeneralLimit == true)
                    {
                        viewModel.GeneralProgressLabel += activeItem.Value;
                        viewModel.GeneralProgress += activeItem.Value / viewModel.StageLimit;
                    }
                }));
                

                // Remove assertion of item selected
                var characterAssertions = character.GetCharacterProperties();
                var predicateName = predicate.Split('#').Last();
                var itemName = activeItem.FullName.Split('#').Last();
                if(characterAssertions.ContainsKey(predicateName))
                {
                    characterAssertions.TryGetValue(predicateName, out var valueList);
                    if (valueList.Contains(itemName))
                        character.RemoveObjectProperty(predicate, activeItem.FullName);
                }
            }
            if(viewModel.HasGeneralLimit == true)
                Task.Run(async () => await viewModel.UpdateView());
        }
    }
}