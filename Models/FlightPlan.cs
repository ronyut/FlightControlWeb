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

        public FlightPlan (int passengers, string company, InitialLocation initialLocation,
                           IEnumerable<Segment> segments)
        {
            this.flightId = GenerateFlightID();
            this.passengers = passengers;
            this.company = company;
            this.initialLocation = initialLocation;
            this.segments = segments;
        }

        public static string GetRandomConsonant()
        {
            return GetRandomChar("BCDFGHJKLMNPQRSTVWXYZ");
        }

        public static string GetRandomVowel()
        {
            return GetRandomChar("AEIOU");
        }

        public static string GetRandomDigit()
        {
            return GetRandomChar("0123456789");
        }

        public static string GetRandomChar(string chars)
        {
            var rand = new Random();
            int num = rand.Next(0, chars.Length -1);
            return chars[num] + "";
        }

        public static string GenerateFlightID()
        {
            return GetRandomConsonant() + GetRandomVowel() + GetRandomConsonant() +
                   GetRandomVowel() + GetRandomDigit() + GetRandomDigit();
        }

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
    }
}