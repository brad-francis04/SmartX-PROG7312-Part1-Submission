using System;
using System.Collections.Generic;
using System.Linq;
using SmartX.Shared;

namespace SmartX.Api.Network
{
    public class NetworkGraphService
    {
        // Stores all sensors in the network.
        private readonly Dictionary<string, NetworkNodeModel> nodes = new();

        // Stores wireless connections between sensors.
        private readonly Dictionary<string, List<NetworkConnectionModel>> adjacencyList = new();

        // Adds or updates a sensor in the network.
        public void AddNode(NetworkNodeModel node)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.SensorId))
                throw new ArgumentException("A valid sensor is required.");

            nodes[node.SensorId] = node;

            if (!adjacencyList.ContainsKey(node.SensorId))
                adjacencyList[node.SensorId] = new List<NetworkConnectionModel>();
        }

        // Adds a wireless connection between two sensors.
        public void AddConnection(NetworkConnectionModel connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (!nodes.ContainsKey(connection.SourceSensorId) ||
                !nodes.ContainsKey(connection.DestinationSensorId))
                throw new InvalidOperationException("Both sensors must exist before connecting them.");

            if (connection.SourceSensorId == connection.DestinationSensorId)
                throw new ArgumentException("A sensor cannot connect to itself.");

            if (connection.PowerCost < 0 ||
     double.IsNaN(connection.PowerCost) ||
     double.IsInfinity(connection.PowerCost))
            {
                throw new ArgumentException(
                    "Power cost must be a valid non-negative number.");
            }

            if (connection.Distance < 0 ||
                double.IsNaN(connection.Distance) ||
                double.IsInfinity(connection.Distance))
            {
                throw new ArgumentException(
                    "Distance must be a valid non-negative number.");
            }

            if (adjacencyList[connection.SourceSensorId]
                .Any(existing =>
                    existing.DestinationSensorId == connection.DestinationSensorId))
            {
                throw new InvalidOperationException(
                    "A connection between these sensors already exists.");
            }
            adjacencyList[connection.SourceSensorId].Add(connection);

            // Mesh connections work in both directions.
            var reverseConnection = new NetworkConnectionModel
            {
                SourceSensorId = connection.DestinationSensorId,
                DestinationSensorId = connection.SourceSensorId,
                Distance = connection.Distance,
                PowerCost = connection.PowerCost,
                IsActive = connection.IsActive
            };

            adjacencyList[connection.DestinationSensorId].Add(reverseConnection);
        }

        // Checks whether a sensor is available for routing.
        private bool IsAvailable(string sensorId)
        {
            var node = nodes[sensorId];

            return node.Status == NetworkNodeStatus.Online ||
                   node.Status == NetworkNodeStatus.Gateway ||
                   node.IsGateway;
        }

        // Breadth-First Search: explores the network level by level.
        public List<string> BreadthFirstSearch(string startSensorId)
        {
            if (!nodes.ContainsKey(startSensorId) || !IsAvailable(startSensorId))
                return new List<string>();

            var visited = new HashSet<string>();
            var queue = new Queue<string>();
            var result = new List<string>();

            visited.Add(startSensorId);
            queue.Enqueue(startSensorId);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                result.Add(current);

                foreach (var connection in adjacencyList[current])
                {
                    string neighbour = connection.DestinationSensorId;

                    if (connection.IsActive &&
                        IsAvailable(neighbour) &&
                        visited.Add(neighbour))
                    {
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return result;
        }
        // Finds the shortest route from a selected sensor to the central gateway.
        public List<string> FindRouteToGateway(string startSensorId)
        {
            var gateway = nodes.Values.FirstOrDefault(node =>
                node.IsGateway || node.Status == NetworkNodeStatus.Gateway);

            if (gateway == null ||
                !nodes.ContainsKey(startSensorId) ||
                !IsAvailable(startSensorId))
            {
                return new List<string>();
            }

            var queue = new Queue<string>();
            var visited = new HashSet<string>();
            var previous = new Dictionary<string, string>();

            queue.Enqueue(startSensorId);
            visited.Add(startSensorId);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();

                if (current == gateway.SensorId)
                {
                    var route = new List<string>();
                    string step = gateway.SensorId;

                    route.Add(step);

                    while (previous.ContainsKey(step))
                    {
                        step = previous[step];
                        route.Add(step);
                    }

                    route.Reverse();
                    return route;
                }

                foreach (var connection in adjacencyList[current])
                {
                    string neighbour = connection.DestinationSensorId;

                    if (connection.IsActive &&
                        IsAvailable(neighbour) &&
                        visited.Add(neighbour))
                    {
                        previous[neighbour] = current;
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return new List<string>();
        }

        // Depth-First Search: explores one path before backtracking.
        public List<string> DepthFirstSearch(string startSensorId)
        {
            var result = new List<string>();
            var visited = new HashSet<string>();

            if (!nodes.ContainsKey(startSensorId) || !IsAvailable(startSensorId))
                return result;

            void Traverse(string current)
            {
                visited.Add(current);
                result.Add(current);

                foreach (var connection in adjacencyList[current])
                {
                    string neighbour = connection.DestinationSensorId;

                    if (connection.IsActive &&
                        IsAvailable(neighbour) &&
                        !visited.Contains(neighbour))
                    {
                        Traverse(neighbour);
                    }
                }
            }

            Traverse(startSensorId);

            return result;
        }

        // Returns devices that cannot currently be reached from the gateway.
        public List<NetworkNodeModel> GetDisconnectedNodes(string gatewaySensorId)
        {
            var reachable = BreadthFirstSearch(gatewaySensorId);

            return nodes.Values
                .Where(node =>
                    !reachable.Contains(node.SensorId) ||
                    !IsAvailable(node.SensorId))
                .ToList();
        }

        // Assigns one sensor as the central network gateway.
        public bool SetGateway(string sensorId)
        {
            if (string.IsNullOrWhiteSpace(sensorId) ||
                !nodes.ContainsKey(sensorId))
            {
                return false;
            }

            // Remove the gateway designation from any previous gateway.
            foreach (var node in nodes.Values)
            {
                if (node.IsGateway ||
                    node.Status == NetworkNodeStatus.Gateway)
                {
                    node.IsGateway = false;

                    // Only change Gateway status back to Online.
                    // Preserve sensors that are already Offline.
                    if (node.Status == NetworkNodeStatus.Gateway)
                    {
                        node.Status = NetworkNodeStatus.Online;
                    }
                }
            }

            // Assign the selected sensor as the new gateway.
            var gateway = nodes[sensorId];

            gateway.IsGateway = true;
            gateway.Status = NetworkNodeStatus.Gateway;

            return true;
        }
        // Returns all registered network nodes.
        public List<NetworkNodeModel> GetAllNodes()
        {
            return nodes.Values.ToList();
        }

        // Returns all registered wireless connections.
        public List<NetworkConnectionModel> GetAllConnections()
        {
            return adjacencyList.Values
                .SelectMany(connections => connections)
                .Where(connection =>
                    string.CompareOrdinal(
                        connection.SourceSensorId,
                        connection.DestinationSensorId) < 0)
                .ToList();
        }
    }
}