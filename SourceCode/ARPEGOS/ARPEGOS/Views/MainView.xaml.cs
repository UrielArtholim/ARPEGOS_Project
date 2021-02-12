using ARPEGOS.Services;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARPEGOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainView : MasterDetailPage
    {
        public MainView()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            if (VersionTracking.IsFirstLaunchEver)
                await this.CreateTestGame();
            base.OnAppearing();
        }

        private async Task CreateTestGame()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var status = await Xamarin.Essentials.Permissions.RequestAsync<Permissions.StorageWrite>();
            if(status == PermissionStatus.Granted)
            {
                foreach (var game in assembly.GetManifestResourceNames().Where(x => x.EndsWith(".owl")).Select(x => x.Split('.')).GroupBy(x => x[2]))
                {
                    if (!FileService.CreateGameFolderStructure(game.Key))
                        continue;

                    foreach (var file in game)
                    {
                        if (file[3].ToLowerInvariant() == "characters")
                        {
                            var path = FileService.GetCharacterFilePath(file[4], game.Key);
                            this.WriteResourceToFile(string.Join('.', file), path);
                        }

                        if (file[3].ToLowerInvariant() == "gamefiles")
                        {
                            var path = FileService.GetGameFilePath(game.Key, file[4]);
                            this.WriteResourceToFile(string.Join('.', file), path);
                        }
                    }
                }

                foreach (var game in assembly.GetManifestResourceNames().Where(x => x.EndsWith(".jpg")).Select(x => x.Split('.')).GroupBy(x => x[2]))
                {
                    foreach (var file in game)
                    {
                        if (file[3].ToLowerInvariant() == "gamefiles")
                        {
                            var path = FileService.GetGameFilePath(game.Key, file[4]);
                            this.WriteResourceToFile(string.Join('.', file), path);
                        }
                        else
                        {
                            var path = Path.Combine(FileService.GetGameBasePath(game.Key), $"{file[3]}.jpg");
                            this.WriteResourceToFile(string.Join('.', file), path);
                        }
                    }
                }
            }
        }

        private void WriteResourceToFile(string resourcePath, string filePath)
        {
            using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            resource.CopyTo(file);
        }
    }
}