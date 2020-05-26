using System;
using System.Collections.Generic;
using FlightControlWeb.Models;
using Microsoft.Data.Sqlite;

namespace FlightControlWeb.Data
{
    public class SqliteFcwRepo : IFcwRepo
    {
        private readonly FcwContext _context;
        private QueryManager _queryManager;

        public SqliteFcwRepo(FcwContext context)
        {
            _context = context;
            _queryManager = new QueryManager(context.conn);

        }

        public IEnumerable<FlightPlan> GetAllFlightPlans()
        {
            throw new Exception("Not implemented!");
        }

        public FlightPlan GetFlightPlanById(string id)
        {
            var flightPlan = _queryManager.GetFlightPlanById(id);
            if (flightPlan != null)
            {
                return flightPlan;
            }

            try
            {
                //flightPlan = _queryManager.GetExternalFlightPlanById(id);
            }
            catch(Exception e)
            {
                
            }

            return flightPlan;
        }

        public IEnumerable<Flight> GetFlightsByTime(string date, bool isExternal)
        {
            var relativeTo = new MyDateTime(date);
            var flights = new List<Flight> {};

            // Get flights from external servers
            // @.......

            flights.AddRange(_queryManager.GetFlightsByTime(relativeTo, isExternal));
            return flights;
        }

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