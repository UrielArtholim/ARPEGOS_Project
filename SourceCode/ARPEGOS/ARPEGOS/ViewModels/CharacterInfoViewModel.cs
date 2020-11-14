using ARPEGOS.Helpers;
using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public class CharacterInfoViewModel: BaseViewModel
    {
        #region Properties
        #region Private
        private ObservableCollection<string> selectableItems;
        private string characterName;

        #endregion

        #region Public
        public ObservableCollection<string> SelectableItems
        {
            get => this.selectableItems;
            set => SetProperty<ObservableCollection<string>>(ref this.selectableItems, value);
        }

        public string CharacterName
        {
            get => this.CharacterName;
            set => SetProperty<string>(ref this.characterName, value);
        }

        #endregion
        #endregion

        #region Constructor
        public CharacterInfoViewModel()
        {
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            this.SelectableItems = new ObservableCollection<string>();
            this.CharacterName = character.FormattedName;
            var ClassificationDictionary = new Dictionary<string, string>();

            var characterClassificationAssertions = character.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("Visualization"));
            if(characterClassificationAssertions.Count() > 0)
            {
                var enumerator = characterClassificationAssertions.GetEnumerator();
                while(enumerator.MoveNext())
                {
                    var assertion = enumerator.Current;
                    var propertyString = assertion.TaxonomySubject.ToString().Split('#').Last();
                    var propertyClassifier = assertion.TaxonomyObject.ToString().Split('^').First();

                    if (!ClassificationDictionary.ContainsKey(propertyString))
                        ClassificationDictionary.Add(propertyString, propertyClassifier);
                }

                var items = new ObservableCollection<string>();

                foreach (var classification in ClassificationDictionary.Values)
                {
                    var classificationFirstWord = classification.Split(':').First();
                    if (!items.Contains(classificationFirstWord))
                        items.Add(classificationFirstWord);
                }

                SelectableItems = new ObservableCollection<string>(items.OrderBy(item => item).ToList());
            }            
        }


        #endregion

        #region Methods



        #endregion
    }
}
