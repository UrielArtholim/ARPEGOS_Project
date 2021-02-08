using ARPEGOS;
using ARPEGOS.Models;
using ARPEGOS.Services;
using ARPEGOS.ViewModels;
using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ARPEGOS_Test.Views
{
    public class Console_CreationView : BaseViewModel
    {
        List<Item> Data;
        public Console_CreationView()
        {            
            var stageString = Setup.Game.GetCreationSchemeRootClass();
            var stageName = stageString.Split('#').Last();
            StageViewModel.CurrentStep = 0;
            Setup.ShowStage(stageName , 1);
            if (Setup.CurrentCharacter == null)
                Setup.CurrentCharacter = Setup.Characters.First();
            else
                Setup.CurrentCharacter = Setup.Characters.ElementAt(Setup.Characters.IndexOf(Setup.CurrentCharacter) + 1);
            Data = new List<Item>(Setup.CurrentCharacter.CharacterService.GetIndividuals(stageString));
            Item selectedOption = null;
            bool acceptedChoice = false;
            while(!acceptedChoice)
            {
                Setup.ShowOptions(Data);
                var choice = Console.ReadLine();
                var selectedOptionName = Setup.CheckInput(Data , choice);
                if (Data.Any(option => option.FullName.Contains(selectedOptionName)))
                {
                    acceptedChoice = true;
                    selectedOption = Data.Where(option => option.FullName.Contains(selectedOptionName)).Single();
                }
            }
            StageViewModel.CreationScheme = Setup.CurrentCharacter.CharacterService.GetCreationScheme(selectedOption.FullName);
            var character = Setup.CurrentCharacter.CharacterService;
            var ItemFullShortName = selectedOption.FullName.Split('#').Last();
            var predicateString = character.GetObjectPropertyAssociated(stageString);
            var predicateName = predicateString.Split('#').Last();
            character.UpdateObjectAssertion($"{character.Context}{predicateName}" , $"{character.Context}{ItemFullShortName}");
            bool creationSchemeFailed = false;

            try
            {
                var scheme = character.GetCreationScheme(selectedOption.FullName);
                StageViewModel.CreationScheme = scheme;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error detecteed in stage {stageString} => {e.Message}");
                creationSchemeFailed = true;
            }

            if (creationSchemeFailed == false)
            {
                StageViewModel.CurrentStep = 0;
                var currentStage = StageViewModel.CreationScheme.ElementAt(StageViewModel.CurrentStep);
                try
                {
                    if (currentStage.IsGrouped)
                    {
                        switch (currentStage.Type)
                        {
                            case Stage.StageType.SingleChoice:
                                new Console_SingleChoiceGroupView();
                                break;
                            case Stage.StageType.MultipleChoice:
                                new Console_MultipleChoiceGroupView();
                                break;
                            default:
                                new Console_MultipleChoiceGroupView();
                                break;
                        }
                    }
                    else
                    {
                        switch (currentStage.Type)
                        {
                            case Stage.StageType.SingleChoice:
                                new Console_SingleChoiceView();
                                break;
                            case Stage.StageType.MultipleChoice:
                                new Console_MultipleChoiceView();
                                break;
                            default:
                                new Console_ValuedView();
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error detecteed in stage {stageString} => {e.Message}");
                    --StageViewModel.CurrentStep;
                }
            }
        }
    }
}
