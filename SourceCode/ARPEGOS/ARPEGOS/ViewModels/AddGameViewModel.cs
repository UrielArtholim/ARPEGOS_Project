using ARPEGOS.Helpers;
using ARPEGOS.Models;
using ARPEGOS.Services;
using ARPEGOS.ViewModels.Base;
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
        private string game, gamefolder, checkMessage, selectMessage, addMessage;
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
        public string CheckMessage 
        { 
            get => checkMessage; 
            set => this.SetProperty(ref this.checkMessage, value);
        }
        public string SelectMessage 
        { 
            get => selectMessage; 
            set => this.SetProperty(ref this.selectMessage, value);
        }
        public string AddMessage 
        { 
            get => addMessage; 
            set => this.SetProperty(ref this.addMessage, value);
        }

        public AddGameViewModel () 
        {
            this.CheckCommand = new Command(()=> 
            {
                var gameFolder = FileService.GetGameBasePath(Game);
                if (!System.IO.Directory.Exists(gameFolder))
                {
                    FileService.CreateGameFolderStructure(Game);
                    this.CheckMessage = $"Juego añadido";
                }
                else
                    this.CheckMessage = $"El juego ya existe";
            });

            this.SelectCommand = new Command(async() =>
            {
                this.file = await CrossFilePicker.Current.PickFile();
                this.SelectMessage = $"Fichero seleccionado: {file.FileName}";
            });

            this.AddCommand = new Command(() =>
            {
                var filename = $"{FileService.EscapedName(file.FileName.Split('.').First())}{'.'}{file.FileName.Split('.').Last()}";
                var oldPath = file.FilePath;
                var newPath = Path.Combine(FileService.GetGameBasePath(Game), FileService.GamesPath, filename);
                if (!File.Exists(newPath))
                    this.AddMessage = $"El fichero {filename} no se ha podido añadir";
                else
                {
                    this.AddMessage = $"El fichero se ha añadido correctamente";
                    //File.Delete(oldPath);
                }
                App.Navigation.PopAsync();
            });

            this.Game = string.Empty;
            this.CheckMessage = $"No game folder has been checked yet";
            this.SelectMessage = $"No game file has been selected yet";
            this.AddMessage = $"No game has been added yet";
        }

        public ICommand CheckCommand { get; set; }

        public ICommand SelectCommand { get; set; }

        public ICommand AddCommand { get; set; }
    }
}
