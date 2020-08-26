using ARPEGOS.Helpers;
using ARPEGOS.Services;
using ARPEGOS.Services.Interfaces;
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
    public partial class SelectVersionView: ContentPage
    {
        public SelectVersionView (IDialogService dialogService)
        {
            InitializeComponent();
            this.BindingContext = new SelectVersionViewModel(dialogService);
        }

    }
}