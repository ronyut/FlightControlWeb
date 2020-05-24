using System.ComponentModel.DataAnnotations;

namespace FlightControlWeb.Models
{
    public class Segment
    {
        public double longitude { get; set; }
        public double latitude { get; set; }
        public int timespan_seconds { get; set; }

        public Segment (double longitude, double latitude, int timespan_seconds)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.timespan_seconds = timespan_seconds;
        }
    }
}