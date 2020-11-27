
namespace ARPEGOS.Views
{
    using ARPEGOS.Helpers;
    using ARPEGOS.Themes;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainViewMaster : ContentPage
    {
        public MainViewMaster()
        {
            this.InitializeComponent();
        }

        void OnToggled(object sender, ToggledEventArgs e)
        {
            Switch activeSwitch = sender as Switch;
            App.Current.Resources.MergedDictionaries.Clear();
            if(activeSwitch.IsToggled)
                App.Current.Resources.MergedDictionaries.Add(new DarkTheme());
            else
                App.Current.Resources.MergedDictionaries.Add(new LightTheme());
        }
    }
}