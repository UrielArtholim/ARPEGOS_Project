using ARPEGOS.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ARPEGOS.ViewModels
{
    public abstract class DirectoryViewModel
    {
        public static readonly IDirectory directoryHelper = DependencyService.Get<IDirectory>();
        public abstract void GetList();
    }
}
