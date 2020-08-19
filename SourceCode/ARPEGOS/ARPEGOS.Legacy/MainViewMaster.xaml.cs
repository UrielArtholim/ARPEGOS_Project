namespace ARPEGOS.Views
{
    using ARPEGOS.ViewModels;

    using Autofac;

    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainViewMaster : ContentPage
    {
        public ListView ListView;

        public MainViewMaster()
        {
            this.InitializeComponent();
            this.BindingContext = App.Container.Resolve<ProgressBarViewModel>();
        }
    }
}