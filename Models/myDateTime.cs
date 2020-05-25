using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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
        public int _year { get; set; }
        [JsonIgnore]
        public int _month { get; set; }
        [JsonIgnore]
        public int _day { get; set; }
        [JsonIgnore]
        public int _hour { get; set; }
        [JsonIgnore]
        public int _minute { get; set; }
        [JsonIgnore]
        public int _second { get; set; }

        public MyDateTime(string str)
        {
            parseIsoDate(str);
            
            this.iso = str;
            this.unix = (int) makeUnix();
            this.sql = makeSql();
        }

        public MyDateTime(int unix)
        {
            this.unix = unix;
            this.iso = makeIso();
            this.sql = makeSql();
        }

        static public int getDiff(MyDateTime x, MyDateTime y)
        {
            return y.unix - x.unix;
        }

        public string makeIso()
        {
            DateTime begin = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            begin = begin.AddSeconds(this.unix).ToLocalTime();
            return begin.ToString().Replace(" ", "T") + "Z";
        }

        public double makeUnix()
        {
            var dateTime = new DateTime(_year, _month, _day, _hour, _minute, _second, DateTimeKind.Utc);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixDateTime = (dateTime.ToUniversalTime() - epoch).TotalSeconds;
            return unixDateTime;
        }

        public string makeSql()
        {
           return this.iso.Replace("T", " ").Replace("Z", "");
        }

        public void parseIsoDate(string isoDate) {
            if (isIsoDate(isoDate)) {
                var regex = new Regex(@"(\d+)");
                var matches = regex.Matches(isoDate);
                var count = matches.Count;
                
                if (count != 6) {
                    throw new Exception("Invalid DateTime Format");
                }

                int i = 1;
                foreach (Match match in matches)
                {
                    int number = Int32.Parse(match.ToString());
                    switch(i++)
                    {
                        case 1:
                            _year = number;
                            break;
                        case 2:
                            _month = number;
                            break;
                        case 3:
                            _day = number;
                            break;
                        case 4:
                            _hour = number;
                            break;
                        case 5:
                            _minute = number;
                            break;
                        case 6:
                            _second = number;
                            break;
                    }
                }
            }
        }

        public static bool isIsoDate(string date)
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