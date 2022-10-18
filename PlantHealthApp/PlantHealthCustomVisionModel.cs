using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlantHealthApp
{

    public class PlantHealthCustomVisionModel
        {
            public string id { get; set; }
            public string project { get; set; }
            public string iteration { get; set; }
            public DateTime created { get; set; }
            public List<Prediction> predictions { get; set; }
        }
}
