using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Services;
using SmartX.Shared;

namespace SmartX.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SensorsController : ControllerBase
{
    private readonly SmartXStore _store;
    private readonly IWebHostEnvironment _environment;
    private readonly CommandProcessingService _processingService;
    public SensorsController(
     SmartXStore store,
     IWebHostEnvironment environment,
     CommandProcessingService processingService)
    {
        _store = store;
        _environment = environment;
        _processingService = processingService;
    }

    [HttpGet]
    public ActionResult<List<SensorDevice>> GetSensors(
      [FromQuery] SensorCategory? category,
      [FromQuery] bool? hasActiveAlert)
    {
        if (category.HasValue || hasActiveAlert.HasValue)
        {
            return Ok(_store.FilterSensors(category, hasActiveAlert));
        }

        return Ok(_store.GetSensors());
    }

    [HttpPost]
    public ActionResult<SensorDevice> RegisterSensor(SensorRegistrationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        SensorDevice? sensor = _store.RegisterSensor(request);

        if (sensor is null)
        {
            return Conflict("A sensor with that MAC/unique ID already exists.");
        }

        return Ok(sensor);
    }

    [HttpPost("{sensorId:guid}/telemetry/generate")]
    public ActionResult<TelemetryPacket<double>> GenerateMockTelemetry(Guid sensorId)
    {
        TelemetryPacket<double>? packet = _store.GenerateMockTelemetry(sensorId);

        if (packet is null)
        {
            return NotFound("Sensor not found.");
        }

        // Add generated telemetry to the Part 2 FIFO queue.
        _processingService.EnqueueTelemetry(packet);

        return Ok(packet);
    }

    [HttpGet("telemetry")]
    public ActionResult<List<TelemetryPacket<double>>> GetTelemetry()
    {
        return Ok(_store.GetTelemetry());
    }
    [HttpGet("telemetry/history")]
    public ActionResult<List<TelemetryPacket<double>>> GetHistoricalTelemetry()
    {
        return Ok(_store.GetHistoricalTelemetry());
    }
    [HttpGet("alerts/active")]
    public ActionResult<List<SensorDevice>> GetActiveAlertSensors()
    {
        return Ok(_store.GetActiveAlertSensors());
    }
    [HttpPost("search-history")]
    public IActionResult RecordSearch([FromBody] string searchTerm)
    {
        _store.RecordSensorSearch(searchTerm);
        return Ok(new { Message = "Search recorded successfully." });
    }

    [HttpGet("search-history")]
    public ActionResult<List<string>> GetSearchHistory()
    {
        return Ok(_store.GetSearchHistory());
    }
    [HttpGet("suggested-actions")]
    public ActionResult<List<string>> GetSuggestedActions()
    {
        var suggestions = _store.GetSuggestedActions();

        var commands = _processingService.GetCommandHistory()
            .Where(command => !command.IsUndone)
            .ToList();

        // Recommend reviewing devices that receive the same command repeatedly.
        var repeatedCommands = commands
            .GroupBy(command => new
            {
                command.SensorId,
                command.CommandType
            })
            .Where(group => group.Count() >= 3);

        foreach (var group in repeatedCommands)
        {
            var sensor = _store.GetSensor(group.Key.SensorId);

            string deviceName = sensor?.DeploymentLocation
                ?? group.First().DeviceName;

            suggestions.Add(
                $"Review {deviceName}: the {group.Key.CommandType} command has been issued {group.Count()} times.");
        }

        // Recommend reviewing devices that have received repeated critical commands.
        var repeatedCriticalCommands = commands
            .Where(command => command.IsCritical)
            .GroupBy(command => command.SensorId)
            .Where(group => group.Count() >= 2);

        foreach (var group in repeatedCriticalCommands)
        {
            var sensor = _store.GetSensor(group.Key);

            string deviceName = sensor?.DeploymentLocation
                ?? group.First().DeviceName;

            suggestions.Add(
                $"Investigate {deviceName}: this device has received {group.Count()} critical commands.");
        }

        // Remove the default message if a command-history recommendation was generated.
        if (suggestions.Any(suggestion =>
            suggestion.Contains("command has been issued") ||
            suggestion.Contains("critical commands")))
        {
            suggestions.RemoveAll(suggestion =>
                suggestion.StartsWith("No immediate recommendations."));
        }

        return Ok(suggestions.Distinct().ToList());
    }
    [HttpPost("{sensorId:guid}/alerts/activate")]
    public IActionResult ActivateAlert(Guid sensorId)
    {
        if (_store.GetSensor(sensorId) is null)
        {
            return NotFound("Sensor not found.");
        }

        _store.AddActiveAlert(sensorId);

        return Ok(new
        {
            Message = "Alert activated.",
            SensorId = sensorId
        });
    }

    [HttpDelete("{sensorId:guid}/alerts/clear")]
    public IActionResult ClearAlert(Guid sensorId)
    {
        if (_store.GetSensor(sensorId) is null)
        {
            return NotFound("Sensor not found.");
        }

        _store.ClearActiveAlert(sensorId);

        return Ok(new
        {
            Message = "Alert cleared.",
            SensorId = sensorId
        });
    }

    [HttpGet("dashboard")]
    public ActionResult<DashboardSummary> GetDashboard()
    {
        return Ok(_store.GetDashboardSummary());
    }

    [HttpGet("deployment/validate")]
    public ActionResult<DeploymentValidationResult> ValidateDeployment()
    {
        return Ok(_store.ValidateDeployment());
    }

    [HttpPost("{sensorId:guid}/upload")]
    public async Task<ActionResult<UploadResult>> UploadFile(Guid sensorId, IFormFile file)
    {
        SensorDevice? sensor = _store.GetSensor(sensorId);

        if (sensor is null)
        {
            return NotFound("Sensor not found.");
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest("Please choose a file first.");
        }

        // Keeps the upload size reasonable for Part 1.
        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest("The file must be smaller than 10 MB.");
        }

        string safeFileName = Path.GetFileName(file.FileName);

        string folder = Path.Combine(
            _environment.ContentRootPath,
            "uploads",
            sensorId.ToString());

        Directory.CreateDirectory(folder);

        string destination = Path.Combine(
            folder,
            $"{DateTime.UtcNow:yyyyMMddHHmmss}_{safeFileName}");

        await using FileStream stream = System.IO.File.Create(destination);
        await file.CopyToAsync(stream);

        return Ok(new UploadResult
        {
            FileName = safeFileName,
            Message = "File uploaded successfully."
        });
    }
}