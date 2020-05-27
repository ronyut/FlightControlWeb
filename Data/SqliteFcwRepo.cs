using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FlightControlWeb.Models;
using Newtonsoft.Json;

namespace FlightControlWeb.Data
{
    public class SqliteFcwRepo : IFcwRepo
    {
        private readonly FcwContext _context;
        private QueryManager _queryManager;
        private HttpClient _httpClient;

        /*
         * Function: 
         * Description: 
         */
        public SqliteFcwRepo(FcwContext context)
        {
            _context = context;
            _queryManager = new QueryManager(context.conn);
            _httpClient = new HttpClient();
        }

        /*
         * Function: 
         * Description: 
         */
        public async Task<FlightPlan> GetFlightPlanByIdAsync(string id)
        {
            // Search in internal DB
            var flightPlan = _queryManager.GetFlightPlanById(id);
            if (flightPlan != null)
            {
                return flightPlan;
            }

            // Prevent infinite loop that happens due to ping pong between connected servers
            if (_queryManager.IsIgnoredFlight(id))
            {
                _queryManager.SetFlightIgnored(id, false);
                throw new Exception("Preventing infinite loop");
            }
             _queryManager.SetFlightIgnored(id, true);

            // Look for the flight ID in external servers
            var servers = _queryManager.GetServers();
            foreach (var server in servers)
            {
                try
                {
                    var url = server.url + "/api/FlightPlan/" + id;
                    string ext = await _httpClient.GetStringAsync(url);
                    flightPlan = JsonConvert.DeserializeObject<FlightPlan>(ext);

                    if (flightPlan.segments != null) break;
                }
                catch (Exception e)
                {
                    var err = @"Problem occurred while fetching data from
                                server "+ server.url + " (" + e +")";
                    Console.WriteLine(err);
                }
            }

            // Stop ignoring flight
            _queryManager.SetFlightIgnored(id, false);

            // No need to save external flight to DB, as it might get deleted and we won't know
            if (flightPlan != null)
            {
                return flightPlan;
            }
            
            throw new Exception("Flight ID not found");
        }

        /*
         * Function: 
         * Description: 
         */
        public async Task<IEnumerable<Flight>> GetFlightsByTimeAsync(string date, bool isExternal)
        {
            var relativeTo = new MyDateTime(date);
            var flights = new List<Flight> {};

            // Get flights from external servers
            if (isExternal)
            {
                var externalFlights = await GetExternalFlightsByTime(relativeTo);
                flights.AddRange(externalFlights);
            }

            // Get internal flights
            flights.AddRange(_queryManager.GetFlightsByTime(relativeTo));
            return flights;
        }

        /*
         * Function: 
         * Description: 
         */
        public async Task<IEnumerable<Flight>> GetExternalFlightsByTime(MyDateTime relativeTo)
        {
            var flights = new List<Flight> {};

            var servers = _queryManager.GetServers();
            foreach (var server in servers)
            {
                try
                {
                    var url = server.url + "/api/Flights?relative_to=" + relativeTo.iso;
                    string ext = await _httpClient.GetStringAsync(url);
                    var severFlights = JsonConvert.DeserializeObject<IEnumerable<Flight>>(ext);

                    flights.AddRange(severFlights);
                }
                catch(Exception e)
                {
                    var err = @"Problem occurred while fetching data from
                                server "+ server.url + " (" + e +")";
                    Console.WriteLine(err);
                }
            }

            // set all flight as external
            foreach (var flight in flights)
            {
                flight.isExternal = true;
            }

            return flights;
        }

        /*
         * Function: DeleteFlightById
         * Description: Deletes a flight and all its segments
         */
        public Response DeleteFlightById(string id)
        {
            try
            {
                _queryManager.DeleteFlightById(id);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return new Response("DELETE", true, "The flight has been successfully deleted");
        }

        /*
         * Function: PostFlightPlan
         * Description: Post a flight plan to DB
         */
        public Response PostFlightPlan(FlightPlan flightPlan)
        {
            try
            {
                _queryManager.PostFlightPlan(flightPlan);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return new Response("POST", true, "Flight plan has been added");
        }
    }
}