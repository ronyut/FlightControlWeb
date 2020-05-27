using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace FlightControlWeb.Models
{
    public class MyDateTime
    {
        public string iso { get; set; }
        [JsonIgnore]
        public int unix { get; set; }
        [JsonIgnore]
        public string sql { get; set; }
        [JsonIgnore]
        public int year { get; set; }
        [JsonIgnore]
        public int month { get; set; }
        [JsonIgnore]
        public int day { get; set; }
        [JsonIgnore]
        public int hour { get; set; }
        [JsonIgnore]
        public int minute { get; set; }
        [JsonIgnore]
        public int second { get; set; }

        public MyDateTime(string str)
        {
            ParseIsoDate(str);
            
            this.iso = str;
            this.unix = (int) MakeUnix();
            this.sql = MakeSql();
        }

        public MyDateTime(int unix)
        {
            this.unix = unix;
            this.iso = MakeIso();
            this.sql = MakeSql();
        }

        static public int GetDiff(MyDateTime x, MyDateTime y)
        {
            return y.unix - x.unix;
        }

        public string MakeIso()
        {
            DateTime begin = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            begin = begin.AddSeconds(this.unix).ToLocalTime();
            return begin.ToString().Replace(" ", "T") + "Z";
        }

        public double MakeUnix()
        {
            var dateTime = new DateTime(this.year, month, this.day, this.hour, this.minute,
                                        this.second, DateTimeKind.Utc);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixDateTime = (dateTime.ToUniversalTime() - epoch).TotalSeconds;
            return unixDateTime;
        }

        public string MakeSql()
        {
           return this.iso.Replace("T", " ").Replace("Z", "");
        }

        public void ParseIsoDate(string isoDate) {
            var regex = new Regex(@"(\d+)");
            var matches = regex.Matches(isoDate);
            var count = matches.Count;
            
            if (count != 6 || !IsIsoDate(isoDate)) {
                throw new Exception("Invalid DateTime Format");
            }

            int i = 1;
            foreach (Match match in matches)
            {
                int number = Int32.Parse(match.ToString());
                switch(i++)
                {
                    case 1:
                        this.year = number;
                        break;
                    case 2:
                        this.month = number;
                        break;
                    case 3:
                        this.day = number;
                        break;
                    case 4:
                        this.hour = number;
                        break;
                    case 5:
                        this.minute = number;
                        break;
                    case 6:
                        this.second = number;
                        break;
                }
            }
        }

        public static bool IsIsoDate(string date)
        {
            var regex = new Regex(@"^\<?(\d{4}-\d{2}-\d{2}[T\s]\d{2}:\d{2}:\d{2}Z?\>?)$");
            if (regex.Matches(date).Count == 1)
            {
                return true;
            }
            return false;
        }

    }
}