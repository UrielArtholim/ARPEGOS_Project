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
    public partial class MainMenuPage : ContentPage
    {
        readonly MainMenuViewModel mainMenuViewModel;
        public MainMenuPage()
        {
            InitializeComponent();
            mainMenuViewModel = new MainMenuViewModel();
            BindingContext = mainMenuViewModel;
        }
    }
}