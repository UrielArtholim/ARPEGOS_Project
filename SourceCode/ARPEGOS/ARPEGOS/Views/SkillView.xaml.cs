using ARPEGOS.Helpers;
using ARPEGOS.ViewModels;
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ARPEGOS.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SkillView: ContentPage
    {
        public SkillView ()
        {
            InitializeComponent();
            this.BindingContext = DependencyHelper.Container.Resolve<SkillViewModel>();
        }
    }
}