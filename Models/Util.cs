/* This is a utility class.
 * 
 * Author: Rony Utesvky.
 * Date: May 28, 2020
 */
namespace FlightControlWeb.Models
{
    public class Util
    {   
        /*
         * Function: BoolToInt
         * Description: Convert bool to int.
         */
        public static int BoolToInt(bool boolean)
        {
            return boolean ? 1 : 0;
        }
        
        /*
         * Function: IntToBool
         * Description: Convert int to bool.
         */
        public static bool IntToBool(int integer)
        {
            return integer != 1 ? false : true;
        }
    }
}