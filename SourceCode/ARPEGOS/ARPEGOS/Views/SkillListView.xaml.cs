using ARPEGOS.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARPEGOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SkillListView: ContentPage
    {
        public SkillListView ()
        {
            InitializeComponent();
            this.BindingContext = new SkillListViewModel();
        }
    }
}
