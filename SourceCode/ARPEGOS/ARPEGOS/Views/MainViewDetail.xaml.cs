
namespace ARPEGOS.Views
{
    using ARPEGOS.Helpers;
    using ARPEGOS.ViewModels;

    using Autofac;
    using System;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainViewDetail : ContentPage
    {
        public MainViewDetail()
        {
            this.InitializeComponent();
            this.BindingContext = DependencyHelper.Container.Resolve<MainViewModel>();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (this.BindingContext is MainViewModel vm)
            {
                await vm.Init();
            }
        }
        void OnBackgroundChanged(object sender, EventArgs e)
        {
            Image image = sender as Image;
            image.Source = DependencyHelper.CurrentContext.Themes.CurrentThemeBackground;
        }
    }
}