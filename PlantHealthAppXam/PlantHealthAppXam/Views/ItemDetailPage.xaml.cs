using PlantHealthAppXam.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace PlantHealthAppXam.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}