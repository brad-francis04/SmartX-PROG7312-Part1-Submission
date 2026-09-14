using System.ComponentModel.DataAnnotations;

namespace SmartX.Shared;

// These are the types of sensors that may be registered in Part 1.
public enum SensorCategory
{
    Environmental,
    PowerConsumption,
    Actuator,
    Security
}

// This is the information the operator enters on the dashboard.
public class SensorRegistrationRequest
{
    [Required]
    public string MacAddress { get; set; } = "";

    [Required]
    public string DeploymentLocation { get; set; } = "";

    public SensorCategory Category { get; set; }
}

// This represents one registered Smart-X sensor/device.
public class SensorDevice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string MacAddress { get; set; } = "";

    public string DeploymentLocation { get; set; } = "";

    public SensorCategory Category { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastTelemetryAt { get; set; }
}

// GENERICS REQUIREMENT:
// T means this packet can carry different kinds of data.
// Example: TelemetryPacket<double>, TelemetryPacket<int>, TelemetryPacket<bool>.
public class TelemetryPacket<T>
{
    public Guid SensorId { get; set; }

    public SensorCategory Category { get; set; }

    public T Payload { get; set; } = default!;

    public string Unit { get; set; } = "";

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

// OPERATOR OVERLOADING REQUIREMENT:
// This lets us add two PowerMeter objects using the + symbol.
public readonly record struct PowerMeter(double Watts)
{
    public static PowerMeter operator +(PowerMeter left, PowerMeter right)
    {
        return new PowerMeter(left.Watts + right.Watts);
    }
}

// This is returned after a log/photo/configuration file is uploaded.
public class UploadResult
{
    public string FileName { get; set; } = "";

    public string Message { get; set; } = "";
}

// This gives the dashboard its live counters.
public class DashboardSummary
{
    public int RegisteredSensorCount { get; set; }

    public int TelemetryCount { get; set; }

    public double TotalPowerWatts { get; set; }

    public double HistoricalAverage { get; set; }

    public int ConnectivityGridCells { get; set; }
}

// Used for the recursive deployment validation requirement.
public class DeploymentNode
{
    public string Name { get; set; } = "";

    public bool IsConfigured { get; set; }

    public List<DeploymentNode> Children { get; set; } = new();
}

public class DeploymentValidationResult
{
    public bool IsValid { get; set; }

    public List<string> Problems { get; set; } = new();
}