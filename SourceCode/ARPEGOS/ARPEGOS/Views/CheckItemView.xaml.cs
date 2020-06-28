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
    public partial class CheckItemView : ContentView
    {
        public CheckItemView()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            var a = this.BindingContext;
            base.OnBindingContextChanged();
        }
    }
}