using ARPEGOS.Helpers;
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
    public partial class CreationRootView: ContentPage
    {
        public CreationRootView ()
        {
            InitializeComponent();
            this.BindingContext = new CreationRootViewModel();
        }
    }
}