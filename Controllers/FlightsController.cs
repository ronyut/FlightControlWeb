using Microsoft.AspNetCore.Mvc;
using FlightControlWeb.Models;
using FlightControlWeb.Data;
using System.Collections.Generic;
using System;

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


        // GET api/FlightPlan?relative_to=<DateTime>&sync_all
        public ActionResult <Flight> GetFlightsByTime([FromQuery] string relative_to)
        {
            var url = new string(this.HttpContext.Request.QueryString.Value);
            bool isSyncAll = url.IndexOf("sync_all", StringComparison.OrdinalIgnoreCase) >= 0;
            
            var item = _repository.GetFlightsByTime(relative_to, isSyncAll);
            return Ok(item);
        }
    }
}