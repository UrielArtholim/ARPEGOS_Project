
namespace ARPEGOS.Controls
{
    using System;

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
            int newStep = Convert.ToInt32(Math.Round(e.NewValue / this.Step));
            this.Value = newStep * this.Step;
        }
    }
}
