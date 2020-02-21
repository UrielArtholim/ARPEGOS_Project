namespace ARPEGOS.Views
{
    using ARPEGOS.Models;
    using ARPEGOS.ViewModels;
    using System;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppMasterMenuPage : ContentPage
    {
        readonly AppMasterMenuViewModel viewModel;
        public AppMasterMenuPage()
        {
            this.InitializeComponent();
            viewModel = new AppMasterMenuViewModel();
            this.BindingContext = viewModel;
        }
    }
}