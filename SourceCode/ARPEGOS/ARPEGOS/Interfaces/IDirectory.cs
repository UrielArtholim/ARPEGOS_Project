using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Interfaces
{
    public interface IDirectory
    {
        string CreateDirectory(string directoryName);
        string CreateDirectory(string rootDirectoryName, string directoryName);
        string RenameDirectory(string oldDirectoryName, string newDirectoryName);
        void RemoveDirectory(string directoryName);
        void ClearBaseDirectory();
    }
}
