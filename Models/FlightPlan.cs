namespace FlightControlWeb.Models
{
    public class FlightPlan
    {
        public int flight_id { get; set; }
        public int passengers { get; set; }
        public string company_name { get; set; }

        public FlightPlan (int flight_id, int passengers, string company_name) {
            this.flight_id = flight_id;
            this.passengers = passengers;
            this.company_name = company_name;
        }
    }
}