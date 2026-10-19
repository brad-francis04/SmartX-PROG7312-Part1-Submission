namespace SmartX.Shared;

// Commands that an operator can send to a device.
public enum DeviceCommandType
{
    TurnOn,
    TurnOff,
    StartPump,
    StopPump,
    Reset
}

// Represents a command issued to a registered device.
public class DeviceCommand
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SensorId { get; set; }

    public string DeviceName { get; set; } = "";

    public DeviceCommandType CommandType { get; set; }

    public string Description { get; set; } = "";

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public bool IsCritical { get; set; }

    public bool IsUndone { get; set; }
}

// Result returned after a command is processed.
public class CommandResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = "";

    public DeviceCommand? Command { get; set; }
}