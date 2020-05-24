using System.ComponentModel.DataAnnotations;

namespace FlightControlWeb.Models
{
    public class InitialLocation
    {
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string date_time { get; set; }

        public InitialLocation (double longitude, double latitude, string date_time)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.date_time = date_time;
        }
    }
}