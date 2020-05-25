using System.Text.Json.Serialization;

namespace FlightControlWeb.Models
{
    public class Flight
    {
        public string flight_id { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public int passengers { get; set; }
        public string company_name { get; set; }
        public string date_time { get; set; }
        public bool is_external { get; set; }
        [JsonIgnore]
        public double angle { get; set; }
        [JsonIgnore]
        public double ETL { get; set; }

        public Flight(string flight_id, double longitude, double latitude, int passengers, string company_name, MyDateTime date_time, bool is_external)
        {
            this.flight_id = flight_id;
            this.longitude = longitude;
            this.latitude = latitude;
            this.passengers = passengers;
            this.company_name = company_name;
            this.date_time = date_time.iso;
            this.is_external = is_external;
        }
    }
}