namespace ARPEGOS.ViewModels
{
    using ARPEGOS.Models;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class HomePageViewModel
    {
        public string CurrentGameName { get; private set; }
        public string CurrentGameVersion { get; private set; }

        public HomePageViewModel()
        {
            CurrentGameName = "Juego activo: " + SystemControl.GetActiveGame();
            CurrentGameVersion = "Versión: " +SystemControl.GetActiveVersion();

        }
    }
}
