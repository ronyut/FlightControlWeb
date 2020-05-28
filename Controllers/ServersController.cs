/* This is a controller for api/servers.
 * This controller enables posting, deleting and viewing servers.
 * 
 * Author: Rony Utesvky.
 * Date: May 28, 2020
 */

using Microsoft.AspNetCore.Mvc;
using FlightControlWeb.Models;
using FlightControlWeb.Data;
using System.Collections.Generic;

namespace FlightControlWeb.Controllers
{
    [Route("api/servers")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        private readonly IFcwRepo _repository;
        
        /*
         * Ctor
         */
        public ServersController(IFcwRepo repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Server>> GetAllServers()
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