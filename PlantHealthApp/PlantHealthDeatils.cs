using Microsoft.Azure.Cosmos.Table;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantHealthApp
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
        public Date TodayDate { get; set; }
        public string longitude { get; set; }

        public string latitude { get; set; }
        public string ImageURL { get; set; }
        public bool Pesticidesprayed { get; set; } = false;

    }
}
