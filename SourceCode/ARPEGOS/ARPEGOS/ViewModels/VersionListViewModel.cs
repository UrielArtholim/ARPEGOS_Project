namespace ARPEGOS.ViewModels
{
    using ARPEGOS.Models;
    using ARPEGOS.Views;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows.Input;
    using Xamarin.Forms;

    class VersionListViewModel
    {
        #region Properties
        public ObservableCollection<SimpleListItem> VersionList { get; private set; }
        public ICommand SelectVersionCommand { get; }
        #endregion

        #region Constructor
        public VersionListViewModel()
        {
            SelectVersionCommand = new Command<SimpleListItem>(item =>
            {
                var selectedItem = this.VersionList.FirstOrDefault(currentVar => currentVar.ItemName == item.ItemName);
                SystemControl.UpdateActiveVersion(selectedItem.ItemName);
                // Load game ontology
                SystemControl.ActiveGame = new Game(SystemControl.GetActiveGame(), SystemControl.GetActiveVersion());
                var game = SystemControl.ActiveGame;
                Debug.WriteLine(SystemControl.ActiveGame);
                MainPage mainpage = Xamarin.Forms.Application.Current.MainPage as MainPage;
                MainPageViewModel mainViewModel = mainpage.BindingContext as MainPageViewModel;
                mainpage.Detail = mainViewModel.PresentDetailPage(PageType.Home);
            });

            VersionList = new ObservableCollection<SimpleListItem>();
            var selectedGame = SystemControl.GetActiveGame();
            var GameFilesPath = Path.Combine(SystemControl.DirectoryHelper.GetBaseDirectory(),selectedGame, "gamefiles");
            DirectoryInfo GamefilesDirectory = new DirectoryInfo(GameFilesPath);
            var GameFiles = GamefilesDirectory.GetFiles();
            foreach (var file in GameFiles)
            {
                VersionList.Add(new SimpleListItem(file.Name));
            }
        }
        #endregion

    }
}
