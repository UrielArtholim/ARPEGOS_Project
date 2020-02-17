namespace ARPEGOS.Views
{
    using ARPEGOS.ViewModels;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        readonly HomePageViewModel viewModel;
        public HomePage()
        {
            this.InitializeComponent();
            viewModel = new HomePageViewModel();
            this.BindingContext = viewModel;
        }
    }
}