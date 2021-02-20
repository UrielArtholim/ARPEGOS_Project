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
    public partial class ValuedGroupView : ContentPage
    {
        public ValuedGroupView()
        {
            InitializeComponent();
            this.BindingContext = new ValuedGroupViewModel();
        }
        protected override bool OnBackButtonPressed()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var viewModel = this.BindingContext as ValuedGroupViewModel;

            var stageLimitPropertyString = character.GetFullString(viewModel.StageLimitProperty, true);
            character.UpdateDatatypeAssertion(stageLimitPropertyString, viewModel.StageLimit.ToString());

            if (viewModel.HasGeneralLimit == true)
            {
                var generalLimitPropertyString = character.GetFullString(StageViewModel.GeneralLimitProperty, true);
                character.UpdateDatatypeAssertion(generalLimitPropertyString, viewModel.GeneralLimit.ToString());
            }

            if (StageViewModel.CreationScheme.Count() > 0 && StageViewModel.CurrentStep > 1)
                --StageViewModel.CurrentStep;
            else
                StageViewModel.CurrentStep = 0;

            return base.OnBackButtonPressed();
        }

        async void OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            var viewModel = this.BindingContext as ValuedGroupViewModel;
            //await Device.InvokeOnMainThreadAsync(() => viewModel.IsBusy = true);
            var entry = sender as Stepper;
            var item = entry.BindingContext as Item;
            var elementGroup = viewModel.Data.Where(group => group.Elements.Contains(item)).Single();
            var element = elementGroup.ElementAt(elementGroup.IndexOf(item));
            double NewValue, OldValue;

            NewValue = e.NewValue;
            OldValue = e.OldValue;

            double lowerProgressValue = viewModel.StageProgressLabel;
            if (viewModel.HasGeneralLimit == true)
                lowerProgressValue = Math.Min(viewModel.GeneralProgressLabel, viewModel.StageProgressLabel);

            double lowerLimit = viewModel.StageLimit;
            if (viewModel.HasGeneralLimit == true)
                lowerProgressValue = Math.Min(viewModel.GeneralLimit, viewModel.StageLimit);

            if (NewValue > OldValue)
            {
                if (lowerProgressValue >= item.Step)
                {
                    if (item.Value < lowerLimit)
                    {
                        ++item.Value;
                        if (viewModel.CurrentStage.EditStageLimit == true)
                        {
                            await Device.InvokeOnMainThreadAsync(() =>
                            {
                                viewModel.StageProgressLabel -= Convert.ToDouble(item.Step);
                                viewModel.StageProgress -= Convert.ToDouble(item.Step / viewModel.StageLimit);
                                if (viewModel.CurrentStage.EditGeneralLimit == true)
                                {
                                    viewModel.GeneralProgressLabel -= Convert.ToDouble(item.Step);
                                    viewModel.GeneralProgress -= Convert.ToDouble(item.Step / StageViewModel.GeneralMaximum);
                                }
                            });
                        }
                    }
                }
            }
            else if (OldValue > NewValue)
            {
                if (lowerProgressValue >= 0)
                {
                    if (item.Value > 0)
                    {
                        --item.Value;
                        if (viewModel.CurrentStage.EditStageLimit == true)
                        {
                            await Device.InvokeOnMainThreadAsync(() =>
                            {
                                viewModel.StageProgressLabel += Convert.ToDouble(item.Step);
                                viewModel.StageProgress += Convert.ToDouble(item.Step / viewModel.StageLimit);
                                if (viewModel.CurrentStage.EditGeneralLimit == true)
                                {
                                    viewModel.GeneralProgressLabel += Convert.ToDouble(item.Step);
                                    viewModel.GeneralProgress += Convert.ToDouble(item.Step / StageViewModel.GeneralMaximum);
                                }
                            });
                        }
                    }
                }
            }
            element.Value = item.Value;
            //await Device.InvokeOnMainThreadAsync(() => viewModel.IsBusy = false);
        }
    }
}