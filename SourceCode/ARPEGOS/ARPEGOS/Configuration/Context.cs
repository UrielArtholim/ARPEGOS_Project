
namespace ARPEGOS.Configuration
{
    using ARPEGOS.Services;
    using ARPEGOS.Views;
    using Xamarin.Essentials;
    using Xamarin.Forms;

    public class Context
    {
        private CharacterOntologyService _currentCharacter;
        public GameOntologyService CurrentGame { get; set; }

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
