
namespace ARPEGOS.Configuration
{
    using ARPEGOS.Helpers;
    using ARPEGOS.Services;
    using ARPEGOS.Views;
    using Xamarin.Essentials;
    using Xamarin.Forms;

    public class Context
    {
        private CharacterOntologyService _currentCharacter;
        private ThemeHelper themes = new ThemeHelper();
        public GameOntologyService CurrentGame { get; set; }
        public ThemeHelper Themes { get => themes; }

        public CharacterOntologyService CurrentCharacter
        {
            get => this._currentCharacter;
            set
            {
                this._currentCharacter?.Save();
                this._currentCharacter = value;
            }
        }

        public MainView AppMainView { get; set; }
    }
}
