using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;
using SmartX.Shared;

namespace SmartX.Api.Controllers;

[ApiController]
[Route("api/telemetry-processing")]
public class TelemetryProcessingController : ControllerBase
{
    private readonly CommandProcessingService _processingService;
    private readonly SmartXStore _store;
    public TelemetryProcessingController(
     CommandProcessingService processingService,
     SmartXStore store)
    {
        _processingService = processingService;
        _store = store;
    }
    // View telemetry waiting to be processed.
    [HttpGet("pending")]
    public ActionResult<List<TelemetryPacket<double>>> GetPendingTelemetry()
    {
        return Ok(_processingService.GetPendingTelemetry());
    }

    // Process the next packet.
    // Critical alerts are processed before ordinary telemetry.
    [HttpPost("process-next")]
    public ActionResult<TelemetryPacket<double>> ProcessNextTelemetry()
    {
        TelemetryPacket<double>? packet =
            _processingService.ProcessNextTelemetry();

        if (packet is null)
        {
            return NotFound("There is no telemetry waiting to be processed.");
        }

        return Ok(packet);
    }

    // View telemetry that has already been processed.
    [HttpGet("processed")]
    public ActionResult<List<TelemetryPacket<double>>> GetProcessedTelemetry()
    {
        return Ok(_processingService.GetProcessedTelemetry());
    }

    // View the number of queued packets.
    [HttpGet("queue-status")]
    public ActionResult<object> GetQueueStatus()
    {
        return Ok(new
        {
            NormalQueueCount = _processingService.NormalQueueCount,
            CriticalQueueCount = _processingService.CriticalQueueCount
        });
    }
    // Queue a registered sensor's latest telemetry as a critical alert.
    [HttpPost("critical/{sensorId:guid}")]
    public ActionResult<TelemetryPacket<double>> EnqueueCriticalAlert(
        Guid sensorId,
        [FromQuery] int priority = 1)
    {
        if (priority < 1)
        {
            return BadRequest("Priority must be 1 or greater.");
        }

        var telemetry = _store.GetTelemetry()
            .LastOrDefault(packet => packet.SensorId == sensorId);

        if (telemetry is null)
        {
            return NotFound(
                "No telemetry found for this sensor. Generate telemetry first.");
        }

        _processingService.EnqueueCriticalAlert(telemetry, priority);

        return Ok(new
        {
            Message = "Critical alert queued successfully.",
            Priority = priority,
            Telemetry = telemetry
        });
    }
}