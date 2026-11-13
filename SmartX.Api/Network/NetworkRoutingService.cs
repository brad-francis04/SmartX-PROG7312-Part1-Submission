using System;
using System.Collections.Generic;
using System.Linq;
using SmartX.Shared;

namespace SmartX.Api.Network
{
    public class NetworkRoutingService
    {
        // Calculates a Minimum Spanning Tree using Prim's algorithm.
        public List<NetworkConnectionModel> CalculateMinimumSpanningTree(
            NetworkGraphService graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            var nodes = graph.GetAllNodes();
            var connections = graph.GetAllConnections();

            // Only include sensors that are available.
            var availableSensorIds = nodes
                .Where(node =>
                    node.Status == NetworkNodeStatus.Online ||
                    node.Status == NetworkNodeStatus.Gateway ||
                    node.IsGateway)
                .Select(node => node.SensorId)
                .ToHashSet();

            // Ignore inactive connections and connections involving
            // unavailable sensors.
            var availableConnections = connections
                .Where(connection =>
                    connection.IsActive &&
                    availableSensorIds.Contains(connection.SourceSensorId) &&
                    availableSensorIds.Contains(connection.DestinationSensorId))
                .ToList();

            var selectedConnections = new List<NetworkConnectionModel>();
            var visited = new HashSet<string>();

            // Handle every connected component.
            foreach (var startNode in nodes)
            {
                if (!availableSensorIds.Contains(startNode.SensorId))
                    continue;

                if (!visited.Add(startNode.SensorId))
                    continue;

                while (true)
                {
                    NetworkConnectionModel? cheapestConnection = null;

                    foreach (var connection in availableConnections)
                    {
                        bool sourceVisited =
                            visited.Contains(connection.SourceSensorId);

                        bool destinationVisited =
                            visited.Contains(connection.DestinationSensorId);

                        // Only consider connections crossing the
                        // visited and unvisited boundary.
                        if (sourceVisited == destinationVisited)
                            continue;

                        if (cheapestConnection == null ||
                            connection.PowerCost < cheapestConnection.PowerCost)
                        {
                            cheapestConnection = connection;
                        }
                    }

                    if (cheapestConnection == null)
                        break;

                    selectedConnections.Add(cheapestConnection);

                    visited.Add(cheapestConnection.SourceSensorId);
                    visited.Add(cheapestConnection.DestinationSensorId);
                }
            }

            return selectedConnections;
        }

        // Calculates the total power cost.
        public double CalculateTotalPowerCost(
            List<NetworkConnectionModel> connections)
        {
            if (connections == null)
                throw new ArgumentNullException(nameof(connections));

            return connections.Sum(connection => connection.PowerCost);
        }
    }
}
