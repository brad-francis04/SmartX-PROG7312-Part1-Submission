namespace SmartX.Shared
{
    // Represents the current state of a network sensor.
    public enum NetworkNodeStatus
    {
        Online,
        Offline,
        Gateway
    }

    // Represents an individual IoT sensor in the mesh network.
    public class NetworkNodeModel
    {
        public string SensorId { get; set; } = string.Empty;

        public string DeviceName { get; set; } = string.Empty;

        public string MacAddress { get; set; } = string.Empty;

        public NetworkNodeStatus Status { get; set; }

        public bool IsGateway { get; set; }

        public double BatteryLevel { get; set; }

        public DateTimeOffset LastSeen { get; set; }
    }

    // Represents a wireless connection between two sensors.
    public class NetworkConnectionModel
    {
        public string SourceSensorId { get; set; } = string.Empty;

        public string DestinationSensorId { get; set; } = string.Empty;

        public double Distance { get; set; }

        public double PowerCost { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // Stores an individual historical telemetry reading.
    public class HistoricalTelemetryRecord
    {
        public string SensorId { get; set; } = string.Empty;

        public string MetricName { get; set; } = string.Empty;

        public double Value { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }

    // Represents a calculated network route.
    public class NetworkRouteResult
    {
        public string SensorId { get; set; } = string.Empty;

        public List<string> Route { get; set; } = new();

        public double TotalPowerCost { get; set; }

        public bool IsReachable { get; set; }
    }
}
