using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ARPEGOS.Interfaces;
using ARPEGOS.iOS.Services;
using Foundation;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(DirectoryService))]

namespace ARPEGOS.iOS.Services
{
    class DirectoryService : IDirectory
    {
        public void ClearBaseDirectory()
        {

        }

        public string CreateDirectory(string directoryName)
        {
            return null;
        }

        public string CreateDirectory(string rootDirectoryName, string directoryName)
        {
            return null;
        }

        public void RemoveDirectory(string directoryName)
        {
            
        }

        public string RenameDirectory(string oldDirectoryName, string newDirectoryName)
        {
            return null;
        }
    }
}