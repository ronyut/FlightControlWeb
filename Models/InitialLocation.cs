/* This class represents a flight's initial location.
 * 
 * Author: Rony Utesvky.
 * Date: May 28, 2020
 */

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace FlightControlWeb.Models
{
    public class InitialLocation
    {
        [Required]
        public double longitude { get; set; }

        [Required]
        public double latitude { get; set; }
        
        [Required]
        [JsonProperty("date_time")]
        public string dateTime { get; set; }

        /*
         * Ctor
         */
        [JsonConstructor]
        public InitialLocation (double longitude, double latitude, string dateTime)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.dateTime = new MyDateTime(dateTime).iso;
        }

        /*
         * Ctor
         */
        public InitialLocation (Coordinate coord, string dateTime)
        {
            this.longitude = coord.longitude;
            this.latitude = coord.latitude;
            this.dateTime = new MyDateTime(dateTime).iso;
        }

        /*
         * Function: ToString
         * Description: Generates a nice string representing the initial location.
         */
        override
        public string ToString()
        {
            return "(" + this.latitude + ", " + this.longitude + ") -> " + this.dateTime;
        }

        /*
         * Function: Validate
         * Description: Validates the field values of this class.
         */
        public void Validate()
        {
            var initCoord = new Coordinate(longitude, latitude);
            initCoord.Validate();
            
            // Will throw excpetion if not valid
            var date = new MyDateTime(dateTime);
        }
    }
}