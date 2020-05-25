using System;
using System.Collections.Generic;
using FlightControlWeb.Models;
using Microsoft.Data.Sqlite;

namespace FlightControlWeb.Data
{
    public class SqliteFcwRepo : IFcwRepo
    {
        private readonly FcwContext _context;

        public SqliteFcwRepo(FcwContext context)
        {
            _context = context;
        }

        public object At(string column, SqliteDataReader reader)
        {
            return reader.GetValue(reader.GetOrdinal(column));
        }

        public IEnumerable<FlightPlan> GetAllFlightPlans()
        {
            throw new Exception("Not implemented!");
        }

        public FlightPlan GetFlightPlanById(string id)
        {
            FlightPlan flightPlan = null;
            using(var connection = _context.conn)
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM flights WHERE flight_name = '" + id + "' LIMIT 1";
                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var flightPk = (int)(long) At("flight_id", reader);
                        var passengers = (int)(long) At("passengers", reader);
                        var company = (string) At("company", reader);
                        var coord = new Coordinate(At("longitude", reader), At("latitude", reader));
                        var initialLocation = new InitialLocation(coord, (string) At("takeoff", reader));
                        var segments = GetSegmentsByFlightPk(flightPk, connection);
                        flightPlan = new FlightPlan(passengers, company, initialLocation, segments);
                    }
                }
            }
            return flightPlan;
        }

        public IEnumerable<Flight> GetFlightsByTime(string date, bool isExternal)
        {
            var relativeTo = new MyDateTime(date);
            var flights = new List<Flight> {};

            using(var connection = _context.conn)
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM flights WHERE takeoff_unix <= " + relativeTo.unix + " AND landing_unix >= " + relativeTo.unix;
                if (!isExternal)
                {
                    cmd.CommandText += " AND is_external = 0";
                }

                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var flight = new Flight(reader, connection, relativeTo);
                        flights.Add(flight);
                    }
                }
            }
            return flights;
        }

        public IEnumerable<Segment> GetSegmentsByFlightPk(int id, SqliteConnection connection)
        {
            var segments = new List<Segment>{};

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM segments WHERE flight_id = '" + id + "' ORDER BY seg_id ASC";
            using(var reader = cmd.ExecuteReader())
            {
                while(reader.Read())
                {
                    var coord = new Coordinate(At("seg_longitude", reader), At("seg_latitude", reader));
                    var segment = new Segment(coord, (int)(long) At("timespan", reader));
                    segments.Add(segment);
                }
            }
            return segments;
        }

        public Response DeleteFlightById(string id)
        {
            var affectedRowsNum = 0;
            var request = "DELETE";
            using(var connection = _context.conn)
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText  = "DELETE FROM flights WHERE flight_name = '" + id + "'";
                affectedRowsNum = cmd.ExecuteNonQuery();
            }

            if (affectedRowsNum != 1)
            {
                return new Response(request, false, "Flight ID does not exist");
            }
            else
            {
                return new Response(request, true, "The flight has been successfully deleted");
            }

            
        }

        public Response PostFlightPlan(string json)
        {
            Console.WriteLine(json);
            return new Response("POST", false, "NaN");
        }
    }
}