using Microsoft.AspNetCore.Mvc;
using FlightControlWeb.Models;
using FlightControlWeb.Data;

namespace FlightControlWeb.Controllers
{
    [Route("api/servers")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        private readonly IFcwRepo _repository;
        
        public ServersController(IFcwRepo repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<Response> GetAllServers()
        {
            var item = _repository.GetAllServers();
            return Ok(item);
        }

        // Delete api/servers/{id}
        [HttpDelete("{id}")]
        public ActionResult<Response> DeleteServer(string id)
        {
            var item = _repository.DeleteServer(id);
            return Ok(item);
        }

        [HttpPost]
        public ActionResult<Response> PostServer(Server server)
        {
            var item = _repository.PostServer(server);
            return Ok(item);
        }
    }
}