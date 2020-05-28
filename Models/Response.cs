/* This class represents response to request.
 * 
 * Author: Rony Utesvky.
 * Date: May 28, 2020
 */

using Newtonsoft.Json;

namespace FlightControlWeb
{
    public class Response
    {
        public string request { get; set; }
        [JsonProperty("success")]
        public bool isSuccess { get; set; }
        public string msg { get; set; }
        
        /*
         * Ctor
         */
        public Response(string request, bool isSuccess, string msg)
        {
            this.request = request;
            this.isSuccess = isSuccess;
            this.msg = msg;
        }
    }
}