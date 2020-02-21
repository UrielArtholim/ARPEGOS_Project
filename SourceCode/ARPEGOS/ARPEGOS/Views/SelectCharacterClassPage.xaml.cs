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
    public partial class SelectCharacterClassPage : ContentPage
    {
        readonly SelectCharacterClassViewModel viewModel;
        public SelectCharacterClassPage()
        {
            this.InitializeComponent();
            viewModel = new SelectCharacterClassViewModel();
            this.BindingContext = viewModel;
        }
    }
}