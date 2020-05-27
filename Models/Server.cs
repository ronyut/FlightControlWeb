using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace FlightControlWeb.Models
{
    public class Server
    {
        [Required]
        [JsonProperty("ServerId")]
        public string key { get; set; }
        [Required]
        [JsonProperty("ServerURL")]
        public string url { get; set; }

        public Server(string key, string url)
        {
            this.key = key;
            this.url = url;
        }
    }
}