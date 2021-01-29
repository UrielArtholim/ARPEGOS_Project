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
    public partial class OptionsView: ContentPage
    {
        public OptionsView ()
        {
            InitializeComponent();
            this.BindingContext = new OptionsViewModel();
        }
    }
}