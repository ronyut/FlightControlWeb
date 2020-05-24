using System;
using System.Text.RegularExpressions;

namespace FlightControlWeb.Models
{
    public class myDateTime
    {
        public string iso { get; }
        public int unix { get; set; }
        public int _year { get; set; }
        public int _month { get; set; }
        public int _day { get; set; }
        public int _hour { get; set; }
        public int _minute { get; set; }
        public int _second { get; set; }

        public myDateTime(string str)
        {
            this.iso = str;
            parseIsoDate(str);
            this.unix = (int) makeUnix();
        }

        static public int getDiff(myDateTime x, myDateTime y)
        {
            return y.unix - x.unix;
        }

        public double makeUnix()
        {
            var dateTime = new DateTime(_year, _month, _day, _hour, _minute, _second, DateTimeKind.Local);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixDateTime = (dateTime.ToUniversalTime() - epoch).TotalSeconds;
            return unixDateTime;
        }

        public void parseIsoDate(string isoDate) {
            if (isIsoDate(isoDate)) {
                var regex = new Regex(@"^(\d{4})-(\d{2})-(\d{2})T\(d{2}):(\d{2}):(\d{2})Z$");
                var matches = regex.Matches(isoDate);
                var count = matches.Count;
                
                if (count != 6) {
                    throw new Exception("Invalid DateTime Format");
                }

                int i = 0;
                foreach (Match match in matches)
                {
                    int number = Int32.Parse(match.ToString());
                    switch(i++)
                    {
                        case 0:
                            _year = number;
                            break;
                        case 1:
                            _month = number;
                            break;
                        case 2:
                            _day = number;
                            break;
                        case 3:
                            _hour = number;
                            break;
                        case 4:
                            _minute = number;
                            break;
                        case 5:
                            _second = number;
                            break;
                    }
                }
            }
        }

        public static bool isIsoDate(string date)
        {
            var regex = new Regex(@"^(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z)$");
            if (regex.Matches(date).Count == 1)
            {
                return true;
            }
            return false;
        }

    }
}