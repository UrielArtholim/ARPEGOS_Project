using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public class ItemListViewModel: BaseViewModel
    {
        public List<Item> Data { get; set; }
        public ItemListViewModel()
        {
            Data = new List<Item>
            {
                new Item("asesino"),
                new Item("conjurador"),
                new Item("explorador"),
                new Item("guerrero"),
                new Item("guerrero_Acróbata"),
                new Item("guerrero_Conjurador"),
                new Item("guerrero_mentalista"),
                new Item("hechicero"),
                new Item("hechicero_mentalista"),
                new Item("ilusionista"),
                new Item("ladrón"),
                new Item("maestro_en_armas"),
                new Item("mentalista"),
                new Item("novel"),
                new Item("paladín"),
                new Item("paladín_oscuro"),
                new Item("sombra"),
                new Item("tao"),
                new Item("tecnicista"),
                new Item("warlock")
            };

        }
    }
}
