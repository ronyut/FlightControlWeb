using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class FlightItem
    {
        public long id { get; set; }
        public string company { get; set; }
        public string flightName { get; set; }
        public int passengers { get; set; }
        public double initLongitude { get; set; }
        public double initLatitude { get; set; }
        public int takeoffTime { get; set; } // unix time
        public bool ended { get; set; }
    }
}
