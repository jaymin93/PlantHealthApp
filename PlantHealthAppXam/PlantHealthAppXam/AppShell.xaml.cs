using PlantHealthAppXam.ViewModels;
using PlantHealthAppXam.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace PlantHealthAppXam
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
        }

    }
}
