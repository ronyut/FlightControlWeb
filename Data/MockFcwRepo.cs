/* This is a mock repository.
 * It generates dummy data and is not connected to the DB.
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
    public class MockFcwRepo : IFcwRepo
    {
        private HttpClient _httpClient;
        /*
         * Ctor
         */
        public MockFcwRepo()
        {
            _httpClient = new HttpClient();
        }

        /*
         * Function: GetFlightsByTimeAsync
         * Description: This method sends an async request to an external servers, and asks for
                        the relevant flights for `relative_to`, and then it also gets the relevant
         *              flights.
         */
        public async Task<IEnumerable<Flight>> GetFlightsByTimeAsync(string date, bool isExternal)
        {
            var flights = new List<Flight>{
                new Flight("FOJI88", 38, -38, 1000, "Air France",
                           new MyDateTime(date), false),
                new Flight("POLA11", 22.1, -23.8, 1, "Swiss Air",
                           new MyDateTime(date), false),
            };

            if (isExternal)
            {
                try
                {
                    // Send the request and try to get the Flights list
                    var server = "http://rony6.atwebpages.com";
                    var url = server + "/api/Flights?relative_to=" + date;
                    string ext = await _httpClient.GetStringAsync(url);
                    var severFlights = JsonConvert.DeserializeObject<IEnumerable<Flight>>(ext);

                    flights.AddRange(severFlights);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return flights;
        }

        /*
         * Function: DeleteFlightById
         * Description: Deletes a flight and all its segments
         */
        public Response DeleteFlightById(string id)
        {
            return new Response("DELETE", true, "The flight has been deleted");
        }

         /*
         * Function: PostFlightPlan
         * Description: Posts a flight plan
         */
        public Response PostFlightPlan(FlightPlan flightPlan)
        {
            return new Response("POST", true, "The flight plan has been added");
        }

        /*
         * Function: GetFlightPlanByIdAsync
         * Description: This asynchronous method checks if the flight ID is present in the DB, and
         *              if not, it sends an async request to the external servers.
         */
        public async Task<FlightPlan> GetFlightPlanByIdAsync(string id)
        {
            try
            {
                // Send the request and try to get the Flights list
                var server = "http://rony6.atwebpages.com";
                var url = server + "/api/FlightPlan/" + id;
                string ext = await _httpClient.GetStringAsync(url);
                var fp = JsonConvert.DeserializeObject<FlightPlan>(ext);
                
                if (fp.segments != null) return fp;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            // If external request failed
            var segments = new List<Segment>{
                new Segment(35, 35, 100),
                new Segment(36, 36, 20)
            };

            InitialLocation initialLocation = new InitialLocation(34, 34, "2020-05-22T15:30:00Z");
            return new FlightPlan(222, "External Deafult Airlines", initialLocation, segments);
        }

        /*
         * Function: PostServer
         * Description: Posts a new server
         */
        public Response PostServer(Server server)
        {
            return new Response("POST", true, "Server added");
        }

        /*
         * Function: DeleteServer
         * Description: Deletes a server by ID
         */
        public Response DeleteServer(string id)
        {
            return new Response("DELETE", true, "Server deleted");
        }

        /*
         * Function: GetAllServers
         * Description: Returns all the servers.
         */
        public IEnumerable<Server> GetAllServers()
        {
            var servers = new List<Server>{
                new Server("server1", "google.com"),
                new Server("server2", "ebay.com"),
                new Server("server3", "meow.cat")
            };

            return servers;
        }
    }
}