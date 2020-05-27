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

        /*
         * Function: 
         * Description: 
         */
        public SqliteFcwRepo(FcwContext context)
        {
            _context = context;
            _queryManager = new QueryManager(context.conn);

        }

        /*
         * Function: 
         * Description: 
         */
        public IEnumerable<FlightPlan> GetAllFlightPlans()
        {
            throw new Exception("Not implemented!");
        }

        /*
         * Function: 
         * Description: 
         */
        public FlightPlan GetFlightPlanById(string id)
        {
            return _queryManager.GetFlightPlanById(id);
        }

        /*
         * Function: 
         * Description: 
         */
        public async Task<FlightPlan> GetFlightPlanByIdAsync(string id, HttpClient httpClient)
        {
            // Search in internal DB
            var flightPlan = GetFlightPlanById(id);
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
                    string ext = await httpClient.GetStringAsync(url);
                    flightPlan = JsonConvert.DeserializeObject<FlightPlan>(ext);

                    if (flightPlan.segments != null) break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Problem with server " + server.url + " (" + e +")");
                }
            }

            // Stop ignoring flight
            _queryManager.SetFlightIgnored(id, false);

            // Save external flight to DB
            if (flightPlan != null)
            {
                _queryManager.PostFlightPlan(flightPlan, true, id);
                return flightPlan;
            }
            
            throw new Exception("Flight ID not found");
        }

        /*
         * Function: 
         * Description: 
         */
        public async Task<IEnumerable<Flight>> GetFlightsByTimeAsync(string date, bool isExternal,
                                                                     HttpClient httpClient)
        {
            var relativeTo = new MyDateTime(date);
            var flights = new List<Flight> {};

            // Get flights from external servers
            if (isExternal)
            {
                var externalFlights = await GetFlightsByTimeExternal(relativeTo, httpClient);
                flights.AddRange(externalFlights);
            }

            flights.AddRange(_queryManager.GetFlightsByTime(relativeTo, isExternal));
            return flights;
        }

        /*
         * Function: 
         * Description: 
         */
        public async Task<IEnumerable<Flight>> GetFlightsByTimeExternal(MyDateTime relativeTo,
                                                                        HttpClient httpClient)
        {
            var flights = new List<Flight> {};
            var temp = new List<Flight> {};

            var servers = _queryManager.GetServers();
            foreach (var server in servers)
            {
                try
                {
                    var url = server.url + "/api/Flights?relative_to=" + relativeTo.iso;
                    Console.WriteLine("@@@ " + url);
                    string ext = await httpClient.GetStringAsync(url);
                    var severFlights = JsonConvert.DeserializeObject<IEnumerable<Flight>>(ext);

                    // Count results
                    temp.AddRange(severFlights);
                    if (temp.Count == 0) continue;
                    Console.WriteLine("@@@ Count: " + temp.Count);
                    temp.Clear();

                    // put in db @@@@

                    // set is_ext = true @@@

                    flights.AddRange(severFlights);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Problem with server "+ server.url + " (" + e +")");
                }
            }
            return flights;
        }

        /*
         * Function: 
         * Description: 
         */
        public Response DeleteFlightById(string id)
        {
            try
            {
                _queryManager.DeleteFlightById(id);
            }
            catch (Exception e)
            {
                return new Response("DELETE", false, e.Message);
            }

            return new Response("DELETE", true, "The flight has been successfully deleted");
        }

        /*
         * Function: PostFlightPlan
         * Description: 
         */
        public Response PostFlightPlan(FlightPlan flightPlan)
        {
            try
            {
                _queryManager.PostFlightPlan(flightPlan);
            }
            catch (Exception e)
            {
                return new Response("POST", false, e.Message);
            }

            return new Response("POST", true, "Flight plan has been added");
        }
    }
}