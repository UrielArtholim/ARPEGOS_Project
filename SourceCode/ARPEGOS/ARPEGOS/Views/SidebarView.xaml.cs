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
    using Autofac;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SidebarView : ContentView
    {
        public SidebarView()
        {
            InitializeComponent();
            this.BindingContext = App.Container.Resolve<SidebarViewModel>();
        }
    }
}