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
    public partial class SliderItemView : ContentView
    {
        public SliderItemView()
        {
            InitializeComponent();
            this.BindingContext = App.Container.Resolve<SliderItemViewModel>();
            
            UserInputSlider.ValueChanged += OnSliderValueChanged;
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            int step = ((SliderItemViewModel)this.BindingContext).Step;
            int newStep = Convert.ToInt32(Math.Round(e.NewValue / step));
            UserInputSlider.Value = newStep * step;
        }
    }
}