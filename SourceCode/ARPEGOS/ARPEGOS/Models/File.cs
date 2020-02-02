using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ARPEGOS.Models
{
    public class File
    {
        public string Name { get; private set; }
        public string GamePath { get; private set; }

        public File(string path)
        {
            GamePath = path;
            Name = Path.GetDirectoryName(GamePath);
        }
    }
}
