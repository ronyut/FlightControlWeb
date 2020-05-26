using Newtonsoft.Json;

namespace FlightControlWeb
{
    public class Response
    {
        public string request { get; set; }
        [JsonProperty("success")]
        public bool isSuccess { get; set; }
        public string msg { get; set; }

        public Response(string request, bool isSuccess, string msg)
        {
            this.request = request;
            this.isSuccess = isSuccess;
            this.msg = msg;
        }
    }
}