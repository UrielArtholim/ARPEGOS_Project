using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ARPEGOS.Droid.Services;
using ARPEGOS.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(DirectoryService))]
namespace ARPEGOS.Droid.Services
{
    class DirectoryService : IDirectory
    {
        static string packageName = Android.App.Application.Context.PackageName;
        public string baseDirectoryPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Android/data", packageName, "files");

        public int Permision { get; private set; }

        public void ClearBaseDirectory()
        {
            DirectoryInfo directory = new DirectoryInfo(baseDirectoryPath);
            foreach (DirectoryInfo dir in directory.GetDirectories())
                dir.Delete(true);
        }

        public string CreateDirectory(string directoryName)
        {
            var directoryPath = Path.Combine(baseDirectoryPath, directoryName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return directoryPath;
        }

        public string CreateDirectory(string rootDirectoryPath, string directoryName)
        {
            var directoryPath = Path.Combine(rootDirectoryPath, directoryName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return directoryPath;
        }

        public void RemoveDirectory(string directoryName)
        {
            var directoryPath = Path.Combine(baseDirectoryPath, directoryName);
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            foreach (DirectoryInfo dir in directory.GetDirectories())
                dir.Delete(true);
        }

        public string RenameDirectory(string oldDirectoryName, string newDirectoryName)
        {
            var oldDirectoryPath = Path.Combine(baseDirectoryPath, oldDirectoryName);
            var newDirectoryPath = Path.Combine(baseDirectoryPath, newDirectoryName);
            if (Directory.Exists(oldDirectoryPath))
                Directory.Move(oldDirectoryPath, newDirectoryPath);
            return newDirectoryPath;
        }
    }
}