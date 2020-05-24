using System;
using System.Collections.Generic;
using System.Linq;
using FlightControlWeb.Models;

namespace FlightControlWeb.Data
{
    public class SqliteFcwRepo : IFcwRepo
    {
        private readonly FcwContext _context;

        public SqliteFcwRepo(FcwContext context)
        {
            _context = context;
        }

        public IEnumerable<FlightPlan> GetAllFlightPlans()
        {
            //return _context.FlightPlans.ToList();
            using(var connection = _context._conn)
            {
                connection.Open();

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = "CREATE table beers(name VARCHAR(50))";
                tableCmd.ExecuteNonQuery();
            }

            IEnumerable<Segment> segments = new List<Segment>{
                new Segment(35, 35, 100),
                new Segment(36, 36, 20)
            };

            InitialLocation initial_location = new InitialLocation(34, 34, "2020-05-22T15:30:00Z");

            var flightPlans = new List<FlightPlan>
            {
                new FlightPlan(0, 222, "El Al 0", initial_location, segments),
                new FlightPlan(1, 333, "El Al 1", initial_location, segments),
                new FlightPlan(2, 444, "El Al 2", initial_location, segments)
            };

            return flightPlans;

        }

        public FlightPlan GetFlightPlanById(int id)
        {
            //return _context.FlightPlans.FirstOrDefault(p => p.flight_id == id);
            throw new Exception("Err meow!");
        }

    }
}