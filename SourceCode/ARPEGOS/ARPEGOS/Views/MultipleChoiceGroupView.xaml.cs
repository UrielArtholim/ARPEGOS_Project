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
    public partial class MultipleChoiceGroupView : ContentPage
    {
        public MultipleChoiceGroupView()
        {
            InitializeComponent();
            this.BindingContext = new MultipleChoiceGroupViewModel();
        }

        public void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {

        }
    }
}