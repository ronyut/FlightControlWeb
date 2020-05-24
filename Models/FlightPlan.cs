using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlightControlWeb.Models
{
    public class FlightPlan
    {
        [Key]
        [JsonIgnore]
        public int _id { get; set;}
        public int flight_id { get; set; }
        [Required]
        public int passengers { get; set; }
        [Required]
        public string company_name { get; set; }
        public InitialLocation initial_location { get; set; }
        [Required]
        public IEnumerable<Segment> segments { get; set; }

        public FlightPlan (int flight_id, int passengers, string company_name, InitialLocation initial_location, IEnumerable<Segment> segments)
        {
            this.flight_id = flight_id;
            this.passengers = passengers;
            this.company_name = company_name;
            this.initial_location = initial_location;
            this.segments = segments;
        }
    }
}