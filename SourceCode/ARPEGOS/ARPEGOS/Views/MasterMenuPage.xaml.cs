namespace ARPEGOS.Views
{
    using ARPEGOS.Models;
    using ARPEGOS.ViewModels;
    using System;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterMenuPage : ContentPage
    {
        public MasterMenuPage()
        {
            this.InitializeComponent();
            this.BindingContext = new MasterMenuViewModel();
        }
    }
}