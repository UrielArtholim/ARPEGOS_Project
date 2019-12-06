using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace ARPEGOS.Models
{
    public class Ontology
    {
        public string gamesFolder = Directory.GetCurrentDirectory();

        public Ontology()
        {
            // Windows path (look for how to vinculate Files with cross-platform projects)
            Debug.WriteLine(gamesFolder);

        }

    }
}
