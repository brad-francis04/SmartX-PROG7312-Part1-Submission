using SmartX.Api.Services;
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Network;
using SmartX.Shared;
using System;
using System.Linq;
using System.Collections.Generic;


namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NetworkTopologyController : ControllerBase
    {
        private readonly NetworkGraphService graph;
        private readonly NetworkRoutingService routing;
        private readonly TelemetryTreeService telemetry;
        private readonly SmartXStore store;

        public NetworkTopologyController(
    NetworkGraphService graph,
    NetworkRoutingService routing,
    TelemetryTreeService telemetry,
    SmartXStore store)
        {
            this.graph = graph;
            this.routing = routing;
            this.telemetry = telemetry;
            this.store = store;
        }

        // Retrieve Part 1 sensors and make them available in Part 3.
        [HttpGet("nodes")]
        public ActionResult GetNodes()
        {
            var existingNodes = graph.GetAllNodes();

            foreach (var sensor in store.GetSensors())
            {
                bool alreadyExists = existingNodes.Any(
                    node => node.SensorId == sensor.Id.ToString());

                if (!alreadyExists)
                {
                    var networkNode = new NetworkNodeModel
                    {
                        SensorId = sensor.Id.ToString(),
                        DeviceName = sensor.DeploymentLocation,
                        MacAddress = sensor.MacAddress,
                        Status = NetworkNodeStatus.Online,
                        IsGateway = false,
                        LastSeen = new DateTimeOffset(
                            sensor.LastTelemetryAt ?? sensor.RegisteredAt)
                    };

                    graph.AddNode(networkNode);
                }
            }

            return Ok(graph.GetAllNodes());
        }

        // Assign a sensor as the central network gateway.
        [HttpPut("gateway/{id}")]
        public ActionResult SetNetworkGateway(string id)
        {
            // Ensure Part 1 sensors are loaded into the network graph.
            GetNodes();

            bool success = graph.SetGateway(id);

            if (!success)
            {
                return NotFound("Sensor not found. Cannot assign gateway.");
            }

            var gateway = graph.GetAllNodes()
                .First(node => node.IsGateway);

            return Ok(new
            {
                Message = "Central gateway assigned successfully.",
                Gateway = gateway
            });
        }
        // Retrieve all wireless connections.
        [HttpGet("connections")]
        public ActionResult GetConnections()
        {
            return Ok(graph.GetAllConnections());
        }

        // Create a wireless connection.
        [HttpPost("connections")]
        public ActionResult AddConnection(
            NetworkConnectionModel connection)
        {
            try
            {
                graph.AddConnection(connection);
                return Ok(connection);
            }
            catch (Exception ex) when (
                ex is ArgumentException ||
                ex is InvalidOperationException)
            {
                return BadRequest(ex.Message);
            }
        }

        // Perform Breadth-First Search.
        [HttpGet("bfs/{id}")]
        public ActionResult BFS(string id)
        {
            if (!graph.GetAllNodes().Any(n => n.SensorId == id))
                return NotFound("Sensor not found.");

            return Ok(graph.BreadthFirstSearch(id));
        }

        // Perform Depth-First Search.
        [HttpGet("dfs/{id}")]
        public ActionResult DFS(string id)
        {
            if (!graph.GetAllNodes().Any(n => n.SensorId == id))
                return NotFound("Sensor not found.");

            return Ok(graph.DepthFirstSearch(id));
        }
        // Find the route from a selected sensor to the central gateway.
        [HttpGet("route/{id}")]
        public ActionResult GetRouteToGateway(string id)
        {
            if (!graph.GetAllNodes().Any(n => n.SensorId == id))
                return NotFound("Sensor not found.");

            var route = graph.FindRouteToGateway(id);

            if (route.Count == 0)
                return NotFound("No available route to the central gateway.");

            return Ok(route);
        }

        // Find disconnected sensors.
        [HttpGet("disconnected/{id}")]
        public ActionResult GetDisconnected(string id)
        {
            if (!graph.GetAllNodes().Any(n => n.SensorId == id))
                return NotFound("Gateway sensor not found.");

            return Ok(graph.GetDisconnectedNodes(id));
        }

        // Calculate the Minimum Spanning Tree.
        [HttpGet("mst")]
        public ActionResult GetMinimumSpanningTree()
        {
            var connections =
                routing.CalculateMinimumSpanningTree(graph);

            var totalCost =
                routing.CalculateTotalPowerCost(connections);

            return Ok(new
            {
                Connections = connections,
                TotalPowerCost = totalCost
            });
        }

        // Store historical telemetry.
        [HttpPost("readings")]
        public ActionResult AddReading(
            HistoricalTelemetryRecord record)
        {
            try
            {
                telemetry.AddReading(record);
                return Ok(record);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Retrieve all historical readings.
        [HttpGet("readings")]
        public ActionResult GetAllReadings()
        {
            return Ok(telemetry.GetAllReadings());
        }

        // Retrieve historical readings for a sensor.
        [HttpGet("readings/{id}")]
        public ActionResult GetReadings(
            string id,
            [FromQuery] DateTimeOffset start,
            [FromQuery] DateTimeOffset end)
        {
            try
            {
                var results = telemetry.GetReadingsInRange(
                    id, start, end);

                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}