using PlantHealthAppXam.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PlantHealthAppXam.ViewModels
{
    [QueryProperty(nameof(IMGURL), nameof(IMGURL))]
    [QueryProperty(nameof(longitude), nameof(longitude))]
    [QueryProperty(nameof(latitude), nameof(latitude))]
    public class ItemDetailViewModel : BaseViewModel
    {
        private string imgurl;
        public Command OpenMapCommand { get; }
        public string longitude { get; set; }
        public string latitude { get; set; }

        public string Id { get; set; }
        
        public string IMGURL
        {
            get => imgurl;
            set => SetProperty(ref imgurl, value);
        }


        public ItemDetailViewModel()
        {
            OpenMapCommand = new Command(async () => await OpenMapByLongitudeLatitude(longitude,latitude));
        }

        public async Task OpenMapByLongitudeLatitude(string Longitude, string Latitude)
        {
            var location = new Location(Convert.ToDouble(Longitude), Convert.ToDouble(Latitude));
            await Map.OpenAsync(location);
        }
    }
}
