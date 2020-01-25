using ARPEGOS.Models;
using ARPEGOS.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public class MainMenuViewModel : BaseViewModel
    {
        public static ObservableCollection<ItemGroup> MenuOptions { get; private set; }

        public MainMenuViewModel()
        {
            ObservableCollection<ItemGroup> MainMenuOptions = new ObservableCollection<ItemGroup>
            {
                new ItemGroup("Juegos")
                {
                    new SimpleListItem("Añadir Juego"),
                    new SimpleListItem("Seleccionar Juego")
                },
                new ItemGroup("Personaje")
                {
                    new SimpleListItem("Ver personaje"),
                    new SimpleListItem("Añadir personaje"),
                    new SimpleListItem("Modificar personaje"),
                    new SimpleListItem("Eliminar personaje")
                },
                new ItemGroup("Habilidades")
                {
                    new SimpleListItem("Cálculo de habilidad"),
                    new SimpleListItem("Tirada enfrentada"),
                }
            };
            MenuOptions = MainMenuOptions;
        }
    }
}
