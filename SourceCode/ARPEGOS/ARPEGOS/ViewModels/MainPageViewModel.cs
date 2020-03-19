namespace ARPEGOS.ViewModels
{
    using ARPEGOS.Models;
    using ARPEGOS.Views;
    using RDFSharp.Model;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Xamarin.Forms;

    public class MainPageViewModel
    {
        #region Constructor
        public MainPageViewModel() 
        {
            
        }
        #endregion

        #region Methods
        public Page PresentDetailPage(PageType pagetype)
        {
            Page NextPage;
            switch (pagetype)
            {
                case PageType.Welcome: NextPage = new WelcomePage(); break;
                case PageType.Home: NextPage = new HomePage(); break;
                case PageType.GamesList: NextPage = new GameListPage(); break;
                case PageType.CreateCharacter: NextPage = CreateCharacter(); break;
                case PageType.ViewCharacter: NextPage = ViewCharacter(); break;
                case PageType.EditCharacter: NextPage = EditCharacter(); break;
                case PageType.RemoveCharacter: NextPage = RemoveCharacter(); break;
                case PageType.OneSkillCalculator: NextPage = OneSkillCalculator(); break;
                case PageType.TwoSkillCalculator: NextPage = TwoSkillCalculator(); break;
                default: NextPage = new HomePage(); break;
            }
            return new NavigationPage(NextPage);
        }

        private Page CreateCharacter()
        {
            SystemControl.ActiveGame.CharacterFile = Task.Run(async()=> 
                await Xamarin.Forms.Application.Current.MainPage.DisplayPromptAsync("Crear personaje", 
                "Introduzca el nombre del personaje", "Este mismo", "Mejor no", "Escriba aquí")).ToString();
            var rootSchemeElement = SystemControl.ActiveGame.GetSchemeRootElement();
            return new IndividualListView(rootSchemeElement);
        }

        private Page ViewCharacter()
        {
            throw new NotImplementedException();
        }

        private Page EditCharacter()
        {
            throw new NotImplementedException();
        }

        private Page RemoveCharacter()
        {
            throw new NotImplementedException();
        }

        private Page OneSkillCalculator()
        {
            throw new NotImplementedException();
        }

        private Page TwoSkillCalculator()
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
