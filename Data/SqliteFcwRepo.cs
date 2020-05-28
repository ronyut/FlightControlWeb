/* This is the Sqlite main repository.
 * It sends all db query requests to the QueryManager class.
 * 
 * Author: Rony Utesvky.
 * Date: May 28, 2020
 */

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
         * Ctor
         */
        public SqliteFcwRepo(FcwContext context)
        {
            _context = context;
            _queryManager = new QueryManager(context.conn);
            _httpClient = new HttpClient();
        }

        /*
         * Function: GetFlightPlanByIdAsync
         * Description: This asynchronous method checks if the flight ID is present in the DB, and
         *              if not, it sends an async request to the external servers.
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
                // Stop ignoring flight
                _queryManager.SetFlightIgnored(id, false);
                throw new Exception("Preventing infinite loop");
            }
            // Start ignoring flight
             _queryManager.SetFlightIgnored(id, true);

            // Look for the flight ID in external servers
            var servers = _queryManager.GetAllServers();
            foreach (var server in servers)
            {
                try
                {
                    // Get FlightPlan from external server
                    var url = server.url + "/api/FlightPlan/" + id;
                    string ext = await _httpClient.GetStringAsync(url);
                    flightPlan = JsonConvert.DeserializeObject<FlightPlan>(ext);

                    // If successful, the FlightPlan's fields won't be null
                    if (flightPlan.segments != null) break;
                }
                catch (Exception e)
                {
                    var err = @"Problem occurred while fetching data from
                                server "+ server.url + " (" + e.Message +")";
                    Console.WriteLine(err);
                }
            }

            // Stop ignoring flight
            _queryManager.SetFlightIgnored(id, false);

            // Save external flight to DB, even if it might get deleted and we won't know
            if (flightPlan.segments != null)
            {
                _queryManager.PostFlightPlan(flightPlan);
                return flightPlan;
            }
            
            throw new Exception("Flight ID not found");
        }

        /*
         * Function: GetFlightsByTimeAsync
         * Description: This method sends an async request to the external servers, and asks for
                        the relevant flights for `relative_to`, and then it also gets the relevant
         *              flights in our db.
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
         * Function: GetExternalFlightsByTime
         * Description: This method sends an async request to the external servers, and asks for
                        the relevant flights for `relative_to`.
         */
        public async Task<IEnumerable<Flight>> GetExternalFlightsByTime(MyDateTime relativeTo)
        {
            var flights = new List<Flight> {};

            var servers = _queryManager.GetAllServers();
            foreach (var server in servers)
            {
                try
                {
                    // Send the request and try to get the Flights list
                    var url = server.url + "/api/Flights?relative_to=" + relativeTo.iso;
                    string ext = await _httpClient.GetStringAsync(url);
                    var severFlights = JsonConvert.DeserializeObject<IEnumerable<Flight>>(ext);

                    flights.AddRange(severFlights);
                }
                catch(Exception e)
                {
                    var err = @"Problem occurred while fetching data from
                                server "+ server.url + " (" + e.Message +")";
                    Console.WriteLine(err);
                }
            }

            // Set all flight as external
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
         * Description: Posts a flight plan to DB
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
    
        /*
         * Function: PostServer
         * Description: Posts a new server to DB
         */
        public Response PostServer(Server server)
        {
            try
            {
                _queryManager.PostServer(server);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return new Response("POST", true, "Server added");
        }

        /*
         * Function: DeleteServer
         * Description: Deletes a server by ID from the DB
         */
        public Response DeleteServer(string id)
        {
            try
            {
                _queryManager.DeleteServer(id);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return new Response("DELETE", true, "Server deleted");
        }

        /*
         * Function: GetAllServers
         * Description: Returns all the servers in the DB.
         */
        public IEnumerable<Server> GetAllServers()
        {
            return _queryManager.GetAllServers();
        }

    }
}