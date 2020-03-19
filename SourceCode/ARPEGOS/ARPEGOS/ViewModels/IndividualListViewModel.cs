namespace ARPEGOS.ViewModels
{
    using ARPEGOS.Models;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Input;

    public class IndividualListViewModel
    {
        public List<IndividualElement> Individuals { get; private set; }
        public IndividualListViewModel(ClassElement element)
        {
            var elementClass = element.Class;
            Individuals = SystemControl.ActiveGame.GetIndividualsOfClass(elementClass);
        }

        public ICommand SelectIndividual { get; }
    }
}
