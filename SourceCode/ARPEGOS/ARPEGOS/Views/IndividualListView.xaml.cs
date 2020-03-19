namespace ARPEGOS.Views
{
    using ARPEGOS.Models;
    using ARPEGOS.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class IndividualListView : ContentPage
    {
        readonly IndividualListViewModel viewModel;
        public IndividualListView(ClassElement element)
        {
            InitializeComponent();
            viewModel = new IndividualListViewModel(element);
            this.BindingContext = viewModel;
        }
    }
}