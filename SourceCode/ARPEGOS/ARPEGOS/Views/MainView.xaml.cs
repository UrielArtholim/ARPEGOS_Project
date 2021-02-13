using ARPEGOS.Services;
using System.Diagnostics;
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
            var statusWrite = PermissionStatus.Denied;
            var statusRead = PermissionStatus.Denied;
            statusWrite = await Permissions.RequestAsync<Permissions.StorageWrite>();
            statusRead = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (statusWrite == PermissionStatus.Granted && statusRead == PermissionStatus.Granted)
            {
                var path = string.Empty;
                foreach (var game in assembly.GetManifestResourceNames().Select(x => x.Split('.')).GroupBy(x => x[2]))
                {
                    if (!string.Equals("Fonts", game.Key))
                    {

                        if (!FileService.CreateGameFolderStructure(game.Key))
                            continue;

                        foreach (var file in game)
                        {
                            if (file[3].ToLowerInvariant() == "characters")
                            {
                                if (file.Last() == "owl")
                                    path = FileService.GetCharacterFilePath(file[4], game.Key);
                                else
                                    path = FileService.GetCharacterFilePath(file[4], game.Key).Replace(".owl", $".{file.Last()}");
                            }
                            else if (file[3].ToLowerInvariant() == "gamefiles")
                            {
                                if (file.Last() == "owl")
                                    path = FileService.GetGameFilePath(game.Key, file[4]);
                                else
                                    path = FileService.GetGameFilePath(game.Key, file[4]).Replace(".owl", $".{file.Last()}");
                            }
                            else
                            {
                                if (file.Last() != "owl")
                                    path = Path.Combine(FileService.GetGameBasePath(game.Key), $"{file[3]}.jpg");
                            }
                            await this.WriteResourceToFile(string.Join('.', file), path);
                        }
                    }
                }
            }
        }

        private async Task WriteResourceToFile(string resourcePath, string filePath)
        {
            if(!File.Exists(filePath))
            {
                using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
                using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await resource.CopyToAsync(file);
                var fileLength = file.Length;
            }            
        }
    }
}