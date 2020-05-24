using Microsoft.AspNetCore.Mvc;
using FlightControlWeb.Models;
using FlightControlWeb.Data;
using System.Collections.Generic;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        private readonly IFcwRepo _repository;
        //private readonly MockFcwRepo _repository = new MockFcwRepo();

        public FlightPlanController(IFcwRepo repository)
        {
            _repository = repository;
        }


        // GET api/FlightPlan
        [HttpGet]
        public ActionResult <IEnumerable<FlightPlan>> GetAllFlightPlans()
        {
            var items = _repository.GetAllFlightPlans();
            return Ok(items);
        }

        // GET api/FlightPlan/{id}
        [HttpGet("{id}")]
        public ActionResult <FlightPlan> GetFlightPlanById(int id)
        {
            var item = _repository.GetFlightPlanById(id);
            return Ok(item);
        }
    }
}