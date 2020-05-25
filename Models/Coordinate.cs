using System;

namespace FlightControlWeb.Models
{
    public class Coordinate
    {
        public double longitude { get; set; }
        public double latitude { get; set; }

        public Coordinate(double longitude, double latitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
        }

        public Coordinate(string longitude, string latitude)
        {
            this.longitude = Double.Parse(longitude);
            this.latitude = Double.Parse(latitude);
        }
    }
}