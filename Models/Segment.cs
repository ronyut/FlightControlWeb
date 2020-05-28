/* This class represents a segment that hold its destination coord and the timespan.
 * 
 * Author: Rony Utesvky.
 * Date: May 28, 2020
 */

using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace FlightControlWeb.Models
{
    public class Segment
    {
        [Required]
        public double longitude { get; set; }
        
        [Required]
        public double latitude { get; set; }
        
        [Required]
        [JsonProperty("timespan_seconds")]
        public int timespan { get; set; }

        /*
         * Ctor
         */
        [JsonConstructor]
        public Segment(double longitude, double latitude, int timespan)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.timespan = timespan;
        }

        /*
         * Ctor
         */
        public Segment(Coordinate coord, int timespan)
        {
            this.longitude = coord.longitude;
            this.latitude = coord.latitude;
            this.timespan = timespan;
        }

        /*
         * Function: ToString
         * Description: Generates a nice string representing the segment.
         */
        override
        public string ToString()
        {
            return "(" + this.latitude + ", " + this.longitude + ") -> " + this.timespan;
        }

        /*
         * Function: Validate
         * Description: Validates the field values of this class.
         */
        public void Validate()
        {
            var coord = new Coordinate(longitude, latitude);
            coord.Validate();

            if (timespan <= 0)
            {
                throw new Exception("Segment timespan must be positive");
            }
        }
    }
}