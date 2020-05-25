namespace FlightControlWeb.Models
{
    public class Util
    {
        public static int BoolToInt(bool boolean)
        {
            return boolean ? 1 : 0;
        }

        public static bool IntToBool(int integer)
        {
            return integer != 1 ? false : true;
        }
    }
}