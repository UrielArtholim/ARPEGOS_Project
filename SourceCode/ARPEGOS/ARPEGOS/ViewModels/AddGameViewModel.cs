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

        public string Game { get => game; set { game = value; OnPropertyChanged(); } }
        public string Gamefolder { get => gamefolder; set { gamefolder = value; OnPropertyChanged(); } }
        public string CheckMessage { get => checkMessage; set { checkMessage = value; OnPropertyChanged(); } }
        public string SelectMessage { get => selectMessage; set { selectMessage = value; OnPropertyChanged(); } }
        public string AddMessage { get => addMessage; set { addMessage = value; OnPropertyChanged(); } }




        public AddGameViewModel () 
        {
            CheckMessage = $"No game folder has been checked yet";
            SelectMessage = $"No game file has been selected yet";
            AddMessage = $"No game has been added yet";
        }

        public ICommand CheckCommand 
        { 
            get 
            {
                return new Command(() => {
                    var gameFolder = FileService.GetGameBasePath(Game);
                    if(!System.IO.Directory.Exists(gameFolder))
                    {
                        FileService.CreateGameFolderStructure(Game);
                        CheckMessage = $"Juego añadido";
                    }
                    else
                        CheckMessage = $"El juego ya existe";
                }); 
            } 
        }

        public ICommand SelectCommand
        {
            get
            {
                return new Command(async () => {
                    file = await CrossFilePicker.Current.PickFile();
                    SelectMessage = $"Fichero seleccionado: {file.FileName}";
                });
            }
        }

        public ICommand AddCommand
        {
            get
            {
                return new Command(() => {
                    var filename = $"{FileService.EscapedName(file.FileName.Split('.').First())}{'.'}{file.FileName.Split('.').Last()}";
                    var oldPath = file.FilePath;
                    var newPath = Path.Combine(FileService.GetGameBasePath(Game), FileService.GamesPath, filename);
                    if (!File.Exists(newPath))
                        AddMessage = $"El fichero {filename} no se ha podido añadir";
                    else
                    {
                        AddMessage = $"El fichero se ha añadido correctamente";
                        //File.Delete(oldPath);
                    }
                });
            }
        }
    }
}
