using Microsoft.AspNetCore.Mvc;
using FlightControlWeb.Models;
using FlightControlWeb.Data;
using System.Threading.Tasks;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly IFcwRepo _repository;

        public FlightsController(IFcwRepo repository)
        {
            _repository = repository;
        }

        // GET api/FlightPlan?relative_to=<DateTime>&sync_all
        [HttpGet]
        public async Task<ActionResult<Flight>> GetFlightsByTimeAsync([FromQuery]
                                                                      string relative_to)
        {
            bool isSyncAll = Request.Query.ContainsKey("sync_all");
            var item = await _repository.GetFlightsByTimeAsync(relative_to, isSyncAll);
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