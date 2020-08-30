using ARPEGOS.Helpers;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARPEGOS.ViewModels
{
    public class CreationRootViewModel: BaseViewModel
    {
        public ObservableCollection<Item> Data { get; private set; }

        public CreationRootViewModel()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var game = DependencyHelper.CurrentContext.CurrentGame;
            var rootStage = game.GetCreationSchemeRootClass();
            Data = new ObservableCollection<Item>(character.GetIndividuals(rootStage));
        }
    }
}
