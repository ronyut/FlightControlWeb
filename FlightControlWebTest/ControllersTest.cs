using FlightControlWeb.Controllers;
using FlightControlWeb.Data;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace FlightControlWebTest
{
    [TestClass]
    public class ControllersTest
    {
        private MockFcwRepo _mockRepo;


        // Ctor - create Mock repository.
        public ControllersTest()
        {
            _mockRepo = new MockFcwRepo();
        }

        [TestMethod]
        public void GetFlightPlanById_ReturnFlighPlan()
        {
            FlightPlanController fpController = new FlightPlanController(_mockRepo);
            var expected = _mockRepo.GetFlightPlanByIdOutput;

            var answer = fpController.GetFlightPlanByIdAsync("KOKO68");
            OkObjectResult result = (OkObjectResult)answer.Result.Result;
            var actual = result.Value;

            Assert.AreEqual(actual, expected, "Invalid Answer in GetFlightPlanByIdAsync method.");
        }

        [TestMethod]
        public void PostFlightPlan_ReturnValidResponse()
        {
            FlightPlanController fpController = new FlightPlanController(_mockRepo);
            FlightPlan mockFlightPlan = GetMockFlightPlan();
            var expected = _mockRepo.PostFlightPlanOutput;

            var answer = fpController.PostFlightPlan(mockFlightPlan);
            OkObjectResult result = (OkObjectResult)answer.Result;
            var actual = result.Value;

            Assert.AreEqual(actual, expected, "Invalid Answer in Post FlightPlan method.");
        }

        [TestMethod]
        public void ServersController_GetServers_ReturnServers()
        {
            var expected = _mockRepo.GetServersOutput;
            ServersController serversController = new ServersController(_mockRepo);

            var answer = (OkObjectResult)serversController.GetAllServers().Result;
            var actual = answer.Value;

            Assert.AreEqual(actual, expected, "Invalid Answer in get servers method.");
        }

        [TestMethod]
        public void ServersController_deleteServer_ReturnValidResponse()
        {
            var expected = _mockRepo.DeleteServerOutput;
            ServersController serversController = new ServersController(_mockRepo);

            var answer = (OkObjectResult)serversController.DeleteServer("").Result;
            var actual = answer.Value;

            Assert.AreEqual(actual, expected, "Invalid Answer in delete servers method.");
        }

        [TestMethod]
        public void ServersController_postServer_ReturnValidResponse()
        {
            var expected = _mockRepo.DeleteServerOutput;
            ServersController serversController = new ServersController(_mockRepo);

            var answer = (OkObjectResult)serversController.DeleteServer("").Result;
            var actual = answer.Value;

            Assert.AreEqual(actual, expected, "Invalid Answer in get servers method.");
        }

        [TestMethod]
        public void FlightsController_DeleteFlights_ReturnValidResponse()
        {
            String errorMessage = "Invalid Answer in delete flight method.";
            var expected = _mockRepo.DeleteFlightByIdOutput;
            FlightsController flightsController = new FlightsController(_mockRepo);

            var answer = (OkObjectResult)flightsController.DeleteFlightById("").Result;
            var actual = answer.Value;

            Assert.AreEqual(actual, expected, errorMessage);
        }

        public FlightPlan GetMockFlightPlan()
        {
            Coordinate c = new Coordinate(12, 32);
            InitialLocation il = new InitialLocation(c, "2020-05-30T12:00:00Z");
            List<Segment> segs = new List<Segment>();
            FlightPlan fp = new FlightPlan(12, "mock airlines", il, segs);

            return fp;
        }



        //GetFlightPlanByIdAsync(string id)

        //PostFlightPlan(FlightPlan flightPlan)
    }
}
