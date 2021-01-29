
namespace ARPEGOS.Views
{
    using ARPEGOS.Helpers;
    using ARPEGOS.Themes;
    using ARPEGOS.ViewModels;
    using Autofac;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainViewMaster : ContentPage
    {
        public MainViewMaster()
        {
            this.InitializeComponent();
            this.BindingContext = DependencyHelper.Container.Resolve<MasterViewModel>();
        }
    }
}