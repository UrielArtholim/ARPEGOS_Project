using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public class DetailInfoViewModel: BaseViewModel
    {
        private ObservableCollection<InfoGroup> data;
        private string viewName;

        public ObservableCollection<InfoGroup> Data
        {
            get => this.data;
            set => SetProperty(ref this.data, value);
        }
        public string ViewName
        {
            get => this.viewName;
            set => SetProperty(ref this.viewName, value);
        }

        public DetailInfoViewModel(string item)
        {
            this.ViewName = item;
            var character = DependencyHelper.CurrentContext.CurrentCharacter;
            var ClassificationDictionary = new Dictionary<string, string>();
            var datalist = new ObservableCollection<InfoGroup>();

            var characterClassificationAssertions = character.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.Where(entry => entry.TaxonomyPredicate.ToString().Contains("Visualization"));
            if (characterClassificationAssertions.Count() > 0)
            {
                var enumerator = characterClassificationAssertions.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var assertion = enumerator.Current;
                    var propertyString = assertion.TaxonomySubject.ToString();
                    var propertyClassifier = assertion.TaxonomyObject.ToString().Split('^').First();

                    if(propertyClassifier.Contains(item))
                        if (!ClassificationDictionary.ContainsKey(propertyString))
                            ClassificationDictionary.Add(propertyString, propertyClassifier);
                }

                SortedSet<string> PropertyGroups = new SortedSet<string>();
                foreach(var classification in ClassificationDictionary.Values)
                {
                    InfoGroup currentGroup = null;
                    var infoGroupName = classification;
                    if (infoGroupName.Contains(':'))
                    {
                        var nameText = infoGroupName.Replace(item, "");
                        var nameList = nameText.Split(':').ToList();
                        if (nameList.Count() > 1)
                            infoGroupName = $"{nameList.ElementAt(0)}{nameList.ElementAt(1)}".Trim();
                        else
                            infoGroupName = $"{nameList.ElementAt(0)}";                                                                         
                    }

                    if (!PropertyGroups.Contains(infoGroupName))
                    {
                        currentGroup = new InfoGroup(infoGroupName, new ObservableCollection<Info>());
                        PropertyGroups.Add(infoGroupName);
                    }
                    else
                        currentGroup = datalist.Where(group => group.Title == infoGroupName).Single();

                    foreach (var propertyString in ClassificationDictionary.Keys)
                    {
                        ClassificationDictionary.TryGetValue(propertyString, out string propertyClassification);
                        if (propertyClassification == classification)
                        {
                            var property = character.Ontology.Model.PropertyModel.SelectProperty(propertyString);
                            var propertyAssertion = character.Ontology.Data.Relations.Assertions.SelectEntriesByPredicate(property);
                            if (propertyAssertion.Count() == 1)
                            {
                                var propertyValueString = propertyAssertion.Single().TaxonomyObject.ToString();
                                var propertyValue = string.Empty;
                                if (character.CheckObjectProperty(propertyString))
                                    propertyValue = propertyValueString.Split('#').Last();
                                else if (character.CheckDatatypeProperty(propertyString))
                                    propertyValue = propertyValueString.Split('^').First();

                                var infoName = propertyString.Split('#').Last().ToLower().Replace("tiene", "").Replace("per_", "").Replace("total", "");
                                Info propertyInfo = new Info(FileService.FormatName(infoName), FileService.FormatName(propertyValue));
                                if ((currentGroup.Where(item => item.PropertyName == propertyInfo.PropertyName).Count() < 1))
                                    currentGroup.Add(propertyInfo);
                                else
                                    if(currentGroup.Where(item => item.PropertyName == propertyInfo.PropertyName && item.PropertyValue == propertyInfo.PropertyValue).Count() < 1)
                                        currentGroup.Add(propertyInfo);
                            }
                            else
                            {
                                Debug.WriteLine("Mas de un elemento");
                            }
                        }
                    }
                    if (datalist.Where(group => group.Title == currentGroup.Title).Count() == 0)
                        datalist.Add(currentGroup);
                    else
                        RefreshCollection();
                }
                Data = new ObservableCollection<InfoGroup>(datalist);
            }
        }
    }
}
