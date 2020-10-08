
namespace ARPEGOS.Controls
{
    using ARPEGOS.ViewModels;
    using System;
    using System.Linq;
    using Xamarin.Forms;

    public class CustomSlider : Slider
    {

        public static readonly BindableProperty StepProperty =
            BindableProperty.Create(nameof(Step), typeof(int), typeof(CustomSlider), 1, BindingMode.OneTime);

        public CustomSlider()
        {
            this.ValueChanged += this.OnSliderValueChanged;

        }

        public int Step { get; set; }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            var newStep = Math.Round(e.NewValue / Step);
            this.Value = newStep * Step;

            var slider = sender as CustomSlider;
            var item = slider.BindingContext as Item;

            var currentStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
            if(!currentStage.IsGrouped)
            {
                var viewModel = App.Navigation.NavigationStack.Last().BindingContext as ValuedViewModel;
                item.Value = slider.Value * viewModel.CurrentLimit;
                if (e.NewValue > e.OldValue)
                {
                    viewModel.GeneralProgress -= e.NewValue * viewModel.CurrentLimit / viewModel.GeneralLimit;
                    viewModel.StageProgress -= e.NewValue * viewModel.CurrentLimit / viewModel.StageLimit;   
                }
                else if (e.NewValue < e.OldValue)
                {
                    viewModel.GeneralProgress += e.NewValue * viewModel.CurrentLimit / viewModel.GeneralLimit;
                    viewModel.StageProgress += e.NewValue * viewModel.CurrentLimit / viewModel.StageLimit;
                }
            }
            else
            {
                var viewModel = App.Navigation.NavigationStack.Last().BindingContext as ValuedGroupViewModel;
                /*item.Value = slider.Value * viewModel.CurrentLimit;
                if (e.NewValue > e.OldValue)
                {
                    viewModel.GeneralProgress -= e.NewValue * viewModel.CurrentLimit / viewModel.GeneralLimit;
                    viewModel.StageProgress -= e.NewValue * viewModel.CurrentLimit / viewModel.StageLimit;
                }
                else if (e.NewValue < e.OldValue)
                {
                    viewModel.GeneralProgress += e.NewValue * viewModel.CurrentLimit / viewModel.GeneralLimit;
                    viewModel.StageProgress += e.NewValue * viewModel.CurrentLimit / viewModel.StageLimit;
                }*/
            }
        }
    }
}
