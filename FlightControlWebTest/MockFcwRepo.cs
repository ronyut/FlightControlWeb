/* This is a mock repository.
 * It generates dummy data and is not connected to the DB.
 * 
 * Author: Rony Utesvky.
 * Date: May 28, 2020
 * * Edited by Yehonatan Sofri in May 30, 2020.
 */

using FlightControlWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightControlWeb.Data
{
    public class MockFcwRepo : IFcwRepo
    {
        private FlightPlan _getFlightPlanByIdOutput;
        private List<Server> _getServersOutput;
        private Response _postServerOutput;
        private Response _deleteServerOutput;
        private Response _deleteFlightByIdOutput;
        private Response _postFlightPlanOutput;
        private List<Flight> _getInternalFlightsOutput;
        private List<Flight> _getAllFlightsOutput;


        public FlightPlan GetFlightPlanByIdOutput
        {
            get
            {
                return _getFlightPlanByIdOutput;
            }
        }

        public List<Server> GetServersOutput
        {
            get
            {
                return _getServersOutput;
            }
        }

        public Response PostServerOutput
        {
            get
            {
                return _postServerOutput;
            }
        }

        public Response DeleteServerOutput
        {
            get
            {
                return _deleteServerOutput;
            }
        }

        public Response DeleteFlightByIdOutput
        {
            get
            {
                return _deleteFlightByIdOutput;
            }
        }

        public Response PostFlightPlanOutput
        {
            get
            {
                return _postFlightPlanOutput;
            }
        }

        public List<Flight> GetInternalFlightsByTimeOutput
        {
            get
            {
                return _getInternalFlightsOutput;
            }
        }

        public List<Flight> GetAllFlightsOutput
        {
            get
            {
                return _getAllFlightsOutput;
            }
        }


        public MockFcwRepo()
        {
            SetMockOutputs();
        }

        /*
         * Function: GetFlightsByTimeAsync
         * Description: This method sends an async request to an external servers, and asks for
                        the relevant flights for `relative_to`, and then it also gets the relevant
         *              flights.
         */
        public async Task<IEnumerable<Flight>> GetFlightsByTimeAsync(string date, bool isExternal)
        {
            if (isExternal)
            {
                return GetAllFlightsOutput;
            }
            return GetInternalFlightsByTimeOutput;
        }

        /*
         * Function: DeleteFlightById
         * Description: Deletes a flight and all its segments
         */
        public Response DeleteFlightById(string id)
        {
            return DeleteFlightByIdOutput;
        }

        /*
        * Function: PostFlightPlan
        * Description: Posts a flight plan
        */
        public Response PostFlightPlan(FlightPlan flightPlan)
        {
            return PostFlightPlanOutput;
        }

        /*
         * Function: GetFlightPlanByIdAsync
         * Description: This asynchronous method checks if the flight ID is present in the DB, and
         *              if not, it sends an async request to the external servers.
         */
        public async Task<FlightPlan> GetFlightPlanByIdAsync(string id)
        {
            return GetFlightPlanByIdOutput;
        }

        /*
         * Function: PostServer
         * Description: Posts a new server
         */
        public Response PostServer(Server server)
        {
            return PostServerOutput;
        }

        /*
         * Function: DeleteServer
         * Description: Deletes a server by ID
         */
        public Response DeleteServer(string id)
        {
            return DeleteServerOutput;
        }

        /*
         * Function: GetAllServers
         * Description: Returns all the servers.
         */
        public IEnumerable<Server> GetAllServers()
        {
            return GetServersOutput;
        }

        private void SetMockOutputs()
        {
            SetGetFlightPlanOutput();
            SetServersOutput();
            SetPostServerOutput();
            SetDeleteFlightByIdOutput();
            SetDeleteServerOutput();
            SetPostFlightPlanOutput();
            SetGetFlightsOutput();
        }

        private void SetGetFlightPlanOutput()
        {
            InitialLocation initialLocation = new InitialLocation(34, 34, "2020-05-30T08:30:00Z");

            var segments = new List<Segment>{
                new Segment(35, 35, 100),
                new Segment(36, 36, 20)
            };

            _getFlightPlanByIdOutput = new FlightPlan(222, "External Deafult Airlines", initialLocation, segments);
        }

        private void SetServersOutput()
        {
            var servers = new List<Server>{
                new Server("server1", "google.com"),
                new Server("server2", "ebay.com"),
                new Server("server3", "meow.cat")
            };

            _getServersOutput = servers;
        }

        public void SetPostServerOutput()
        {
            _postServerOutput = new Response("POST", true, "Server added");
        }

        public void SetDeleteServerOutput()
        {
            _deleteServerOutput = new Response("DELETE", true, "Server deleted");
        }

        public void SetDeleteFlightByIdOutput()
        {
            _deleteFlightByIdOutput = new Response("DELETE", true, "The flight has been deleted");
        }

        public void SetPostFlightPlanOutput()
        {
            _postFlightPlanOutput = new Response("POST", true, "The flight plan has been added");
        }

        public void SetGetFlightsOutput()
        {
            _getInternalFlightsOutput = new List<Flight>{
                new Flight("FOJI88", 38, -38, 1000, "Air France",
                           new MyDateTime("2020-05-30T08:00:00Z"), false),
                new Flight("POLA11", 22.1, -23.8, 1, "Swiss Air",
                           new MyDateTime("2020-05-30T08:00:00Z"), false),
            };

            _getAllFlightsOutput = new List<Flight>(_getInternalFlightsOutput);
            _getAllFlightsOutput.Add(new Flight("KOKO68", 22.1, -23.8, 1, "Turkish Airlines",
                                    new MyDateTime("2020-05-30T08:00:00Z"), true));

        }
    }
}