using Arpegos_Test.Views;
using ARPEGOS_Test.Models;
using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arpegos_Test.ViewModels
{
    public class MultipleChoiceDynamicLimitViewModel
    {
        public MultipleChoiceDynamicLimitViewModel(string stage, float? LimitValue, out float? ReturnLimitValue)
        {
            ReturnLimitValue = null;
            Group StageGroup = new Group(stage);
            string grouptype = StageGroup.GroupList.GetType().ToString();
            if (grouptype.Contains("Item"))
            {
                string AvailablePointsLimit = Program.Game.GetAvailablePoints(stage, out float? AvailablePoints);
                bool hasAvailablePointsLimit = AvailablePointsLimit != null;

                if ((AvailablePointsLimit == null) || (AvailablePoints != null && AvailablePoints > 0))
                {
                    ReturnLimitValue = LimitValue;
                    string finish = "Siguiente";
                    string choice = null;
                    List<string> choices = new List<string>();

                    string LimitName = Program.Game.GetLimit(stage, out float? stageLimitValue);
                    bool hasLimit = LimitName != null;

                    List<string> AvailableItems = Program.Game.CheckAvailableOptions(stage, hasAvailablePointsLimit, AvailablePoints, LimitValue);
                    if (AvailableItems.Count() > 0)
                    {
                        while (choice == null || choice.ToLower() != finish.ToLower())
                        {
                            if (choice != null)
                            {
                                Program.ShowStage(stage, Program.Counter);
                                AvailableItems = Program.Game.CheckAvailableOptions(stage, hasLimit, AvailablePoints, LimitValue);
                            }

                            ShowAvailableOptions(stage, AvailableItems);
                            Console.WriteLine("\n\n");

                            if (choices.Count > 0)
                            {
                                Console.WriteLine("\n\nElementos seleccionados");
                                Console.WriteLine("|------------------------------------------|");
                                foreach (string item in choices)
                                    Console.WriteLine(item);
                                Console.WriteLine("|------------------------------------------|");
                                Console.WriteLine("\n\n");

                            }

                            Console.WriteLine("Seleccione una opción. Si ha terminado, escriba \"" + finish + "\"");
                            string input = Console.ReadLine().ToLower();
                            if (input == finish.ToLower())
                                choice = finish;
                            else
                                choice = Program.CheckInput(StageGroup.GroupList, input);
                            if (choice != null)
                            {
                                if (choice != finish && choices.Contains(choice) == false)
                                    choices.Add(choice);
                            }
                            Console.Clear();
                        }
                        choice = stage;
                        foreach (string element in choices) //stop here
                        {
                            //Check if element is a class

                            List<string> elementWords = element.Split('_').ToList();
                            elementWords.Remove(elementWords.FirstOrDefault());
                            string elementClassName = string.Join("_", elementWords);

                            RDFOntologyClass elementClass = Program.Game.GameOntology.Model.ClassModel.SelectClass(Program.Game.CurrentGameContext + elementClassName);
                            if(elementClass != null)
                            {
                                float? previousLimitValue = LimitValue;

                                new SelectViewType(Program.CreationScheme, elementClassName, Program.Counter);
                                ++Program.Counter;

                                Program.Game.GetLimit(elementClassName, out float? currentLimitValue);
                                if(previousLimitValue > currentLimitValue)
                                {
                                    string elementPredicate = Program.Game.GetObjectPropertyAssociated(stage);
                                    Program.Game.AddObjectProperty(Program.Game.CurrentCharacterContext + Program.Game.CurrentCharacterName, Program.Game.CurrentCharacterContext + elementPredicate, Program.Game.CurrentCharacterContext + element);
                                    Program.Game.AddClassification(elementPredicate);
                                }
                            }
                            else
                            {
                                string predicate = Program.Game.GetObjectPropertyAssociated(stage);
                                Program.Game.AddObjectProperty(Program.Game.CurrentCharacterContext + Program.Game.CurrentCharacterName, Program.Game.CurrentCharacterContext + predicate, Program.Game.CurrentCharacterContext + element);
                                Program.Game.AddClassification(predicate);
                            }

                            List<string> CostWords = new List<string> { "Coste", "Cost", "Coût" };
                            Dictionary<string, float> ElementCosts = new Dictionary<string, float>();
                            RDFOntologyFact ElementFact = Program.Game.GameOntology.Data.SelectFact(Program.Game.CurrentGameContext + element);
                            RDFOntologyTaxonomy ElementAssertions = Program.Game.GameOntology.Data.Relations.Assertions.SelectEntriesBySubject(ElementFact);
                            IEnumerable<RDFOntologyTaxonomyEntry> ElementCostAssertions = ElementAssertions.Where(entry => CostWords.Any(cost => entry.TaxonomyPredicate.ToString().Contains(cost)));
                            foreach(RDFOntologyTaxonomyEntry assertion in ElementCostAssertions)
                            {
                                string costPredicate = assertion.TaxonomyPredicate.ToString().Substring(assertion.TaxonomyPredicate.ToString().LastIndexOf('#')+1);
                                float costValue = Convert.ToSingle(assertion.TaxonomyObject.ToString().Substring(0, assertion.TaxonomyObject.ToString().IndexOf('^')));
                                ElementCosts.Add(costPredicate, costValue);
                            }

                            foreach(KeyValuePair<string, float> elementCost in ElementCosts)
                            {
                                string GeneralCostPredicate = Program.Game.GetGeneralCost(stage, element, out float GeneralCost);
                                if(GeneralCostPredicate != null) // Testing if this serves to check when to edit limit value
                                {
                                    if (GeneralCostPredicate != elementCost.Key)
                                    {
                                        List<string> predicateWords = elementCost.Key.Split('_').ToList();
                                        predicateWords.RemoveAt(0);

                                        RDFOntologyFact CharacterFact = Program.Game.CharacterOntology.Data.SelectFact(Program.Game.CurrentCharacterContext + Program.Game.CurrentCharacterName);
                                        RDFOntologyTaxonomy CharacterAssertions = Program.Game.CharacterOntology.Data.Relations.Assertions.SelectEntriesBySubject(CharacterFact);
                                        foreach (RDFOntologyTaxonomyEntry entry in CharacterAssertions)
                                        {
                                            string entryPredicate = entry.TaxonomyPredicate.ToString().Substring(entry.TaxonomyPredicate.ToString().LastIndexOf('#') + 1);
                                            if (Program.Game.CheckDatatypeProperty(entryPredicate, false) == true)
                                            {
                                                RDFOntologyDatatypeProperty DatatypeProperty = entry.TaxonomyPredicate as RDFOntologyDatatypeProperty;
                                                if (DatatypeProperty.Domain != null)
                                                {
                                                    List<string> CharacterClass = new List<string> { "Personaje", "Character", "Personnage" };
                                                    if (CharacterClass.Any(word => DatatypeProperty.Domain.ToString().Contains(word)))
                                                    {
                                                        string firstWord = DatatypeProperty.ToString().Replace(Program.Game.CurrentCharacterContext, "").Split('_').ToList().FirstOrDefault();
                                                        predicateWords.Insert(0, firstWord);
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        if (CostWords.Any(word => predicateWords.LastOrDefault() == word))
                                        {
                                            predicateWords.RemoveAt(predicateWords.Count() - 1);
                                            predicateWords.Add("Total");
                                        }

                                        string ElementRequirementCost = "";
                                        foreach (string word in predicateWords)
                                        {
                                            ElementRequirementCost += word;
                                            if (word != predicateWords.LastOrDefault())
                                                ElementRequirementCost += '_';
                                        }

                                        RDFOntologyDatatypeProperty RequirementProperty = Program.Game.CharacterOntology.Model.PropertyModel.SelectProperty(Program.Game.CurrentCharacterContext + ElementRequirementCost) as RDFOntologyDatatypeProperty;
                                        RDFOntologyTaxonomy RequirementPropertyAssertions = CharacterAssertions.SelectEntriesByPredicate(RequirementProperty);
                                        if (RequirementPropertyAssertions.Count() > 0)
                                        {
                                            float updateValue = Convert.ToSingle(RequirementPropertyAssertions.SingleOrDefault().TaxonomyObject.ToString().Substring(0, RequirementPropertyAssertions.SingleOrDefault().TaxonomyObject.ToString().IndexOf('^')));
                                            if (elementCost.Value < 0)
                                                updateValue += Math.Abs(elementCost.Value);
                                            else
                                                updateValue -= elementCost.Value;
                                            Program.Game.UpdateDatatypeAssertion(ElementRequirementCost, updateValue.ToString());
                                        }
                                    }
                                    else
                                    {
                                        Program.Game.UpdateAvailablePoints(stage, AvailablePoints -= GeneralCost);
                                        if (LimitValue != null)
                                        {
                                            Program.Game.UpdateLimit(stage, LimitValue -= GeneralCost);
                                            ReturnLimitValue = LimitValue;
                                        }
                                    }
                                }
                                else
                                {
                                    Program.Game.UpdateAvailablePoints(stage, AvailablePoints -= elementCost.Value);
                                    if (LimitValue != null)
                                    {
                                        Program.Game.UpdateLimit(stage, LimitValue -= elementCost.Value);
                                        ReturnLimitValue = LimitValue;
                                    }
                                }
                            }
                                
                        }
                        Program.Game.SaveCharacter();
                        Console.Clear();
                    }
                    else
                    {
                        Program.ShowStage(stage, Program.Counter);
                        Console.WriteLine("No hay elementos disponibles en esta sección");
                        Console.WriteLine("Pulse Intro para acceder a la siguiente sección.");
                        Console.ReadLine();
                        Console.Clear();
                    }
                }
                else
                {
                    Program.ShowStage(stage, Program.Counter);
                    Console.WriteLine("Ya ha agotado todos los puntos disponibles para esta sección");
                    Console.WriteLine("Pulse Intro para acceder a la siguiente sección.");
                    Console.ReadLine();
                    Console.Clear();
                }
            }
            else
            {
                SortedList<int, string> OrderedSubstages = Program.Game.GetOrderedSubstages(stage);
                foreach(string substage in OrderedSubstages.Values)
                {
                    bool substageHasViewType = false;
                    IEnumerable<RDFOntologyTaxonomyEntry> SubstageAnnotations = Program.Game.GameOntology.Data.Annotations.CustomAnnotations.Where(item => item.TaxonomySubject.ToString().Contains(substage));
                    IEnumerable <RDFOntologyTaxonomyEntry> substageViewTypeAnnotations =  SubstageAnnotations.Where(item => item.TaxonomyPredicate.ToString().Contains("ViewType"));
                    substageHasViewType = substageViewTypeAnnotations.Count() > 0;

                    string substageViewType = null;
                    RDFOntologyTaxonomyEntry substageViewTypeEntry;
                    if (substageViewTypeAnnotations.Count() != 1)
                        substageViewTypeEntry = null;
                    else
                        substageViewTypeEntry = substageViewTypeAnnotations.SingleOrDefault();

                    if (substageViewTypeEntry != null)
                    {
                        string substageViewTypeUri = substageViewTypeAnnotations.SingleOrDefault().TaxonomyObject.ToString();
                        if(substageViewTypeUri.Contains('^'))
                            substageViewType = substageViewTypeUri.Substring(0, substageViewTypeAnnotations.SingleOrDefault().TaxonomyObject.ToString().IndexOf('^'));                       
                    }
                    else
                        substageViewType = null;
                    
                    switch (substageViewType)
                    {
                        case "SingleChoiceListView": new SingleChoiceViewModel(substage, out _); break;
                        case "MultipleChoiceStaticLimitView": new MultipleChoiceStaticLimitViewModel(substage, LimitValue, out ReturnLimitValue); break;
                        case "ValuedListView": new ValuedListViewModel(substage, LimitValue, out ReturnLimitValue); break;
                        default: new MultipleChoiceDynamicLimitViewModel(substage, LimitValue, out ReturnLimitValue); break;
                    }
                    LimitValue = ReturnLimitValue;
                }
                if(LimitValue != null)
                    Program.Game.UpdateLimit(stage, LimitValue);

            }
        }

        internal void ShowAvailableOptions(string listName, List<string> AvailableOptions)
        {
            Console.WriteLine("====[ " + listName + " ]");
            foreach(string item in AvailableOptions)
            {
                string formattedItem = item.Replace('_', ' ');
                Console.WriteLine("----|====> " + formattedItem);
            }
        }
    }
}
