using Microsoft.AspNetCore.Mvc;
using FlightControlWeb.Models;
using FlightControlWeb.Data;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly IFcwRepo _repository;
        private readonly HttpClient _httpClient;

        public FlightsController(IFcwRepo repository)
        {
            _repository = repository;
            _httpClient = new HttpClient();
        }

        // GET api/FlightPlan?relative_to=<DateTime>&sync_all
        [HttpGet]
        public async Task<ActionResult<Flight>> GetFlightsByTimeAsync([FromQuery]
                                                                      string relative_to)
        {
            bool isSyncAll = Request.Query.ContainsKey("sync_all");
            var item = await _repository.GetFlightsByTimeAsync(relative_to, isSyncAll, _httpClient);
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public ActionResult<Response> DeleteFlightById(string id)
        {
            var response = _repository.DeleteFlightById(id);
            return Ok(response);
        }


    }
}