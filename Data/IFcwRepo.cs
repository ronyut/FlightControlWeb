/* This is an interface fo the repositories.
 * 
 * Author: Rony Utesvky.
 * Date: May 28, 2020
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using FlightControlWeb.Models;

namespace FlightControlWeb.Data
{
    public interface IFcwRepo
    {
        // Flight Plans
        Task<FlightPlan> GetFlightPlanByIdAsync(string id);
        Response PostFlightPlan(FlightPlan flightPlan);

        // Flights
        Task<IEnumerable<Flight>> GetFlightsByTimeAsync(string date, bool isExternal);
        Response DeleteFlightById(string id);
        
        // Servers
        Response PostServer(Server server);
        Response DeleteServer(string id);
        IEnumerable<Server> GetAllServers();
    }
}