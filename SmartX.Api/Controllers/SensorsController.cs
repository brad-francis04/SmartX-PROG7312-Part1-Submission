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

    public SensorsController(SmartXStore store, IWebHostEnvironment environment)
    {
        _store = store;
        _environment = environment;
    }

    [HttpGet]
    public ActionResult<List<SensorDevice>> GetSensors()
    {
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

        return Ok(packet);
    }

    [HttpGet("telemetry")]
    public ActionResult<List<TelemetryPacket<double>>> GetTelemetry()
    {
        return Ok(_store.GetTelemetry());
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