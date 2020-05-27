using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace FlightControlWeb.Models
{
    public class InitialLocation
    {
        [Required]
        public double longitude { get; set; }

        [Required]
        public double latitude { get; set; }
        
        [Required]
        [JsonProperty("date_time")]
        public string dateTime { get; set; }

        [JsonConstructor]
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

        override
        public string ToString()
        {
            return "(" + this.latitude + ", " + this.longitude + ") -> " + this.dateTime;
        }

        public void Validate()
        {
            var initCoord = new Coordinate(longitude, latitude);
            initCoord.Validate();
            
            // Will throw excpetion if not valid
            var date = new MyDateTime(dateTime);
        }
    }
}