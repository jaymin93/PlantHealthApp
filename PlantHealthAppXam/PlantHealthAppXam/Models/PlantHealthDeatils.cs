using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PlantHealthAppXam.Models
{
    public class PlantHealthDeatils 
    {
        public PlantHealthDeatils()
        {

        }
        
        public DateTime CapturedTime { get; set; }

        public string longitude { get; set; }

        public string latitude { get; set; }
        public string ImageURL { get; set; }
        public bool Pesticidesprayed { get; set; } = false;

    }
}
