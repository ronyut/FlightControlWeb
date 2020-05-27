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

        public Coordinate(object longitude, object latitude)
        {
            this.longitude = (double) longitude;
            this.latitude = (double) latitude;
        }

        public void Validate()
        {
            if (longitude > 180 || longitude < -180 || latitude > 90 || latitude < -90)
            {
                throw new Exception("Invalid coordinate in initial location");
            }
        }
    }
}