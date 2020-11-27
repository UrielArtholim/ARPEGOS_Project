
namespace ARPEGOS
{
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using ARPEGOS.Helpers;
    using ARPEGOS.Services;
    using ARPEGOS.Views;

    using Xamarin.Essentials;
    using Xamarin.Forms;

    public partial class App : Application
    {
        public static INavigation Navigation { get; set; }        

        public App()
        {
            this.InitializeComponent();
            _ = new DependencyHelper();

            if (VersionTracking.IsFirstLaunchEver)
                this.CreateTestGame();
            Device.SetFlags(new string[] { "RadioButton_Experimental" });
            Device.SetFlags(new string[] { "AppTheme_Experimental" });
            DependencyHelper.CurrentContext.AppMainView = new MainView();
            App.Navigation = DependencyHelper.CurrentContext.AppMainView?.Detail?.Navigation;
            this.MainPage = DependencyHelper.CurrentContext.AppMainView;
        }

        private void CreateTestGame()
        {
            var assembly = Assembly.GetExecutingAssembly();
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
        }

        private void WriteResourceToFile(string resourcePath, string filePath)
        {
            using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            resource.CopyTo(file);
        }
    }
}
