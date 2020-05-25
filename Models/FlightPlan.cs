using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlightControlWeb.Models
{
    public class FlightPlan
    {
        [JsonIgnore]
        public string flight_id { get; set; }
        [Required]
        public int passengers { get; set; }
        [Required]
        public string company_name { get; set; }
        public InitialLocation initial_location { get; set; }
        [Required]
        public IEnumerable<Segment> segments { get; set; }

        public FlightPlan (int passengers, string company_name, InitialLocation initial_location, IEnumerable<Segment> segments)
        {
            this.flight_id = GenerateFlightID();
            this.passengers = passengers;
            this.company_name = company_name;
            this.initial_location = initial_location;
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
            return GetRandomConsonant() + GetRandomVowel() + GetRandomConsonant() + GetRandomVowel() + GetRandomDigit() + GetRandomDigit();
        }
    }
}