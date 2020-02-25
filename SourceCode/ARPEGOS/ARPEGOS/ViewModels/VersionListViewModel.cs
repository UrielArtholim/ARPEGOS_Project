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
        public ObservableCollection<ListItem> VersionList { get; private set; }
        public VersionListViewModel()
        {
            SelectVersionCommand = new Command<ListItem>(item =>
            {
                var selectedItem = this.VersionList.FirstOrDefault(currentVar => currentVar.ItemName == item.ItemName);
                SystemControl.UpdateActiveVersion(selectedItem.ItemName);
                // Load game ontology
                SystemControl.ActiveGameDB = new GameDB(SystemControl.GetActiveGame(), SystemControl.GetActiveVersion());
                var game = SystemControl.ActiveGameDB;
                Debug.WriteLine(SystemControl.ActiveGameDB);
                Xamarin.Forms.Application.Current.MainPage.Navigation.PopToRootAsync();
                
            });

            VersionList = new ObservableCollection<ListItem>();
            var selectedGame = SystemControl.GetActiveGame();
            var GameFilesPath = Path.Combine(SystemControl.DirectoryHelper.GetBaseDirectory(),selectedGame, "gamefiles");
            DirectoryInfo GamefilesDirectory = new DirectoryInfo(GameFilesPath);
            var GameFiles = GamefilesDirectory.GetFiles();
            foreach (var file in GameFiles)
            {
                VersionList.Add(new ListItem(file.Name));
            }
        }

        public ICommand SelectVersionCommand { get; }

    }
}
