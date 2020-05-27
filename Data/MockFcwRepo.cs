using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FlightControlWeb.Models;

namespace FlightControlWeb.Data
{
    public class MockFcwRepo : IFcwRepo
    {
        private HttpClient _httpClient;
        public MockFcwRepo()
        {
            _httpClient = new HttpClient();
        }

        public FlightPlan GetFlightPlanById(string id)
        {
            var segments = new List<Segment>{
                new Segment(35, 35, 100),
                new Segment(36, 36, 20)
            };

            InitialLocation initialLocation = new InitialLocation(34, 34, "2020-05-22T15:30:00Z");
            return new FlightPlan(222, "El Al 0", initialLocation, segments);
        }

        public async Task<IEnumerable<Flight>> GetFlightsByTimeAsync(string date, bool isExternal)
        {
            var flights = new List<Flight>{
                new Flight("FOJI88", 38, -38, 1000, "Air France",
                           new MyDateTime("2020-05-25T11:33:00Z"), false),
                new Flight("POLA11", 22.1, -23.8, 1, "Swiss Air",
                           new MyDateTime("2020-05-25T11:33:01Z"), false),
            };

            return flights;
        }

        public Response DeleteFlightById(string id)
        {
            return new Response("DELETE", true, "The flight has been deleted");
        }

        public Response PostFlightPlan(FlightPlan flightPlan)
        {
            return new Response("POST", true, "The flight plan has been added");
        }

        public async Task<FlightPlan> GetFlightPlanByIdAsync(string id)
        {
            var segments = new List<Segment>{
                new Segment(35, 35, 100),
                new Segment(36, 36, 20)
            };

            InitialLocation initialLocation = new InitialLocation(34, 34, "2020-05-22T15:30:00Z");
            return new FlightPlan(222, "External Airlines", initialLocation, segments);
        }
    }
}