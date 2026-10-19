using SmartX.Shared;

namespace SmartX.Api.Services;

// This service stores information while the application is running.
// Part 1 uses mock/in-memory data, so a database is not required yet.
public class SmartXStore
{
    // Generic List<T> requirement.
    private readonly List<SensorDevice> _sensors = new();
    // Dictionary lookups for registered sensors.
private readonly Dictionary<Guid, SensorDevice> _sensorsById = new();

private readonly Dictionary<string, SensorDevice> _sensorsByMac =
    new(StringComparer.OrdinalIgnoreCase);

    // Generic telemetry collection.
    private readonly List<TelemetryPacket<double>> _telemetry = new();
    // Stores historical telemetry in timestamp order.
    private readonly SortedDictionary<DateTime, List<TelemetryPacket<double>>> _historicalTelemetry = new();
    // Tracks unique sensors with active alerts or disconnections.
    private readonly HashSet<Guid> _activeAlertSensorIds = new();
    // Records sensor searches to support predictive recommendations.
    private readonly List<string> _searchHistory = new();

    // JAGGED ARRAY requirement:
    // Each historical batch may have a different number of readings.
    private readonly double[][] _historicalBatches =
    {
        new double[] { 20.1, 21.4, 20.8 },
        new double[] { 22.2, 21.9 },
        new double[] { 19.8, 20.0, 20.3, 21.1 }
    };

    // MULTIDIMENSIONAL ARRAY requirement:
    // Represents a small simulated connectivity/signal grid.
    private readonly int[,] _connectivityGrid =
    {
        { 80, 76, 71 },
        { 85, 90, 74 }
    };

    // A small deployment hierarchy for recursive validation.
    private readonly DeploymentNode _deploymentRoot = new()
    {
        Name = "Smart-X Facility",
        IsConfigured = true,
        Children =
        {
            new DeploymentNode
            {
                Name = "Hydroponic Zone A",
                IsConfigured = true,
                Children =
                {
                    new DeploymentNode
                    {
                        Name = "Environmental Sensor Node",
                        IsConfigured = true
                    }
                }
            },
            new DeploymentNode
            {
                Name = "Smart Grid Node 1",
                IsConfigured = true
            }
        }
    };

    public List<SensorDevice> GetSensors()
    {
        return _sensors.OrderByDescending(sensor => sensor.RegisteredAt).ToList();
    }

    public SensorDevice? GetSensor(Guid id)
    {
        return _sensorsById.TryGetValue(id, out SensorDevice? sensor)
            ? sensor
            : null;
    }

    public SensorDevice? RegisterSensor(SensorRegistrationRequest request)
    {
        string macAddress = request.MacAddress.Trim();

        if (_sensorsByMac.ContainsKey(macAddress))
        {
            return null;
        }

        var sensor = new SensorDevice
        {
            MacAddress = macAddress,
            DeploymentLocation = request.DeploymentLocation.Trim(),
            Category = request.Category
        };

        _sensors.Add(sensor);

        _sensorsById.Add(sensor.Id, sensor);
        _sensorsByMac.Add(sensor.MacAddress, sensor);

        return sensor;
    }
    public TelemetryPacket<double>? GenerateMockTelemetry(Guid sensorId)
    {
        var sensor = GetSensor(sensorId);

        if (sensor is null)
        {
            return null;
        }

        // Create different simulated values depending on sensor category.
        double value;
        string unit;

        switch (sensor.Category)
        {
            case SensorCategory.Environmental:
                value = Math.Round(18 + Random.Shared.NextDouble() * 12, 2);
                unit = "°C";
                break;

            case SensorCategory.PowerConsumption:
                value = Math.Round(50 + Random.Shared.NextDouble() * 450, 2);
                unit = "W";
                break;

            case SensorCategory.Actuator:
                value = Random.Shared.Next(0, 2);
                unit = "ON/OFF";
                break;

            default:
                value = Math.Round(Random.Shared.NextDouble() * 100, 2);
                unit = "%";
                break;
        }

        var packet = new TelemetryPacket<double>
        {
            SensorId = sensor.Id,
            Category = sensor.Category,
            Payload = value,
            Unit = unit
        };

        _telemetry.Add(packet);

        if (!_historicalTelemetry.TryGetValue(packet.RecordedAt, out var readings))
        {
            readings = new List<TelemetryPacket<double>>();
            _historicalTelemetry[packet.RecordedAt] = readings;
        }

        readings.Add(packet);

        sensor.LastTelemetryAt = packet.RecordedAt;

        return packet;
    }

    public List<TelemetryPacket<double>> GetTelemetry()
    {
        return _telemetry
            .OrderByDescending(packet => packet.RecordedAt)
            .Take(20)
            .ToList();
    }
    public List<TelemetryPacket<double>> GetHistoricalTelemetry()
    {
        return _historicalTelemetry
            .SelectMany(entry => entry.Value)
            .OrderByDescending(packet => packet.RecordedAt)
            .ToList();
    }
    public void AddActiveAlert(Guid sensorId)
    {
        if (_sensorsById.ContainsKey(sensorId))
        {
            _activeAlertSensorIds.Add(sensorId);
        }
    }

    public void ClearActiveAlert(Guid sensorId)
    {
        _activeAlertSensorIds.Remove(sensorId);
    }
    public List<SensorDevice> GetActiveAlertSensors()
    {
        return _sensors
            .Where(sensor => _activeAlertSensorIds.Contains(sensor.Id))
            .ToList();
    }
    public List<SensorDevice> FilterSensors(
    SensorCategory? category,
    bool? hasActiveAlert)
    {
        IEnumerable<SensorDevice> sensors = _sensors;

        if (category.HasValue)
        {
            sensors = sensors.Where(sensor => sensor.Category == category.Value);
        }

        if (hasActiveAlert.HasValue)
        {
            sensors = sensors.Where(sensor =>
                _activeAlertSensorIds.Contains(sensor.Id) == hasActiveAlert.Value);
        }

        return sensors.ToList();
    }
    public void RecordSensorSearch(string searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            _searchHistory.Add(searchTerm.Trim());
        }
    }

    public List<string> GetSearchHistory()
    {
        return _searchHistory.ToList();
    }
    public List<string> GetSuggestedActions()
    {
        var suggestions = new List<string>();

        // 1. Recommend investigating devices with active alerts.
        foreach (var sensor in GetActiveAlertSensors())
        {
            suggestions.Add(
                $"Investigate {sensor.DeploymentLocation}: this device has an active alert.");
        }

        // 2. Detect repeated searches.
        var repeatedSearches = _searchHistory
            .GroupBy(term => term, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() >= 2);

        foreach (var search in repeatedSearches)
        {
            suggestions.Add(
                $"You searched for '{search.Key}' repeatedly. Review the relevant device readings.");
        }

        // 3. Analyse recent telemetry for potentially concerning readings.
        foreach (var sensor in _sensors)
        {
            var latestReading = _telemetry
                .Where(packet => packet.SensorId == sensor.Id)
                .OrderByDescending(packet => packet.RecordedAt)
                .FirstOrDefault();

            if (latestReading is null)
            {
                continue;
            }

            // Example simulated threshold for environmental sensors.
            if (sensor.Category == SensorCategory.Environmental &&
                (latestReading.Payload < 18 || latestReading.Payload > 28))
            {
                suggestions.Add(
                    $"Review {sensor.DeploymentLocation}: the latest environmental reading is {latestReading.Payload} {latestReading.Unit}.");
            }

            // Example simulated threshold for power consumption.
            if (sensor.Category == SensorCategory.PowerConsumption &&
                latestReading.Payload > 400)
            {
                suggestions.Add(
                    $"Review power consumption at {sensor.DeploymentLocation}: the latest reading is {latestReading.Payload} {latestReading.Unit}.");
            }
        }

        // 4. Provide a default recommendation when no rules are triggered.
        if (suggestions.Count == 0)
        {
            suggestions.Add(
                "No immediate recommendations. Generate telemetry and review device readings to build useful history.");
        }

        return suggestions.Distinct().ToList();
    }
    public DashboardSummary GetDashboardSummary()
    {
        // Converts the jagged array into a generic List<double>.
        var allHistoricalReadings = new List<double>();

        foreach (double[] batch in _historicalBatches)
        {
            allHistoricalReadings.AddRange(batch);
        }

        // Operator overloading is used here.
        PowerMeter totalPower = new(0);

        foreach (var packet in _telemetry.Where(packet =>
                     packet.Category == SensorCategory.PowerConsumption))
        {
            totalPower = totalPower + new PowerMeter(packet.Payload);
        }

        return new DashboardSummary
        {
            RegisteredSensorCount = _sensors.Count,
            TelemetryCount = _telemetry.Count,
            TotalPowerWatts = Math.Round(totalPower.Watts, 2),
            HistoricalAverage = Math.Round(allHistoricalReadings.Average(), 2),
            ConnectivityGridCells = _connectivityGrid.Length
        };
    }

    public DeploymentValidationResult ValidateDeployment()
    {
        var result = new DeploymentValidationResult();

        // Start the recursive method at the root of the deployment tree.
        ValidateNode(_deploymentRoot, "", result.Problems);

        result.IsValid = result.Problems.Count == 0;
        return result;
    }

    // RECURSION REQUIREMENT:
    // This method calls itself for every child node.
    private static void ValidateNode(
        DeploymentNode node,
        string parentPath,
        List<string> problems)
    {
        string currentPath = $"{parentPath}/{node.Name}";

        if (!node.IsConfigured)
        {
            problems.Add($"{currentPath} is not configured.");
        }

        foreach (DeploymentNode child in node.Children)
        {
            ValidateNode(child, currentPath, problems);
        }
    }
}
