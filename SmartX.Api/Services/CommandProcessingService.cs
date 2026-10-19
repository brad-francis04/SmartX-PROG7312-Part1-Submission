using SmartX.Shared;

namespace SmartX.Api.Services;

public class CommandProcessingService
{
    private readonly object _lock = new();

    // FIFO queue for ordinary telemetry.
    private readonly Queue<TelemetryPacket<double>> _telemetryQueue = new();

    // Critical alerts: lower numeric priority values are processed first.
    private readonly PriorityQueue<TelemetryPacket<double>, int> _priorityQueue = new();

    // Stack preserves last-in, first-out command processing.
    private readonly Stack<DeviceCommand> _commandHistory = new();

    // Keeps commands visible even after they are undone.
    private readonly List<DeviceCommand> _allCommands = new();

    // Processed telemetry history.
    private readonly List<TelemetryPacket<double>> _processedTelemetry = new();

    public void EnqueueTelemetry(TelemetryPacket<double> packet)
    {
        lock (_lock)
        {
            _telemetryQueue.Enqueue(packet);
        }
    }

    public void EnqueueCriticalAlert(
        TelemetryPacket<double> packet,
        int priority = 1)
    {
        lock (_lock)
        {
            _priorityQueue.Enqueue(packet, priority);
        }
    }

    public TelemetryPacket<double>? ProcessNextTelemetry()
    {
        lock (_lock)
        {
            TelemetryPacket<double>? packet = null;

            // Critical alerts take precedence over ordinary telemetry.
            if (_priorityQueue.Count > 0)
            {
                packet = _priorityQueue.Dequeue();
            }
            else if (_telemetryQueue.Count > 0)
            {
                packet = _telemetryQueue.Dequeue();
            }

            if (packet is not null)
            {
                _processedTelemetry.Add(packet);
            }

            return packet;
        }
    }

    public List<TelemetryPacket<double>> GetProcessedTelemetry()
    {
        lock (_lock)
        {
            return _processedTelemetry.ToList();
        }
    }

    public List<TelemetryPacket<double>> GetPendingTelemetry()
    {
        lock (_lock)
        {
            var pending = new List<TelemetryPacket<double>>();

            pending.AddRange(_priorityQueue.UnorderedItems
                .OrderBy(item => item.Priority)
                .Select(item => item.Element));

            pending.AddRange(_telemetryQueue);

            return pending;
        }
    }

    public List<DeviceCommand> GetCommandHistory()
    {
        lock (_lock)
        {
            // Newest commands appear first, including undone commands.
            return _allCommands
                .OrderByDescending(command => command.IssuedAt)
                .ToList();
        }
    }

    public void RecordCommand(DeviceCommand command)
    {
        lock (_lock)
        {
            _commandHistory.Push(command);
            _allCommands.Add(command);
        }
    }

    public DeviceCommand? UndoLastCommand()
    {
        lock (_lock)
        {
            // Find the newest command that has not already been undone.
            DeviceCommand? command = _allCommands
                .Where(c => !c.IsUndone)
                .OrderByDescending(c => c.IssuedAt)
                .FirstOrDefault();

            if (command is null)
            {
                return null;
            }

            command.IsUndone = true;

            // Remove it from the undo stack if it is at the top.
            if (_commandHistory.Count > 0 &&
                _commandHistory.Peek().Id == command.Id)
            {
                _commandHistory.Pop();
            }

            return command;
        }
    }

    public int NormalQueueCount
    {
        get
        {
            lock (_lock)
            {
                return _telemetryQueue.Count;
            }
        }
    }

    public int CriticalQueueCount
    {
        get
        {
            lock (_lock)
            {
                return _priorityQueue.Count;
            }
        }
    }

    public int CommandHistoryCount
    {
        get
        {
            lock (_lock)
            {
                return _commandHistory.Count;
            }
        }
    }
}