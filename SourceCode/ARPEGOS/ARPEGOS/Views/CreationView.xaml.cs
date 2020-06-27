using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARPEGOS.Views
{
    using ARPEGOS.ViewModels;

    using Autofac;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreationView : ContentPage
    {
        public CreationView()
        {
            InitializeComponent();
            this.BindingContext = App.Container.Resolve<CreationViewModel>();
        }
    }
}