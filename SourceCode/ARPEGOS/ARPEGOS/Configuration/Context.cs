
namespace ARPEGOS.Configuration
{
    using ARPEGOS.Helpers;
    using ARPEGOS.Services;
    using ARPEGOS.Views;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Xamarin.Essentials;
    using Xamarin.Forms;

    public class Context
    {
        private CharacterOntologyService _currentCharacter;
        private ThemeHelper _themes = new ThemeHelper();
        public DialogService Dialog = new DialogService();
        public GameOntologyService CurrentGame { get; set; }
        public ThemeHelper Themes 
        { 
            get => _themes; 
        }

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
