using ARPEGOS_Test.Models;
using RDFSharp.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Arpegos_Test.ViewModels
{
    public class ValuedListViewModel
    {
        /// <summary>
        /// Subroutine that simulates a Xamarin View List for Valued List
        /// </summary>
        /// <param name="stage">Etapa del proceso de creación que tiene que mostrar</param>
        public ValuedListViewModel(string stage, float? LimitValue, out float? ReturnLimitValue)
        {
            ReturnLimitValue = null;
            //Comprobar Personaje_Límite_PD
            string AvailablePointsLimit = Program.Game.GetAvailablePoints(stage, out float? AvailablePoints);
            bool hasAvailablePointsLimit = AvailablePointsLimit != null;

            if ((hasAvailablePointsLimit == false) || (AvailablePoints != null && AvailablePoints > 0))
            {
                ReturnLimitValue = LimitValue;
                //comprobar si stage es una clase en el juego
                if (Program.Game.CheckClass(stage, false))
                {
                    //Diccionario para mostrar resultados - Borrar en ARPEGOS
                    Dictionary<string, Dictionary<string, string>> ListElements = new Dictionary<string, Dictionary<string, string>>();
                    Group StageGroup = new Group(stage);
                    string grouptype = StageGroup.GroupList.GetType().ToString();

                    List<string> stageTitleWords = StageGroup.Title.Split('_').ToList();
                    string stagetype = "";
                    if (stageTitleWords.Count() > 2)
                    {
                        for (int i = 0; i < stageTitleWords.Count() - 2; ++i)
                            stagetype += stageTitleWords.ElementAtOrDefault(i) + "_";
                        stagetype = stagetype[0..^1].Trim();
                    }
                    else
                        stagetype = stageTitleWords.ElementAtOrDefault(0);

                    //Find ValuedStageInfo CheckValueListInfo(string stage)
                    string Info = Program.Game.CheckValueListInfo(StageGroup.Title);
                    List<string> InfoRows = Info.Split('\n').ToList();
                    //Hasta aqui

                    if (grouptype.Contains("Item"))
                    {
                        List<List<string>> StageValuedProperties = new List<List<string>>();

                        foreach (string row in InfoRows)
                            StageValuedProperties.Add(row.Split(',').ToList());
                        string StageScheme = "\t\t\t";
                        foreach (List<string> row in StageValuedProperties)
                            StageScheme = row.ElementAtOrDefault(0).Replace("Per_Item", stagetype).Replace('_', ' ') + "\t\t";

                        foreach (Item item in StageGroup.GroupList)
                        {
                            Program.ShowStage(stage, Program.Counter);
                            string itemClass = Program.Game.GetClass(item.Name);
                            bool hasLimit;
                            string LimitName = null;
                            LimitName = Program.Game.GetLimit(itemClass, out LimitValue);
                            hasLimit = LimitName != null;                                

                            Dictionary<string, string> ItemProperties = new Dictionary<string, string>();
                            bool ValueAddedCorrectly = false;
                            float InputValue = 0;
                            string User_Input = null;
                            bool NoPointsLeft = false;

                            RDFOntologyFact itemFact = Program.Game.GameOntology.Data.SelectFact(Program.Game.CurrentGameContext + item.Name);
                            RDFOntologyTaxonomy itemAssertions = Program.Game.GameOntology.Data.Relations.Assertions.SelectEntriesBySubject(itemFact);
                          
                            while (ValueAddedCorrectly == false)
                            {
                                if (hasLimit && LimitValue > 0)
                                {
                                    Console.WriteLine("Introduzca el valor de " + StageValuedProperties.ElementAtOrDefault(0).ElementAtOrDefault(0).Replace("Per_Item", stagetype).Replace('_', ' ') + " para el elemento " + item.FormattedName + ". Límite (" + LimitValue + ")");
                                    User_Input = Console.ReadLine();
                                    Console.WriteLine();
                                    if (User_Input == "")
                                    {
                                        Console.Clear();
                                        Program.ShowStage(stage, Program.Counter);
                                        Console.WriteLine("|<-ERROR->| - No se ha introducido ningún valor. \n");
                                        continue;
                                    }

                                    InputValue = Convert.ToSingle(User_Input);
                                    if (hasAvailablePointsLimit == true && InputValue > AvailablePoints)
                                    {
                                        ValueAddedCorrectly = false;
                                        Console.Clear();
                                        Program.ShowStage(stage, Program.Counter);
                                        Console.WriteLine("El valor que ha introducido supera el límite de puntos total que puede gastar: " + AvailablePoints);
                                        continue;
                                    }
                                    if (InputValue > LimitValue)
                                    {
                                        ValueAddedCorrectly = false;
                                        Console.Clear();
                                        Program.ShowStage(stage, Program.Counter);
                                        Console.WriteLine("|<-ERROR->| - El valor que ha introducido supera el límite estipulado. \n");
                                        continue;
                                    }

                                }
                                else if (LimitValue == 0 || AvailablePoints == 0)
                                {
                                    User_Input = "0";
                                    NoPointsLeft = true;
                                }
                                else
                                {
                                    Console.WriteLine("Introduzca el valor de " + StageValuedProperties.ElementAtOrDefault(0).ElementAtOrDefault(0).Replace("Per_Item", stagetype).Replace('_', ' ') + " para el elemento " + item.FormattedName);
                                    User_Input = Console.ReadLine();

                                    while (User_Input == "")
                                    {
                                        Console.Clear();
                                        Program.ShowStage(stage, Program.Counter);
                                        Console.WriteLine("|<-ERROR->| - No se ha introducido ningún valor. \n");
                                        Console.WriteLine("Introduzca el valor de " + StageValuedProperties.ElementAtOrDefault(0).ElementAtOrDefault(0).Replace("Per_Item", stagetype).Replace('_', ' ') + " para el elemento " + item.FormattedName);
                                        User_Input = Console.ReadLine();
                                    }

                                    Console.WriteLine();
                                }

                                if (itemAssertions.Count() != 0 || Program.Game.CheckEquipmentClass(itemClass) == false)
                                {
                                    foreach (List<string> row in StageValuedProperties)
                                    {
                                        string CurrentValue = "0";
                                        string PropertyName = row.ElementAt(0).Replace("Item", item.Name);

                                        string UserEditionField = row.Where(column => column.Contains("user_edit")).SingleOrDefault().Split(':').ElementAtOrDefault(1);
                                        if (UserEditionField.Contains('^'))
                                            UserEditionField = UserEditionField.Substring(0, UserEditionField.IndexOf('^'));

                                        bool user_edition_available = Convert.ToBoolean(UserEditionField);
                                        if (user_edition_available == false)
                                        {
                                            string RowDescription = row.Last();
                                            if (RowDescription.Contains("^"))
                                                RowDescription = RowDescription.Substring(0, RowDescription.IndexOf('^'));
                                            if (RowDescription.Contains("Item"))
                                                RowDescription = RowDescription.Replace("Item", item.Name);

                                            List<string> RowDescriptionElements = new List<string>();
                                            List<string> RowDescriptionProperties = new List<string>();

                                            foreach (string element in RowDescription.Split(':'))
                                                RowDescriptionElements.Add(element.Trim());

                                            List<string> operators = new string[] { "+", "-", "*", "/", "%", "<", ">", "<=", ">=", "=", "!=" }.ToList();

                                            //aislar elementos que puedan pertenecer a ItemProperties
                                            List<string> UpdatedRowDescriptionElements = new List<string>();
                                            foreach (string element in RowDescriptionElements)
                                            {
                                                UpdatedRowDescriptionElements.Add(element);
                                                string elementTrim = element.Trim();
                                                if (ItemProperties.ContainsKey(elementTrim))
                                                    RowDescriptionProperties.Add(elementTrim);
                                                else if (elementTrim.Contains("Ref"))
                                                    RowDescriptionProperties.Add(elementTrim);
                                                else if (Program.Game.CheckDatatypeProperty(elementTrim, false) == true)
                                                    RowDescriptionProperties.Add(elementTrim);
                                                else
                                                {
                                                    List<string> elementWords = elementTrim.Split('_').ToList();
                                                    int wordCounter = elementWords.Count();
                                                    bool elementFound = false;
                                                    elementTrim = "";

                                                    for (int i = 0; i < wordCounter - 2; ++i)
                                                        elementTrim += elementWords.ElementAtOrDefault(i) + "_";
                                                    elementTrim += elementWords.LastOrDefault();

                                                    if (ItemProperties.ContainsKey(elementTrim))
                                                    {
                                                        int elementIndex = UpdatedRowDescriptionElements.IndexOf(element);
                                                        UpdatedRowDescriptionElements.Remove(element);
                                                        UpdatedRowDescriptionElements.Insert(elementIndex, elementTrim);
                                                        RowDescriptionProperties.Add(elementTrim);
                                                        elementFound = true;
                                                    }

                                                    else if (Program.Game.CheckDatatypeProperty(elementTrim, false) == true)
                                                    {
                                                        int elementIndex = UpdatedRowDescriptionElements.IndexOf(element);
                                                        UpdatedRowDescriptionElements.Remove(element);
                                                        UpdatedRowDescriptionElements.Insert(elementIndex, elementTrim);
                                                        RowDescriptionProperties.Add(elementTrim);
                                                        elementFound = true;
                                                    }
                                                    --wordCounter;

                                                    while (elementFound == false && wordCounter > 1)
                                                    {
                                                        elementTrim = "";
                                                        for (int i = 0; i < wordCounter - 1; ++i)
                                                            elementTrim += elementWords.ElementAtOrDefault(i) + "_";
                                                        elementTrim += elementWords.LastOrDefault();

                                                        if (ItemProperties.ContainsKey(elementTrim))
                                                        {
                                                            int elementIndex = UpdatedRowDescriptionElements.IndexOf(element);
                                                            UpdatedRowDescriptionElements.Remove(element);
                                                            UpdatedRowDescriptionElements.Insert(elementIndex, elementTrim);
                                                            RowDescriptionProperties.Add(elementTrim);
                                                            elementFound = true;
                                                        }

                                                        else if (Program.Game.CheckDatatypeProperty(elementTrim, false) == true)
                                                        {
                                                            int elementIndex = UpdatedRowDescriptionElements.IndexOf(element);
                                                            UpdatedRowDescriptionElements.Remove(element);
                                                            UpdatedRowDescriptionElements.Insert(elementIndex, elementTrim);
                                                            RowDescriptionProperties.Add(elementTrim);
                                                            elementFound = true;
                                                        }
                                                        --wordCounter;
                                                    }
                                                }
                                            }

                                            RowDescriptionElements = UpdatedRowDescriptionElements;

                                            bool AllValuesInDictionary = true;
                                            if (RowDescriptionProperties.Count() > 0)
                                            {
                                                bool anyPropertyFailed = false;
                                                foreach (string property in RowDescriptionProperties)
                                                {
                                                    if (Program.Game.CheckDatatypeProperty(property, false))
                                                    {
                                                        if (anyPropertyFailed == false)
                                                            if (!ItemProperties.ContainsKey(property))
                                                                anyPropertyFailed = true;
                                                    }
                                                    else
                                                        if (property.Contains("Ref"))
                                                        anyPropertyFailed = true;
                                                }
                                                if (anyPropertyFailed == true)
                                                    AllValuesInDictionary = false;
                                            }

                                            if (RowDescriptionProperties.Count() == 0)
                                                AllValuesInDictionary = false;

                                            if (AllValuesInDictionary == true)
                                            {
                                                if (ItemProperties.Count == 1)
                                                {
                                                    if (operators.Any(op => RowDescriptionElements.Any(element => element == op)))
                                                    {
                                                        int operatorIndex = RowDescriptionElements.IndexOf(RowDescriptionElements.Where(element => operators.Any(op => element == op)).FirstOrDefault());
                                                        string nextDescriptionElement = RowDescriptionElements.ElementAtOrDefault(operatorIndex + 1);
                                                        if (Regex.IsMatch(nextDescriptionElement, @"\D"))
                                                        {
                                                            if (RowDescription.EndsWith(':'))
                                                                RowDescription = RowDescription[0..^1];
                                                            CurrentValue = Program.Game.GetValue(RowDescription, item.Name, User_Input).ToString();
                                                        }

                                                        else if (Regex.IsMatch(nextDescriptionElement, @"\d"))
                                                        {
                                                            CurrentValue = ItemProperties.Values.SingleOrDefault();
                                                            string currentOperator = RowDescriptionElements.ElementAtOrDefault(operatorIndex);
                                                            dynamic operatorResult = Game.ConvertToOperator(currentOperator, Convert.ToSingle(CurrentValue), Convert.ToSingle(nextDescriptionElement));

                                                            RDFOntologyDatatypeProperty property = Program.Game.GameOntology.Model.PropertyModel.SelectProperty(Program.Game.CurrentGameContext + PropertyName) as RDFOntologyDatatypeProperty;
                                                            bool isFloat = property.Range.ToString().Contains("float");

                                                            if (isFloat == false)
                                                            {
                                                                string resultString = operatorResult.ToString();
                                                                if (resultString.Contains(','))
                                                                    CurrentValue = resultString.Split(',').ElementAtOrDefault(0);
                                                                else
                                                                    CurrentValue = resultString;
                                                            }
                                                            else
                                                                CurrentValue = operatorResult.ToString();
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    RowDescriptionProperties = RowDescriptionElements;
                                                    int index = 0;

                                                    foreach (string element in RowDescriptionProperties)
                                                    {
                                                        if (element == RowDescriptionProperties.FirstOrDefault())
                                                            ItemProperties.TryGetValue(element, out CurrentValue);
                                                        else
                                                        {
                                                            if (operators.Any(op => element == op))
                                                            {
                                                                string NextValue = null;
                                                                int nextIndex = index + 1;
                                                                string nextElement = RowDescriptionProperties.ElementAtOrDefault(nextIndex);
                                                                if (Regex.IsMatch(nextElement, @"\d") && !Regex.IsMatch(nextElement, @"\D"))
                                                                    NextValue = nextElement;
                                                                else
                                                                {
                                                                    while (!ItemProperties.ContainsKey(nextElement) && nextIndex < RowDescriptionProperties.Count())
                                                                    {
                                                                        ++nextIndex;
                                                                        nextElement = RowDescriptionProperties.ElementAtOrDefault(nextIndex);
                                                                    }
                                                                    ItemProperties.TryGetValue(RowDescriptionProperties.ElementAtOrDefault(nextIndex), out NextValue);
                                                                }

                                                                if (element == "/" && NextValue == "0")
                                                                    NextValue = "1";
                                                                dynamic operatorResult = Game.ConvertToOperator(element, Convert.ToSingle(CurrentValue), Convert.ToSingle(NextValue));

                                                                RDFOntologyDatatypeProperty property = Program.Game.GameOntology.Model.PropertyModel.SelectProperty(Program.Game.CurrentGameContext + PropertyName) as RDFOntologyDatatypeProperty;
                                                                bool isFloat = property.Range.ToString().Contains("float");

                                                                if (isFloat == false)
                                                                {
                                                                    string resultString = operatorResult.ToString();
                                                                    if (resultString.Contains(','))
                                                                        CurrentValue = resultString.Split(',').ElementAtOrDefault(0);
                                                                    else
                                                                        CurrentValue = resultString;
                                                                }
                                                                else
                                                                    CurrentValue = operatorResult.ToString();
                                                            }
                                                        }
                                                        ++index;
                                                    }
                                                }


                                            }
                                            else
                                                CurrentValue = Program.Game.GetValue(RowDescription, item.Name, User_Input).ToString();
                                        }
                                        else
                                        {
                                            CurrentValue = User_Input;
                                            RDFOntologyDatatypeProperty predicate = Program.Game.GameOntology.Model.PropertyModel.SelectProperty(Program.Game.CurrentGameContext + PropertyName) as RDFOntologyDatatypeProperty;
                                            string valueType;
                                            if (int.TryParse(User_Input, out int currentInt))
                                                valueType = "integer";
                                            else
                                                valueType = "float";

                                            if (!Program.Game.CheckDatatypeProperty(PropertyName))
                                                predicate = Program.Game.CreateDatatypeProperty(PropertyName);
                                            else
                                                predicate = Program.Game.CharacterOntology.Model.PropertyModel.SelectProperty(Program.Game.CurrentCharacterContext + PropertyName) as RDFOntologyDatatypeProperty;

                                            RDFOntologyTaxonomy CharacterCurrentPredicate = Program.Game.CharacterOntology.Data.Relations.Assertions.SelectEntriesByPredicate(predicate);
                                            if (CharacterCurrentPredicate.Count() > 0)
                                                Program.Game.UpdateDatatypeAssertion(PropertyName, User_Input);
                                            else
                                                Program.Game.AddDatatypeProperty(Program.Game.CurrentCharacterContext + Program.Game.CurrentCharacterName, Program.Game.CurrentCharacterContext + PropertyName, User_Input, valueType);
                                        }

                                        PropertyName = PropertyName.Replace("Item", item.Name);
                                        if (ItemProperties.ContainsKey(PropertyName))
                                            ItemProperties.Remove(PropertyName);
                                        ItemProperties.Add(PropertyName, CurrentValue);
                                    }

                                    if (NoPointsLeft == false)
                                    {
                                        Console.Write("\t");
                                        for (int i = 0; i < item.FormattedName.Length; ++i)
                                            Console.Write(" ");
                                        Console.Write("\t");

                                        foreach (string property in ItemProperties.Keys)
                                        {
                                            Console.Write(property + "\t");
                                        }
                                        Console.WriteLine("\n");
                                        Console.Write("\t" + item.FormattedName + "\t");

                                        foreach (string property in ItemProperties.Keys)
                                        {
                                            ItemProperties.TryGetValue(property, out string propertyValue);
                                            for (int i = 0; i < property.Length / 2; ++i)
                                                Console.Write(" ");
                                            Console.Write(propertyValue);
                                            for (int i = 0; i < property.Length / 2; ++i)
                                                Console.Write(" ");
                                            Console.Write("\t");
                                        }

                                        Console.WriteLine("\n");

                                        {
                                            Console.WriteLine("Indique si los datos son correctos (S/N)");
                                            ConsoleKeyInfo confirmCorrectData = Console.ReadKey();
                                            ValueAddedCorrectly = ((confirmCorrectData.KeyChar == 's') || (confirmCorrectData.KeyChar == 'S'));
                                            if (ListElements.ContainsKey(item.FormattedName))
                                                ListElements.Remove(item.FormattedName);
                                            ListElements.Add(item.FormattedName, ItemProperties);
                                            if (ValueAddedCorrectly == true)
                                            {
                                                ReturnLimitValue = LimitValue - InputValue;
                                                if (hasAvailablePointsLimit == true)
                                                {
                                                    if(Program.Game.CheckEquipmentClass(itemClass)== true)
                                                    {
                                                        List<string> CostWords = new List<string> { "Coste", "Cost", "Coût" };
                                                        string ItemTotalCostProperty = ItemProperties.Keys.Where(property => CostWords.Any(word => property.Contains(word)) && property.Contains("Total")).SingleOrDefault();
                                                        ItemProperties.TryGetValue(ItemTotalCostProperty, out string totalCostValue);
                                                        Program.Game.UpdateAvailablePoints(stage, AvailablePoints -= Convert.ToSingle(totalCostValue));
                                                    }
                                                    else
                                                        Program.Game.UpdateAvailablePoints(stage, AvailablePoints -= Convert.ToSingle(User_Input));
                                                    if (hasLimit == true)
                                                        Program.Game.UpdateLimit(stage, ReturnLimitValue);
                                                }
                                            }
                                            else
                                                ItemProperties.Clear();
                                        }
                                    }
                                    else
                                    {
                                        if (ListElements.ContainsKey(item.FormattedName))
                                            ListElements.Remove(item.FormattedName);
                                        ListElements.Add(item.FormattedName, ItemProperties);
                                        ValueAddedCorrectly = true;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Indique si los datos son correctos (S/N)");
                                    ConsoleKeyInfo confirmCorrectData = Console.ReadKey();
                                    ValueAddedCorrectly = ((confirmCorrectData.KeyChar == 's') || (confirmCorrectData.KeyChar == 'S'));
                                }
                                Console.Clear();
                            }

                            List<string> EquipmentWords = new List<string> { "Equipamiento", "Equipment", "Équipement" };
                            if (EquipmentWords.Any(word => stage.Contains(word)))
                            {
                                if(Convert.ToSingle(User_Input) > 0)
                                {
                                    if (!Program.Game.CheckIndividual(item.Name))
                                        Program.Game.CreateIndividual(item.Name);

                                    string predicate = Program.Game.GetObjectPropertyAssociated(stage);
                                    if (!Program.Game.CheckObjectProperty(predicate))
                                        Program.Game.CreateObjectProperty(predicate);

                                    Program.Game.AddObjectProperty(Program.Game.CurrentCharacterContext + Program.Game.CurrentCharacterName, Program.Game.CurrentCharacterContext + predicate, Program.Game.CurrentCharacterContext + item.Name);

                                    List<string> InventoryWords = new List<string> { "Inventario", "Inventory", "Inventaire" };
                                    RDFOntologyDatatypeProperty EquipmentInventoryProperty = Program.Game.GameOntology.Model.PropertyModel.Where(entry => InventoryWords.Any(word => entry.Range.ToString().Contains(stage))).SingleOrDefault() as RDFOntologyDatatypeProperty;
                                    string EquipmentInventoryPropertyName = EquipmentInventoryProperty.ToString().Substring(EquipmentInventoryProperty.ToString().LastIndexOf('#') + 1);
                                    string valuetype = EquipmentInventoryProperty.Range.ToString().Substring(EquipmentInventoryProperty.Range.ToString().LastIndexOf('#') + 1);
                                    if (!Program.Game.CheckDatatypeProperty(EquipmentInventoryPropertyName))
                                        Program.Game.CreateDatatypeProperty(EquipmentInventoryPropertyName);

                                    Program.Game.AddDatatypeProperty(Program.Game.CurrentCharacterContext + item.Name, Program.Game.CurrentCharacterContext + EquipmentInventoryPropertyName, ItemProperties.Values.FirstOrDefault(), valuetype);

                                    if (itemAssertions.Count() > 0)
                                    {
                                        List<string> LimitProperties = new List<string>();
                                        foreach (string property in ItemProperties.Keys)
                                        {
                                            if (property.Contains("Total"))
                                                LimitProperties.Add(property);
                                        }

                                        List<string> GeneralLimitWords = Program.Game.GetAvailablePoints(stage, out _).Split('_').ToList();
                                        string GeneralLimitProperty = LimitProperties.Where(property => LimitProperties.Any(word => property.Contains(word))).SingleOrDefault();
                                        ItemProperties.TryGetValue(GeneralLimitProperty, out string propertyValue);

                                        AvailablePoints -= Convert.ToSingle(ItemProperties.Values.FirstOrDefault()) * Convert.ToSingle(propertyValue);

                                        List<string> PartialLimitWords = Program.Game.GetLimit(stage, out _).Split('_').ToList();
                                        string PartialLimitProperty = LimitProperties.Where(property => LimitProperties.Any(word => property.Contains(word))).SingleOrDefault();
                                        ItemProperties.TryGetValue(GeneralLimitProperty, out propertyValue);

                                        LimitValue -= Convert.ToSingle(ItemProperties.Values.FirstOrDefault()) * Convert.ToSingle(propertyValue);
                                    }
                                }
                                                
                            }
                            else
                            {

                                foreach (KeyValuePair<string, string> property in ItemProperties)
                                {
                                    if (ItemProperties.ToList().IndexOf(property) != 0)
                                    {
                                        RDFOntologyDatatypeProperty predicate = Program.Game.GameOntology.Model.PropertyModel.SelectProperty(Program.Game.CurrentGameContext + property.Key) as RDFOntologyDatatypeProperty;
                                        string valueType = predicate.Range.ToString().Substring(predicate.Range.ToString().LastIndexOf('#') + 1);
                                        Program.Game.AddDatatypeProperty(Program.Game.CurrentCharacterContext + Program.Game.CurrentCharacterName, Program.Game.CurrentCharacterContext + property.Key, property.Value, valueType);
                                    }
                                }
                                Program.Game.AddClassification(ItemProperties.Keys.LastOrDefault());

                                //Comprobar cuándo depende de la entrada del usuario
                                LimitValue -= Convert.ToSingle(User_Input);
                            }

                            Console.WriteLine();                            

                        }
                    }
                    else
                    {
                        IEnumerable<Group> GroupCollection = StageGroup.GroupList;
                        List<Group> GroupList = new List<Group>(GroupCollection);
                        Group FirstStage = GroupList.FirstOrDefault();
                        string FirstStageTitle = FirstStage.Title;
                        float? SharedLimit = null;

                        bool limitChanges = true;
                        foreach (Group group in StageGroup.GroupList)
                        {
                            if (group.Title == FirstStageTitle)
                                new ValuedListViewModel(group.Title, LimitValue, out SharedLimit);
                            else
                            {
                                new ValuedListViewModel(group.Title, SharedLimit, out float? NewSharedLimit);
                                if (SharedLimit != NewSharedLimit)
                                {
                                    limitChanges = true;
                                    SharedLimit = NewSharedLimit;
                                }
                                else
                                    limitChanges = false;
                            }
                        }
                        if(limitChanges)
                            Program.Game.UpdateLimit(stage, SharedLimit);
                        Console.Clear();
                    }

                    //No copiar en ARPEGOS
                    if (grouptype.Contains("Item"))
                    {
                        Console.WriteLine("Resultado final de " + stage.Replace("_", " ") + ":\n");
                        Console.Write("\t\t\t");
                        foreach (string row in InfoRows)
                        {
                            string column_name = row.Substring(0, row.IndexOf(','));
                            Console.Write(column_name + "\t\t");
                        }
                        Console.WriteLine();

                        foreach (var item in ListElements)
                        {
                            bool firstTime = true;
                            Console.Write("\t" + item.Key + "\t\t");

                            foreach (var property in item.Value)
                            {
                                if (item.Key.Length < 7 && firstTime == true)
                                {
                                    firstTime = false;
                                    Console.Write("\t");
                                }
                                Console.Write(property.Value);
                                Console.Write("\t\t\t");
                            }
                            Console.WriteLine();
                        }
                        Console.WriteLine("\n\nPulse Intro para continuar");
                        Console.ReadLine();
                        Console.Clear();
                    }
                    //Hasta aquí
                }
                else
                {
                    // Procesar propiedad
                    RDFOntologyDatatypeProperty predicate;
                    if (!Program.Game.CheckDatatypeProperty(stage))
                        predicate = Program.Game.CreateDatatypeProperty(stage);
                    else
                        predicate = (RDFOntologyDatatypeProperty)Program.Game.CharacterOntology.Model.PropertyModel.SelectProperty(Program.Game.CurrentCharacterContext + stage);

                    string LimitName = Program.Game.GetLimit(stage, out float? InputLimit);
                    bool hasLimit = LimitName != null;

                    List<string> stageWords = stage.Split('_').ToList();
                    bool ValueAddedCorrectly = false;
                    string User_Input = string.Empty;
                    float InputValue = 0;
                    while (ValueAddedCorrectly == false)
                    {
                        if (hasLimit)
                        {
                            if (InputValue > InputLimit)
                                Console.WriteLine("|<-ERROR->| - El valor que ha introducido supera el límite estipulado. \n");
                            Console.WriteLine("Introduzca el " + stageWords.Last() + " del personaje. Límite(" + InputLimit + ") : ");
                            User_Input = Console.ReadLine();
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.WriteLine("Introduzca el " + stageWords.Last() + " del personaje : ");
                            User_Input = Console.ReadLine();
                            Console.WriteLine();
                        }

                        if (User_Input == "")
                        {
                            Console.Clear();
                            Program.ShowStage(stage, Program.Counter);
                            Console.WriteLine("|<-ERROR->| - No se ha introducido ningún valor. \n");
                            continue;
                        }

                        Console.WriteLine("Compruebe si el valor indicado es correcto: " + stageWords.Last() + " => " + User_Input + "(S/N)");
                        ConsoleKeyInfo confirmCorrectData = Console.ReadKey();
                        InputValue = Convert.ToSingle(User_Input);
                        ValueAddedCorrectly = (confirmCorrectData.KeyChar == 's' || confirmCorrectData.KeyChar == 'S');
                        Console.Clear();

                        if (hasLimit)
                            if (InputValue > InputLimit)
                                ValueAddedCorrectly = false;

                        if (ValueAddedCorrectly == false)
                            Program.ShowStage(stage, Program.Counter);

                    }

                    string valuetype = predicate.Range.ToString().Substring(predicate.Range.ToString().LastIndexOf('#') + 1);
                    Program.Game.AddDatatypeProperty(Program.Game.CurrentCharacterContext + Program.Game.CurrentCharacterName, Program.Game.CurrentCharacterContext + stage, User_Input, valuetype);
                    Program.Game.AddClassification(stage);
                    Console.Clear();
                }
            }
            else
            {
                Program.ShowStage(stage, Program.Counter);
                Console.WriteLine("Ya has agotado todos los puntos disponibles para esta sección");
                Console.WriteLine("Pulse Intro para acceder a la siguiente sección.");
                Console.ReadLine();
                Console.Clear();
            }            
        }
    }
}
