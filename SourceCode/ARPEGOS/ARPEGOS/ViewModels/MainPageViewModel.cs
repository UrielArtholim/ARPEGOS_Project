namespace ARPEGOS.ViewModels
{
    using ARPEGOS.Models;
    using ARPEGOS.Views;
    using Xamarin.Forms;

    public class MainPageViewModel
    {
        public MainPageViewModel() 
        {
            
        }

        public Page PresentDetailPage(PageType pagetype)
        {
            Page NextPage;
            switch (pagetype)
            {
                case PageType.Welcome: NextPage = new WelcomePage(); break;
                case PageType.Home: NextPage = new HomePage(); break;
                case PageType.GamesList: NextPage = new GameListPage(); break;
                case PageType.CreateCharacter: NextPage = new WelcomePage(); break;
                case PageType.ViewCharacter: NextPage = new WelcomePage(); break;
                case PageType.EditCharacter: NextPage = new WelcomePage(); break;
                case PageType.RemoveCharacter: NextPage = new WelcomePage(); break;
                case PageType.OneSkillCalculator: NextPage = new WelcomePage(); break;
                case PageType.TwoSkillCalculator: NextPage = new WelcomePage(); break;
                default: NextPage = new HomePage(); break;
            }
            return new NavigationPage(NextPage);
        }
    }
}
