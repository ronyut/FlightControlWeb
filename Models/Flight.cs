using System;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace FlightControlWeb.Models
{
    public class Flight
    {
        public static readonly int CoordRound = 6;
        // From DB
        [JsonIgnore]
        public int flightPk { get; set; }

        [JsonProperty("flight_id")]
        public string flightId { get; set; }

        [JsonProperty("company_name")]
        public string company { get; set; }
        public int passengers { get; set; }

        [JsonProperty("is_external")]
        public bool isExternal { get; set; }

        [JsonProperty("date_time")]
        public string dateTime { get; set; }
        [JsonIgnore]
        public MyDateTime takeoff { get; set; }
        [JsonIgnore]
        public MyDateTime landing { get; set; }
        [JsonIgnore]
        public Coordinate initialLocation { get; set; }
        
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
        private SqliteConnection _conn { get; set; }
        
        [JsonIgnore]
        private SqliteDataReader _reader { get; set; }

        [JsonConstructor]
        public Flight()
        {

        }

        public Flight(string flight_id, double longitude, double latitude, int passengers,
                      string company, MyDateTime dateTime, bool isExternal)
        {
            this.flightId = flight_id;
            this.longitude = longitude;
            this.latitude = latitude;
            this.passengers = passengers;
            this.company = company;
            this.dateTime = dateTime.iso;
            this.isExternal = isExternal;
        }

        public Flight(SqliteDataReader reader, SqliteConnection connection, MyDateTime relativeTo)
        {
            this._conn = connection;
            this._reader = reader;
            this.relativeTo = relativeTo;
            this.dateTime = relativeTo.iso;

            this.flightPk = (int)(long) At("flight_id");
            this.flightId = (string) At("flight_name");
            this.company = (string) At("company");
            this.passengers = (int)(long) At("passengers");
            this.takeoff = new MyDateTime((string) At("takeoff"));
            this.landing = new MyDateTime((int)(long) At("landing_unix"));
            this.initialLocation = new Coordinate(At("longitude"), At("latitude"));
            this.isExternal = Util.IntToBool((int)(long) At("is_external"));

            Calculate();
        }

        public object At(string column)
        {
            return At(column, this._reader);
        }

        public object At(string column, SqliteDataReader reader)
        {
            return reader.GetValue(reader.GetOrdinal(column));
        }

        public void Calculate()
        {
            var timePassed = relativeTo.unix - takeoff.unix;

            var cmd = _conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM segments WHERE flight_id = "+ this.flightPk + " AND ";
            cmd.CommandText += "timespan_cdf >= "+ timePassed +" ORDER BY timespan_cdf ASC LIMIT 1";
            using(var reader = cmd.ExecuteReader())
            {
                while(reader.Read())
                {
                    var segOrder = (int)(long) At("seg_order", reader);
                    var segDest = new Coordinate(At("seg_longitude", reader),
                                                 At("seg_latitude", reader));
                    var segTimespan = (double)(long) At("timespan", reader);
                    var sefTimespanCdf = (int)(long) At("timespan_cdf", reader);

                    var currentLocation = new Coordinate(0, 0);
                    var startCoord = GetSegmentStart(segOrder - 1);
                    double timeFrac = (segTimespan - (sefTimespanCdf - timePassed)) / segTimespan;

                    double distLong = (segDest.longitude - startCoord.longitude) * timeFrac;
                    currentLocation.longitude = startCoord.longitude + distLong;

                    double distLat = (segDest.latitude - startCoord.latitude) * timeFrac;
                    currentLocation.latitude = startCoord.latitude + distLat;
                    
                    // update properties
                    this.longitude = Math.Round(currentLocation.longitude, CoordRound);
                    this.latitude = Math.Round(currentLocation.latitude, CoordRound);
                    // Calculate angle
                    this.angle = CalculateAngle(startCoord, currentLocation);
                    // Calculate time left to landing
                    this.ETL = this.landing.unix - this.takeoff.unix - timePassed;
                }
            }
        }

        public Coordinate GetSegmentStart(int segOrder)
        {
            Coordinate coord = null;

            // If this is the 1st segment, then its start coord is the flight plan's initial coord
            if (segOrder == 0)
            {
                return this.initialLocation;
            }

            var cmd = _conn.CreateCommand();
            cmd.CommandText  = @"SELECT * FROM segments
                                  WHERE flight_id = "+ this.flightPk +
                                " AND seg_order = "+ segOrder +" LIMIT 1";
            using(var reader = cmd.ExecuteReader())
            {
                while(reader.Read())
                {
                    coord = new Coordinate(At("seg_longitude", reader), At("seg_latitude", reader));
                }
            }
            
            // Check for possible error
            if (coord == null)
            {
                throw new Exception("Segment #"+ segOrder +" not found for flight "+ flightId);
            }

            return coord;
        }

        public double CalculateAngle(Coordinate from, Coordinate to)
        {
            double angle;
            var xDist = to.longitude - from.longitude;
            var yDist = to.latitude - from.latitude;

            if (xDist == 0) {
                if (yDist > 0) {
                    angle = 90;
                } else {
                    angle = 180;
                }
            } else {
                angle = Math.Atan(yDist / xDist);
                var add = 0;

                if (xDist > 0 && yDist > 0)
                {
                    add = 0;
                }
                else if (xDist > 0 && yDist < 0)
                {
                    add = 360;
                }
                else if (xDist < 0 && yDist > 0)
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