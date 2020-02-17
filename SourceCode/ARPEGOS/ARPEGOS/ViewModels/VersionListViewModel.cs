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
            SelectVersionCommand = new Command<ListItem>(async item =>
            {
                var selectedItem = this.VersionList.FirstOrDefault(currentVar => currentVar.ItemName == item.ItemName);
                SystemControl.UpdateActiveVersion(selectedItem.ItemName);
                await Xamarin.Forms.Application.Current.MainPage.Navigation.PopToRootAsync();
                
            });

            VersionList = new ObservableCollection<ListItem>();
            var selectedGame = SystemControl.GetActiveGame();
            var GameFilesPath = Path.Combine(SystemControl.directoryHelper.GetBaseDirectory(),selectedGame, "gamefiles");
            DirectoryInfo GamefilesDirectory = new DirectoryInfo(GameFilesPath);
            var GameFiles = GamefilesDirectory.GetFiles();
            foreach (var file in GameFiles)
            {
                var filename = file.Name.Split('.');
                VersionList.Add(new ListItem(filename[1]));
            }
        }

        public ICommand SelectVersionCommand { get; }

    }
}
