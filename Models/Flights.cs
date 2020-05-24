using System.Text.Json.Serialization;

namespace FlightControlWeb.Models
{
    public class Flights
    {
        public int flight_id { get; set; }
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
    }
}