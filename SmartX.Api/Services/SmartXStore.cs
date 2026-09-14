using SmartX.Shared;

namespace SmartX.Api.Services;

// This service stores information while the application is running.
// Part 1 uses mock/in-memory data, so a database is not required yet.
public class SmartXStore
{
    // Generic List<T> requirement.
    private readonly List<SensorDevice> _sensors = new();

    // Generic telemetry collection.
    private readonly List<TelemetryPacket<double>> _telemetry = new();

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
        return _sensors.FirstOrDefault(sensor => sensor.Id == id);
    }

    public SensorDevice? RegisterSensor(SensorRegistrationRequest request)
    {
        bool alreadyExists = _sensors.Any(sensor =>
            sensor.MacAddress.Equals(request.MacAddress, StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            return null;
        }

        var sensor = new SensorDevice
        {
            MacAddress = request.MacAddress.Trim(),
            DeploymentLocation = request.DeploymentLocation.Trim(),
            Category = request.Category
        };

        _sensors.Add(sensor);
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
