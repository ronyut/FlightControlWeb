using Newtonsoft.Json;

namespace FlightControlWeb.Models
{
    public class Segment
    {
        public double longitude { get; set; }
        public double latitude { get; set; }
        [JsonProperty("timespan_seconds")]
        public int timespan { get; set; }

        public Segment(double longitude, double latitude, int timespan)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.timespan = timespan;
        }

        public Segment(Coordinate coord, int timespan)
        {
            this.longitude = coord.longitude;
            this.latitude = coord.latitude;
            this.timespan = timespan;
        }
    }
}