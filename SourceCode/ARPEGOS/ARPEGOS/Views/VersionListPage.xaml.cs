namespace ARPEGOS.Views
{
    using ARPEGOS.ViewModels;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VersionListPage : ContentPage
    {
        readonly VersionListViewModel viewModel;
        public VersionListPage()
        {
            this.InitializeComponent();
            viewModel = new VersionListViewModel();
            this.BindingContext = viewModel;
        }
    }
}