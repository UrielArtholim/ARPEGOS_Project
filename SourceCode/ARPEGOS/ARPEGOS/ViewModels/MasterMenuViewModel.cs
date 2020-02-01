namespace ARPEGOS.ViewModels
{
    using ARPEGOS.Models;
    using ARPEGOS.Views;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Input;
    using Xamarin.Essentials;
    using Xamarin.Forms;

    public class MasterMenuViewModel
    {
        public event EventHandler<PageType> PageSelected;

        public Page NextPage { get; private set; }
        const string home = "Inicio";
        const string selectGame = "Seleccionar juego";
        const string addCharacter = "Añadir personaje";
        const string viewCharacter = "Ver personaje";
        const string editCharacter = "Editar personaje";
        const string removeCharacter = "Eliminar personaje";
        const string characterSkillCalculator = "Calcular habilidad de personaje";
        const string confrontedSkillCalculator = "Calcular tirada enfentada";
        

        public MasterMenuViewModel()
        {
            ExpandCommand = new Command<ItemGroupViewModel>(itemgroup =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var selectedGroup = this.ItemsList.FirstOrDefault(currentVar => currentVar.Title == itemgroup.Title);
                    selectedGroup.Expanded = !selectedGroup.Expanded;
                    this.UpdateListContent();
                });
            });

            SelectPageCommand = new Command<ListItem>(listitem =>
            {
                    this.GetNextPageType(listitem);
            });

            this.ItemsList = new List<ItemGroupViewModel>
            {
                new ItemGroupViewModel("General") 
                {
                    new ListItem(home),
                    new ListItem(selectGame) 
                },
                new ItemGroupViewModel("Personaje")
                    {
                        new ListItem (addCharacter),
                        new ListItem (viewCharacter),
                        new ListItem (editCharacter),
                        new ListItem (removeCharacter)
                    },
                new ItemGroupViewModel("Habilidad")
                    {
                        new ListItem (characterSkillCalculator),
                        new ListItem (confrontedSkillCalculator)
                    }
            };
            this.Data = new ObservableCollection<ItemGroupViewModel>();
            this.NextPage = new WelcomePage();
            this.UpdateListContent();
        }

        private void GetNextPageType(ListItem item)
        {
            switch (item.ItemName)
            {
                case selectGame: PageSelected?.Invoke(this, PageType.GamesList); break;
                case addCharacter: PageSelected?.Invoke(this, PageType.CreateCharacter); break;
                case viewCharacter: PageSelected?.Invoke(this, PageType.ViewCharacter); break;
                case editCharacter: PageSelected?.Invoke(this, PageType.EditCharacter); break;
                case removeCharacter: PageSelected?.Invoke(this, PageType.RemoveCharacter); break;
                case characterSkillCalculator: PageSelected?.Invoke(this, PageType.OneSkillCalculator); break;
                case confrontedSkillCalculator: PageSelected?.Invoke(this, PageType.TwoSkillCalculator); break;
                default: PageSelected?.Invoke(this, PageType.Home); break;
            }
        }
                

        public ICommand ExpandCommand { get; }
        public ICommand SelectPageCommand { get; }
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
