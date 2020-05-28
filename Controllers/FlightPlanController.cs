/* This is a controller for api/FlightPlan.
 * This controller enables posting and viewing a flight plan.
 * 
 * Author: Rony Utesvky.
 * Date: May 28, 2020
 */

using Microsoft.AspNetCore.Mvc;
using FlightControlWeb.Models;
using FlightControlWeb.Data;
using System.Threading.Tasks;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        private readonly IFcwRepo _repository;
        
        /*
         * Ctor
         */
        public FlightPlanController(IFcwRepo repository)
        {
            _repository = repository;
        }

        // GET api/FlightPlan/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<FlightPlan>> GetFlightPlanByIdAsync(string id)
        {
            var item = await _repository.GetFlightPlanByIdAsync(id);
            return Ok(item);
        }

        [HttpPost]
        public ActionResult<Response> PostFlightPlan(FlightPlan flightPlan)
        {
            var item = _repository.PostFlightPlan(flightPlan);
            return Ok(item);
        }
    }
}