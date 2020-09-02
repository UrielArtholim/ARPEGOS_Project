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
    public partial class StageView: ContentPage
    {
        public StageView (int stageCounter)
        {
            InitializeComponent();
            this.BindingContext = DependencyHelper.Container.Resolve<StageViewModel>(new NamedParameter("counter", stageCounter));
        }
    }
}