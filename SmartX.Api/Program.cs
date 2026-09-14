
using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Lets the Blazor dashboard safely call this API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("SmartXClient", policy =>
    {
        // Allows the SmartX dashboard running on your local computer
        // to communicate with the SmartX API during development.
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add our in-memory data service once for the whole application.
builder.Services.AddSingleton<SmartXStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("SmartXClient");

app.MapControllers();

// A small API health check.
app.MapGet("/health", () => Results.Ok("SmartX API is running."));

app.Run();