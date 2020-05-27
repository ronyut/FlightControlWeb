using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace FlightControlWeb.Models
{
    public class Segment
    {
        [Required]
        public double longitude { get; set; }
        
        [Required]
        public double latitude { get; set; }
        
        [Required]
        [JsonProperty("timespan_seconds")]
        public int timespan { get; set; }

        [JsonConstructor]
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

        override
        public string ToString()
        {
            return "(" + this.latitude + ", " + this.longitude + ") -> " + this.timespan;
        }

        public void Validate()
        {
            var coord = new Coordinate(longitude, latitude);
            coord.Validate();

            if (timespan <= 0)
            {
                throw new Exception("Segment timespan must be positive");
            }
        }
    }
}