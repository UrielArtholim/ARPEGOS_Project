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
    public partial class ValuedGroupView : ContentPage
    {
        public ValuedGroupView()
        {
            InitializeComponent();
            this.BindingContext = new ValuedGroupViewModel();
        }

        public void OnValueChanged(object sender, ValueChangedEventArgs e)
        {

        }
    }
}