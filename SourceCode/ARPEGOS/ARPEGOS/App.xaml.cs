
namespace ARPEGOS
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using ARPEGOS.Helpers;
    using ARPEGOS.Services;
    using ARPEGOS.Views;

    using Xamarin.Essentials;
    using Xamarin.Forms;

    public partial class App : Application
    {
        public static INavigation Navigation { get; set; }        

        public App()
        {
            this.InitializeComponent();
            _ = new DependencyHelper();

            Device.SetFlags(new string[] { "RadioButton_Experimental", "AppTheme_Experimental" });
            DependencyHelper.CurrentContext.AppMainView = new MainView();
            App.Navigation = DependencyHelper.CurrentContext.AppMainView?.Detail?.Navigation;
            this.MainPage = DependencyHelper.CurrentContext.AppMainView;
        }        
    }
}
