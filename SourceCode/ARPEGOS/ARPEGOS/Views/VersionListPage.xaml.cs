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
    public partial class VersionListPage : ContentPage
    {
        readonly VersionListViewModel viewModel;
        public VersionListPage()
        {
            this.InitializeComponent();
            viewModel = new VersionListViewModel();
            this.
            BindingContext = viewModel;
        }
    }
}