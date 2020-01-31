namespace ARPEGOS.ViewModels
{
    using ARPEGOS.Models;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Xamarin.Essentials;
    using Xamarin.Forms;

    public class MainMenuViewModel
    {
        public MainMenuViewModel()
        {
            ExpandCommand = new Command<ItemGroupViewModel>(t =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var a = this.ItemsList.FirstOrDefault(x => x.Title == t.Title);
                    a.Expanded = !a.Expanded;
                    this.UpdateListContent();
                });
            });
            this.ItemsList = new List<ItemGroupViewModel>
            {
                new ItemGroupViewModel("Juego") 
                { 
                    new ListItem("Seleccionar juego") 
                },
                new ItemGroupViewModel("Personaje")
                    {
                        new ListItem ("Añadir personaje"),
                        new ListItem ("Ver personaje"),
                        new ListItem ("Editar personaje"),
                        new ListItem ("Eliminar personaje")
                    },
                new ItemGroupViewModel("Habilidad")
                    {
                        new ListItem ("Calcular tirada de habilidad"),
                        new ListItem ("Calcular tirada enfentada")
                    }
            };
            this.Data = new ObservableCollection<ItemGroupViewModel>();
            this.UpdateListContent();
        }

        public ICommand ExpandCommand { get; }

        public List<ItemGroupViewModel> ItemsList { get; }

        public ObservableCollection<ItemGroupViewModel> Data { get; }

        private void UpdateListContent()
        {
            this.Data.Clear();
            foreach (var group in this.ItemsList)
            {
                var elements = new ItemGroupViewModel(group.Title, group.Expanded);
                if (group.Expanded)
                {
                    foreach (var element in group)
                    {
                        elements.Add(element);
                    }
                }
                this.Data.Add(elements);
            }
        }
    }
}
