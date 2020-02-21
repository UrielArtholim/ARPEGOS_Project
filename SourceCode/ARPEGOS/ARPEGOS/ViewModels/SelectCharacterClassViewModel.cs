namespace ARPEGOS.ViewModels
{
    using ARPEGOS.Models;
    using ARPEGOS.Views;
    using System;
    using System.Collections.Generic;
    using System.Text;
    public class SelectCharacterClassViewModel
    {
        bool FirstConstructorCall = true;

        public SelectCharacterClassViewModel()
        {
            bool GameAssigned = SystemControl.ActiveGameDB != null ? true : false;
            if (!GameAssigned)
            {
                NotifyGameNotSelected();
            }

            if (FirstConstructorCall)
            {
                GetCharacterName();
                FirstConstructorCall = !FirstConstructorCall;
            }
        }

        void NotifyGameNotSelected()
        {
            

        }
        async void GetCharacterName()
        {
            SystemControl.ActiveCharacter = await Xamarin.Forms.Application.Current.MainPage.DisplayPromptAsync("Creación de personaje", "Introduzca el nombre del personaje");
        }

        


    }
}
