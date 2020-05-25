using System;
using System.Text.Json.Serialization;
using FlightControlWeb.Data;
using Microsoft.Data.Sqlite;

namespace FlightControlWeb.Models
{
    public class FlightCalcData
    {
        public static readonly int COORD_ROUND = 6;
        // From DB
        public int flightPK { get; set; }
        public string flight_id { get; set; }
        public string company_name { get; set; }
        public int passengers { get; set; }
        [JsonIgnore]
        public MyDateTime takeoff { get; set; }
        [JsonIgnore]
        public MyDateTime landing { get; set; }
        [JsonIgnore]
        public Coordinate initial_location { get; set; }
        
        // Calculated
        public double longitude { get; set; }
        public double latitude { get; set; }
        [JsonIgnore]
        public double angle { get; set; }
        [JsonIgnore]
        public int ETL { get; set; }
        
        // Additional
        [JsonIgnore]
        public MyDateTime relativeTo { get; set; }
        [JsonIgnore]
        public SqliteConnection _conn { get; set; }
        [JsonIgnore]
        public SqliteDataReader _reader { get; set; }

        public FlightCalcData(SqliteDataReader reader, SqliteConnection connection, MyDateTime relativeTo)
        {
            this._conn = connection;
            this._reader = reader;
            this.relativeTo = relativeTo;

            this.flightPK = Int32.Parse(at("flight_id"));
            this.flight_id = at("flight_name");
            this.company_name = at("company");
            this.passengers = Int32.Parse(at("passengers"));
            this.takeoff = new MyDateTime(at("takeoff"));
            this.landing = new MyDateTime(Int32.Parse(at("landing_unix")));
            this.initial_location = new Coordinate(at("longitude"), at("latitude"));

            Calculate();
        }

        public string at(string column)
        {
            return at(column, this._reader);
        }

        public string at(string column, SqliteDataReader reader)
        {
            return reader.GetString(reader.GetOrdinal(column));
        } 

        public void Calculate()
        {
            var time_passed = relativeTo.unix - takeoff.unix;

            var cmd = _conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM segments WHERE flight_id = "+ this.flightPK +" AND timespan_cdf >= "+ time_passed +" ORDER BY timespan_cdf ASC LIMIT 1";
            using(var reader = cmd.ExecuteReader())
            {
                while(reader.Read())
                {
                    var segment_order = Int32.Parse(at("seg_order", reader));
                    var seg_destination = new Coordinate(at("seg_longitude", reader), at("seg_latitude", reader));
                    var segment_timespan = Int32.Parse(at("timespan", reader));
                    var segment_timespan_cdf = Int32.Parse(at("timespan_cdf", reader));

                    var currentLocation = new Coordinate(0, 0);
                    var startCoord = GetSegmentStart(segment_order - 1);
                    double timeFraction = (double) (segment_timespan - (segment_timespan_cdf - time_passed)) / (double) segment_timespan;

                    double distance_longitude = (seg_destination.longitude - startCoord.longitude) * timeFraction;
                    currentLocation.longitude = startCoord.longitude + distance_longitude;

                    double distance_latitude = (seg_destination.latitude - startCoord.latitude) * timeFraction;
                    currentLocation.latitude = startCoord.latitude + distance_latitude;
                    
                    // update properties
                    this.longitude = Math.Round(currentLocation.longitude, COORD_ROUND);
                    this.latitude = Math.Round(currentLocation.latitude, COORD_ROUND);
                    /*Console.WriteLine("Current: " + this.longitude + ", " + this.latitude);
                    Console.WriteLine("Distance: " + distance_longitude + ", " + distance_latitude);
                    Console.WriteLine("Start: " + startCoord.longitude + ", " + startCoord.latitude);
                    Console.WriteLine("Destination: " + seg_destination.longitude + ", " + seg_destination.latitude);
                    Console.WriteLine("Time passed: " + time_passed);
                    Console.WriteLine("Timespan: " + segment_timespan);
                    Console.WriteLine("Timespan CDF: " + segment_timespan_cdf);
                    Console.WriteLine("Calc1: " + (seg_destination.longitude - startCoord.longitude));
                    Console.WriteLine("Calc2: " + (seg_destination.latitude - startCoord.latitude));
                    Console.WriteLine("Calc3: " + ((segment_timespan - (segment_timespan_cdf - time_passed)) / segment_timespan));
                    Console.WriteLine("--------------------------------------------------------------");*/
                    // Calculate angle
                    this.angle = CalculateAngle(startCoord, currentLocation);
                    // Calculate time left to landing
                    this.ETL = this.landing.unix - this.takeoff.unix - time_passed;
                }
            }
        }

        public Coordinate GetSegmentStart(int segment_order)
        {
            Coordinate coord = null;
            if (segment_order > 0)
            {
                var cmd = _conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM segments WHERE flight_id = "+ this.flightPK +" AND seg_order = "+ segment_order +" LIMIT 1";
                using(var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        coord = new Coordinate(at("seg_longitude", reader), at("seg_latitude", reader));
                    }
                }
            }
            else
            {
                coord = this.initial_location;
            }

            // Check for possible error
            if (coord == null)
            {
                throw new Exception("No segments found for this flight!");
            }

            return coord;
        }

        public double CalculateAngle(Coordinate from, Coordinate to)
        {
            double angle;
            var x_dist = to.longitude - from.longitude;
            var y_dist = to.latitude - from.latitude;

            if (x_dist == 0) {
                if (y_dist > 0) {
                    angle = 90;
                } else {
                    angle = 180;
                }
            } else {
                angle = Math.Atan(y_dist / x_dist);
                var add = 0;

                if (x_dist > 0 && y_dist > 0)
                {
                    add = 0;
                }
                else if (x_dist > 0 && y_dist < 0)
                {
                    add = 360;
                }
                else if (x_dist < 0 && y_dist > 0)
                {
                    add = 180;
                }
                else
                {
                    add = 180;
                }

                angle = angle * (180 / Math.PI) + add;
            }

            return angle;
        }
    }
}