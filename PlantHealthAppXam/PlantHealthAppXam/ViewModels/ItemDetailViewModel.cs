using PlantHealthAppXam.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PlantHealthAppXam.ViewModels
{
    [QueryProperty(nameof(IMGURL), nameof(IMGURL))]

    public class ItemDetailViewModel : BaseViewModel
    {
        private string imgurl;

        public string Id { get; set; }

        public string IMGURL
        {
            get => imgurl;
            set => SetProperty(ref imgurl, value);
        }
    }
}
