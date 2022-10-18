using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetPlantHealthDetails
{
    public class PlantHealthDeatils : TableEntity
    {
        public PlantHealthDeatils()
        {

        }
        public PlantHealthDeatils(string skey, string srow)
        {
            PartitionKey = skey;
            RowKey = srow;
        }
        public DateTime CapturedTime { get; set; }
        public string longitude { get; set; }

        public string latitude { get; set; }
        public string ImageURL { get; set; }
        public bool Pesticidesprayed { get; set; } = false;
        public string ETag { get; set; }
        public string RowKey { get; set; }
        public string PartitionKey { get; set; }

    }
}
