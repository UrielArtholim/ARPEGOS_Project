using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ARPEGOS.Models
{
    public class GameFile
    {
        public string Name { get; private set; }
        public string GamePath { get; private set; }

        public GameFile(string path)
        {
            GamePath = path;
            Name = Path.GetDirectoryName(GamePath);
        }
    }
}
