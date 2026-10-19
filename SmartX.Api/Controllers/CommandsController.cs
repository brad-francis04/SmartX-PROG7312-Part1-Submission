using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;
using SmartX.Shared;

namespace SmartX.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommandsController : ControllerBase
{
    private readonly CommandProcessingService _processingService;
    private readonly SmartXStore _store;

    public CommandsController(
        CommandProcessingService processingService,
        SmartXStore store)
    {
        _processingService = processingService;
        _store = store;
    }

    [HttpPost]
    public ActionResult<CommandResult> IssueCommand(
        [FromBody] DeviceCommand command)
    {
        var sensor = _store.GetSensor(command.SensorId);

        if (sensor is null)
        {
            return NotFound(new CommandResult
            {
                Success = false,
                Message = "Sensor not found."
            });
        }

        command.DeviceName = sensor.DeploymentLocation;
        command.IssuedAt = DateTime.UtcNow;
        command.IsUndone = false;

        _processingService.RecordCommand(command);

        // If the command is marked critical, queue the sensor's latest
        // telemetry as a critical alert.
        if (command.IsCritical)
        {
            var telemetry = _store.GetTelemetry()
                .LastOrDefault(packet => packet.SensorId == command.SensorId);

            if (telemetry is not null)
            {
                _processingService.EnqueueCriticalAlert(telemetry);
            }
        }
        return Ok(new CommandResult
        {
            Success = true,
            Message = "Command recorded successfully.",
            Command = command
        });
    }

    [HttpPost("undo")]
    public ActionResult<CommandResult> UndoLastCommand()
    {
        var command = _processingService.UndoLastCommand();

        if (command is null)
        {
            return BadRequest(new CommandResult
            {
                Success = false,
                Message = "There are no commands available to undo."
            });
        }

        return Ok(new CommandResult
        {
            Success = true,
            Message = "Command marked as undone.",
            Command = command
        });
    }

    [HttpGet("history")]
    public ActionResult<List<DeviceCommand>> GetCommandHistory()
    {
        return Ok(_processingService.GetCommandHistory());
    }
}