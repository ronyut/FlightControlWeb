using Newtonsoft.Json;

namespace FlightControlWeb.Models
{
    public class InitialLocation
    {
        public double longitude { get; set; }
        public double latitude { get; set; }
        [JsonProperty("date_time")]
        public string dateTime { get; set; }

        public InitialLocation (double longitude, double latitude, string dateTime)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.dateTime = dateTime;
        }

        public InitialLocation (Coordinate coord, string dateTime)
        {
            this.longitude = coord.longitude;
            this.latitude = coord.latitude;
            this.dateTime = dateTime;
        }
    }
}