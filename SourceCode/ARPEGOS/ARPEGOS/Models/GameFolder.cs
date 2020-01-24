using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Models
{
    class GameFolder
    {
        public string DisplayName { get; set; }
        public GameFolder(string item)
        {
            DisplayName = item;
        }
    }
}
