using ARPEGOS.Interfaces;
using ARPEGOS.iOS.Services;

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

        public string GetBaseDirectory()
        {
            throw new System.NotImplementedException();
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