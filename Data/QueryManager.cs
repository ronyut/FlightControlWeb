/* This class manages all the DB queries.
 * 
 * Author: Rony Utesvky.
 * Date: May 28, 2020
 */

using System;
using System.Collections.Generic;
using FlightControlWeb.Models;
using Microsoft.Data.Sqlite;

namespace FlightControlWeb.Data
{
    public class QueryManager
    {
        private SqliteConnection _conn { get; set; }

        /*
         * Ctor
         */
        public QueryManager(SqliteConnection conn)
        {
            this._conn = conn;
        }

        /*
         * Function: At
         * Description: Get the object from the DB by the column name.
         */
        public object At(string column, SqliteDataReader reader)
        {
            return reader.GetValue(reader.GetOrdinal(column));
        }

        /*
         * Function: GetFlightPlanById
         * Description: Gets the FlightPlan from DB by flight ID.
         */
        public FlightPlan GetFlightPlanById(string id)
        {
            FlightPlan flightPlan = null;

            // Connection Opened //
            _conn.Open();
            
                var cmd = _conn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM flights
                                    WHERE flight_name = '" + id + "' LIMIT 1";
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

        /*
         * Function: GetSegmentsByFlightPk
         * Description: Gets all the segments of a flight by the flight PK.
         */
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

        /*
         * Function: GetFlightsByTime
         * Description: Gets all the flight in DB that are active during `relative_to` time.
         */
        public IEnumerable<Flight> GetFlightsByTime(MyDateTime relativeTo)
        {
            var flights = new List<Flight> {};

            // Connection Opened //
            _conn.Open();
            
                var cmd = _conn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM flights 
                                    WHERE takeoff_unix <= " + relativeTo.unix +
                                   " AND landing_unix >= " + relativeTo.unix +
                                   " AND is_external = 0";
                

                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        // Logic seems problematic - passing reader and connection to Flight
                        var flight = new Flight(reader, _conn, relativeTo);
                        flights.Add(flight);
                    }
                }
            
            // Connection Closed //
            _conn.Close(); 

            return flights;
        }

        /*
         * Function: DeleteFlightById
         * Description: Deletes a flight by ID.
         */
        public void DeleteFlightById(string id)
        {
            // Connection Opened //
            _conn.Open();
            
                var cmd = _conn.CreateCommand();
                cmd.CommandText  = @"DELETE FROM flights WHERE flight_name = '" + id + "' "+
                                    "AND is_external = 0";
                var affectedRowsNum = cmd.ExecuteNonQuery();
                if (affectedRowsNum != 1)
                {
                    throw new Exception("Flight ID does not exist");
                }
            
            // Connection Closed //
            _conn.Close();
        }

        /*
         * Function: PostFlightPlan
         * Description: Posts a new flight plan.
         * Default: non-external flight
         */
        public void PostFlightPlan(FlightPlan flightPlan)
        {
            PostFlightPlan(flightPlan, false, null);
        }

        /*
         * Function: PostFlightPlan
         * Description: Posts a new flight plan.
         */
        public void PostFlightPlan(FlightPlan flightPlan, bool isExternal, string id)
        {
            if (isExternal)
            {
                if (id == null || id == "") throw new Exception("External flight must have ID");
                flightPlan.flightId = id;
            }
            
            // Check that all field values are valid 
            flightPlan.Validate();

            // Calculate total flight span
            var totalTimespan = 0;
            foreach (var segment in flightPlan.segments)
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
                                        longitude, latitude, takeoff, takeoff_unix, landing_unix,
                                        is_external)
                                        VALUES ('"+ fp.flightId +"', '"+ fp.company +
                                        "', "+ fp.passengers +", "+ fp.initialLocation.longitude +
                                        ", "+ fp.initialLocation.latitude +", '"+ takeoff.sql +
                                        "', "+ takeoff.unix +", "+ landing.unix +
                                        ", "+ Util.BoolToInt(isExternal) +");";
                    cmd.CommandText += "SELECT last_insert_rowid()";
                    var flightPk = (long) cmd.ExecuteScalar();

                    // Handle the segments
                    cmd = _conn.CreateCommand();
                    int index = 1, cdf = 0;
                    foreach (var seg in flightPlan.segments)
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
                    if (affectedRowsNum != --index)
                    {
                        throw new Exception("Could not post all segments to DB");
                    }
                    transaction.Commit();
                }

            // Connection Closed //
            _conn.Close();
        }

        /*
         * Function: IsIgnoredFlight
         * Description: Check if a searching this flight ID should be ignored if requested by
                        other servers.
         */
        public bool IsIgnoredFlight(string id)
        {
            bool isIgnored;

            // Connection Opened //
            _conn.Open();
            
                var cmd = _conn.CreateCommand();
                cmd.CommandText = @"SELECT count(*) FROM flights_ignored
                                    WHERE flight_name = '"+ id +"'";
                isIgnored = Util.IntToBool((int)(long) cmd.ExecuteScalar());

            // Connection Closed //
            _conn.Close(); 

            return isIgnored;
        }

        /*
         * Function: SetFlightIgnored
         * Description: Set a flight to be ignored/not-ignored if requseted by other servers.
         */
        public void SetFlightIgnored(string id, bool ignore)
        {
            var cmdText = "DELETE FROM flights_ignored WHERE flight_name = '"+ id +"'";
            if (ignore)
            {
                cmdText = "INSERT INTO flights_ignored (flight_name) VALUES('"+ id +"')";
            }

            // Connection Opened //
            _conn.Open();
            
                var cmd = _conn.CreateCommand();
                cmd.CommandText = cmdText;
                cmd.ExecuteNonQuery();

            // Connection Closed //
            _conn.Close(); 
        }

        /*
         * Function: GetAllServers
         * Description: Returns all external servers in DB.
         */
        public IEnumerable<Server> GetAllServers()
        {
            var servers = new List<Server>{};

            // Connection Opened //
            _conn.Open();
            
                var cmd = _conn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM servers WHERE is_enabled = 1";
                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var key = (string) At("server_key", reader);
                        var url = (string) At("server_url", reader);
                        var server = new Server(key, url);
                        servers.Add(server);
                    }
                }

            // Connection Closed //
            _conn.Close();

            return servers;
        }

        /*
         * Function: DeleteServer
         * Description: Deletes a server by ID from the DB.
         */
        public void DeleteServer(string id)
        {
            // Connection Opened //
            _conn.Open();
            
                var cmd = _conn.CreateCommand();
                cmd.CommandText  = @"DELETE FROM servers WHERE server_key = '" + id + "'";
                var affectedRowsNum = cmd.ExecuteNonQuery();
                if (affectedRowsNum != 1)
                {
                    throw new Exception("Server does not exist in DB");
                }
            
            // Connection Closed //
            _conn.Close();
        }

        /*
         * Function: PostServer
         * Description: Posts a new server to DB
         */
        public void PostServer(Server server)
        {
            // Connection Opened //
            _conn.Open();
            
                var cmd = _conn.CreateCommand();
                cmd.CommandText  = @"INSERT INTO servers (server_key, server_url)
                                     VALUES('"+ server.key +"', '"+ server.url +"')";
                var affectedRowsNum = cmd.ExecuteNonQuery();
                if (affectedRowsNum != 1)
                {
                    throw new Exception("Could not add server");
                }
            
            // Connection Closed //
            _conn.Close();
        }
    }
}