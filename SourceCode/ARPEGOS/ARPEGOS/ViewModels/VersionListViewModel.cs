namespace ARPEGOS.ViewModels
{
    using ARPEGOS.Models;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;

    class VersionListViewModel
    {
        public ObservableCollection<ListItem> VersionList { get; private set; }
        public VersionListViewModel()
        {
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

    }
}
