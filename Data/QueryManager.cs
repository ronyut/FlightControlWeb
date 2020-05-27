using System;
using System.Collections.Generic;
using FlightControlWeb.Models;
using Microsoft.Data.Sqlite;

namespace FlightControlWeb.Data
{
    public class QueryManager
    {
        private SqliteConnection _conn { get; set; }

        public QueryManager(SqliteConnection conn)
        {
            this._conn = conn;
        }

        public FlightPlan GetFlightPlanById(string id)
        {
            FlightPlan flightPlan = null;

            // Connection Opened //
            _conn.Open();
            
                var cmd = _conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM flights WHERE flight_name = '" + id + "' LIMIT 1";
                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var flightPk = (int)(long) At("flight_id", reader);
                        var passengers = (int)(long) At("passengers", reader);
                        var company = (string) At("company", reader);
                        var coord = new Coordinate(At("longitude", reader), At("latitude", reader));
                        var initialLocation = new InitialLocation(coord,
                                                                  (string) At("takeoff", reader));
                        var segments = GetSegmentsByFlightPk(flightPk);

                        flightPlan = new FlightPlan(passengers, company, initialLocation, segments);
                    }
                }
            
            // Connection Closed //
            _conn.Close();
            
            return flightPlan;
        }

        public IEnumerable<Segment> GetSegmentsByFlightPk(int id)
        {
            var segments = new List<Segment>{};

            var cmd = _conn.CreateCommand();
            cmd.CommandText  = @"SELECT * FROM segments
                                 WHERE flight_id = '" + id + "'" +
                                "ORDER BY seg_id ASC";
            using(var reader = cmd.ExecuteReader())
            {
                while(reader.Read())
                {
                    var coord = new Coordinate(At("seg_longitude", reader),
                                               At("seg_latitude", reader));
                    var segment = new Segment(coord, (int)(long) At("timespan", reader));
                    segments.Add(segment);
                }
            }

            return segments;
        }

        public object At(string column, SqliteDataReader reader)
        {
            return reader.GetValue(reader.GetOrdinal(column));
        }

        public IEnumerable<Flight> GetFlightsByTime(MyDateTime relativeTo, bool isExternal)
        {
            var flights = new List<Flight> {};

            // Connection Opened //
            _conn.Open();
            
                var cmd = _conn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM flights 
                                    WHERE takeoff_unix <= " + relativeTo.unix +
                                   " AND landing_unix >= " + relativeTo.unix;

                if (!isExternal)
                {
                    cmd.CommandText += " AND is_external = 0";
                }

                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        // @ Logic is very bad - we should not pass reader to Flight
                        var flight = new Flight(reader, _conn, relativeTo);
                        flights.Add(flight);
                    }
                }
            
            // Connection Closed //
            _conn.Close(); 

            return flights;
        }

        public void DeleteFlightById(string id)
        {
            // Connection Opened //
            _conn.Open();
            
                var cmd = _conn.CreateCommand();
                cmd.CommandText  = "DELETE FROM flights WHERE flight_name = '" + id + "'";
                var affectedRowsNum = cmd.ExecuteNonQuery();
                if (affectedRowsNum != 1)
                {
                    throw new Exception("Flight ID does not exist");
                }
            
            // Connection Closed //
            _conn.Close();
        }

        public void PostFlightPlan(FlightPlan flightPlan)
        {
            // Check that all values are valid 
            // @ flightPlan.Validate();

            // Calculate total flight span
            var totalTimespan = 0;
            foreach(Segment segment in flightPlan.segments)
            {
                totalTimespan += segment.timespan;
            }

            var takeoff = new MyDateTime(flightPlan.initialLocation.dateTime);
            var landing = new MyDateTime((takeoff.unix + totalTimespan));

            // Connection Opened //
            _conn.Open();
           
                var fp = flightPlan;
                using(var transaction = _conn.BeginTransaction())
                {
                    var cmd = _conn.CreateCommand();
                    cmd.CommandText = @"INSERT INTO flights (flight_name, company, passengers,
                                        longitude, latitude, takeoff, takeoff_unix, landing_unix)
                                        VALUES ('"+ fp.flightId +"', '"+ fp.company +
                                        "', "+ fp.passengers +", "+ fp.initialLocation.longitude +
                                        ", "+ fp.initialLocation.latitude +", '"+ takeoff.sql +
                                        "', "+ takeoff.unix +", "+ landing.unix +");";
                    cmd.CommandText += "SELECT last_insert_rowid()";
                    var flightPk = (long) cmd.ExecuteScalar();

                    // Handle the segments
                    cmd = _conn.CreateCommand();
                    int index = 1, cdf = 0;
                    foreach(Segment seg in flightPlan.segments)
                    {
                        cdf += seg.timespan;
                        cmd.CommandText += @"INSERT INTO segments (seg_order, seg_longitude,
                                            seg_latitude, timespan, timespan_cdf, flight_id)
                                            VALUES ("+ index +", "+ seg.longitude +
                                            ", "+ seg.latitude +", "+ seg.timespan +", "+ cdf +
                                            ", "+ flightPk +");";
                        index++;
                    }
                    var affectedRowsNum = cmd.ExecuteNonQuery();
                    if (affectedRowsNum != index - 1)
                    {
                        throw new Exception("Could not post all segments to DB");
                    }
                    transaction.Commit();
                }

            // Connection Closed //
            _conn.Close();
        }
    }
}