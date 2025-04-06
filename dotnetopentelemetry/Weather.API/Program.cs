using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddOpenTelemetry()
    .WithMetrics(metric =>
    {
        metric.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Service - Weather.API"));
        metric.AddMeter("Meter - Weather.API");
        
        metric.AddAspNetCoreInstrumentation();
        metric.AddRuntimeInstrumentation();
        metric.AddProcessInstrumentation();

        metric.AddOtlpExporter(exporter =>
        {
            exporter.Endpoint = new Uri("localhost:4317");
        });
    });

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/weather", () => Results.Ok("Hello World!"));
app.Run();