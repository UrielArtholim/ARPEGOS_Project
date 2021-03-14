using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
using ARPEGOS.Views;
using FFImageLoading.Helpers.Exif;
using Plugin.FilePicker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public class AddGameViewModel: BaseViewModel
    {
        private DialogService dialog;
        private string game, gamefolder;
        private Plugin.FilePicker.Abstractions.FileData file;

        public string Game 
        { 
            get => game; 
            set => this.SetProperty(ref this.game, value); 
        }
        public string Gamefolder 
        { 
            get => gamefolder; 
            set => this.SetProperty(ref this.gamefolder, value); 
        }

        public AddGameViewModel () 
        {
            dialog = DependencyHelper.CurrentContext.Dialog;
            this.CheckCommand = new Command(async ()=> 
            {
                var gameFolder = FileService.GetGameBasePath(Game);
                if (!System.IO.Directory.Exists(gameFolder))
                {
                    FileService.CreateGameFolderStructure(Game);
                    await dialog.DisplayAlert(string.Empty, "El juego indicado se ha añadido correctamente.");
                }
                else
                    await dialog.DisplayAlert(string.Empty , "El juego indicado ya existe.");
            });

            this.SelectCommand = new Command(async() =>
            {
                this.file = await CrossFilePicker.Current.PickFile();
                await DependencyHelper.CurrentContext.Dialog.DisplayAlert(string.Empty , $"El fichero seleccionado es:\n {file.FileName}");
            });

            this.AddCommand = new Command(async() =>
            {
                var filename = $"{FileService.EscapedName(file.FileName.Split('.').First())}{'.'}{file.FileName.Split('.').Last()}";
                var filedata = file.DataArray;
                var newPath = Path.Combine(FileService.GetGameBasePath(Game), FileService.GamesPath, filename);
                await File.WriteAllBytesAsync(newPath, filedata);
                if (!File.Exists(newPath))
                    await dialog.DisplayAlert(string.Empty , $"El fichero {filename} no se ha podido añadir");
                else
                    await dialog.DisplayAlert(string.Empty , $"El fichero {filename} se ha añadido correctamente");
                var mainView = App.Navigation.NavigationStack.First() as MainViewDetail;
                var mainViewModel = mainView.BindingContext as MainViewModel;
                mainViewModel.Load(mainViewModel.CurrentStatus);
                await App.Navigation.PopAsync();
            });

            this.Game = string.Empty;
        }

        public ICommand CheckCommand { get; set; }

        public ICommand SelectCommand { get; set; }

        public ICommand AddCommand { get; set; }
    }
}
