using Microsoft.AspNetCore.Mvc;
using FlightControlWeb.Models;
using FlightControlWeb.Data;
using System.Collections.Generic;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly IFcwRepo _repository;
        //private readonly MockFcwRepo _repository = new MockFcwRepo();

        public FlightsController(IFcwRepo repository)
        {
            _repository = repository;
        }


        // GET api/FlightPlan?relative_to=<DateTime>
        [HttpGet("{relative_to}")]
        public ActionResult <Flight> GetFlightsByTime(string relative_to, bool sync_all)
        {
            // @check if sync_all is in URL
            sync_all = false;
            var item = _repository.GetFlightsByTime(relative_to, sync_all);
            return Ok(item);
        }
    }
}