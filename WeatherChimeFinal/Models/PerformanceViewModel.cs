using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeatherChimeFinal.Models
{
    public class PerformanceViewModel
    {
        public string Location { get; set; }
        public string Temp { get; set; }
        public string Pressure { get; set; }
        public string Humidity { get; set; }
        public string WindSpeed { get; set; }

    }
}