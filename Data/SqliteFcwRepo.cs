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
            throw new Exception("Not implemented!");
        }

        public FlightPlan GetFlightPlanById(int id)
        {
            throw new Exception("Not implemented!");
        }

        public IEnumerable<Flight> GetFlightsByTime(string date, bool isExternal)
        {
            var relativeTo = new MyDateTime(date);
            var flights = new List<Flight> {};

            using(var connection = _context._conn)
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM flights WHERE takeoff_unix <= " + relativeTo.unix + " AND landing_unix >= " + relativeTo.unix + " AND is_external = " + isExternal;
                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var fd = new FlightCalcData(reader, connection, relativeTo);
                        var flight = new Flight(fd.flight_id, fd.longitude, fd.latitude, fd.passengers, fd.company_name, relativeTo, isExternal);
                        flights.Add(flight);
                    }
                }
            }
            return flights;
        }

    }
}