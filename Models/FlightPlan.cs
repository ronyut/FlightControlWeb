/* This class represents a flight plan.
 * 
 * Author: Rony Utesvky.
 * Date: May 28, 2020
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace FlightControlWeb.Models
{
    public class FlightPlan
    {
        [JsonIgnore]
        public string flightId { get; set; }

        [Required]
        public int passengers { get; set; }
        
        [Required]
        [JsonProperty("company_name")]
        public string company { get; set; }
        
        [Required]
        [JsonProperty("initial_location")]
        public InitialLocation initialLocation { get; set; }

        [Required]
        public IEnumerable<Segment> segments { get; set; }

        /*
         * Ctor
         */
        public FlightPlan (int passengers, string company, InitialLocation initialLocation,
                           IEnumerable<Segment> segments)
        {
            this.flightId = GenerateFlightID();
            this.passengers = passengers;
            this.company = company;
            this.initialLocation = initialLocation;
            this.segments = segments;
        }

        /*
         * Function: GetRandomConsonant
         * Description: Returns a radnom consonant
         */
        public static string GetRandomConsonant()
        {
            return GetRandomChar("BCDFGHJKLMNPQRSTVWXYZ");
        }

        /*
         * Function: GetRandomVowel
         * Description: Returns a radnom vowel
         */
        public static string GetRandomVowel()
        {
            return GetRandomChar("AEIOU");
        }

        /*
         * Function: GetRandomDigit
         * Description: Returns a radnom digit char
         */
        public static string GetRandomDigit()
        {
            return GetRandomChar("0123456789");
        }

        /*
         * Function: GetRandomChar
         * Description: Returns a radnom char from string
         */
        public static string GetRandomChar(string chars)
        {
            var rand = new Random();
            int num = rand.Next(0, chars.Length -1);
            return chars[num] + "";
        }

        /*
         * Function: GenerateFlightID
         * Description: Generates a random readable flight ID
         */
        public static string GenerateFlightID()
        {
            return GetRandomConsonant() + GetRandomVowel() + GetRandomConsonant() +
                   GetRandomVowel() + "-" + GetRandomDigit() + GetRandomDigit();
        }

        /*
         * Function: ToString
         * Description: Generates a nice string representing the flight plan's data.
         */
        override
        public string ToString()
        {
            var output = "";
            output += "Flight ID:" + this.flightId + "\n";
            output += "Company:" + this.company + "\n";
            output += "Passengers:" + this.passengers + "\n";
            output += "Initial location:" + this.initialLocation + "\n";
            output += "Segments:\n";
            
            foreach (var segment in this.segments)
            {
                output += "\t" + segment.ToString();
            }

            return output;
        }

        /*
         * Function: Validate
         * Description: Validates the field values of this class.
         */
        public void Validate()
        {
            if (passengers <= 0)
            {
                throw new Exception("Passenger must be a positive number");
            }

            if (company == null)
            {
                throw new Exception("Company name must be given");
            }

            initialLocation.Validate();
            
            foreach (var segment in segments)
            {
                segment.Validate();
            }
        }
    }
}