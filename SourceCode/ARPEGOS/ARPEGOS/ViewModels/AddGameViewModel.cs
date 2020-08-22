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
        private string _game, _gamefolder, _checkMessage, _selectMessage, _addMessage;
        private Plugin.FilePicker.Abstractions.FileData file;

        public string Game 
        { 
            get => _game; 
            set => this.SetProperty(ref this._game, value); 
        }
        public string Gamefolder 
        { 
            get => _gamefolder; 
            set => this.SetProperty(ref this._gamefolder, value); 
        }
        public string CheckMessage 
        { 
            get => _checkMessage; 
            set => this.SetProperty(ref this._checkMessage, value);
        }
        public string SelectMessage 
        { 
            get => _selectMessage; 
            set => this.SetProperty(ref this._selectMessage, value);
        }
        public string AddMessage 
        { 
            get => _addMessage; 
            set => this.SetProperty(ref this._addMessage, value);
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
