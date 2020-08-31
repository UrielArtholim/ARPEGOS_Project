using ARPEGOS.Helpers;
using ARPEGOS.ViewModels;
using Autofac;
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
            this.BindingContext = DependencyHelper.Container.Resolve<SkillListViewModel>();
        }
    }
}
