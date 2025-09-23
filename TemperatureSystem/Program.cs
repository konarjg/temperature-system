using System.Text.Json.Serialization;
using DatabaseAdapters.Repositories.SqLite;
using Domain;
using Domain.Services.Util;
using ExternalServiceAdapters;
using Microsoft.AspNetCore.Http.Json;
using TemperatureSystem.Endpoints;
using TemperatureSystem.HostedServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
  options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
  options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

List<SensorDefinition>? sensorDefinitions = builder.Configuration.GetSection("Sensors").Get<List<SensorDefinition>>();
builder.Services.AddSingleton(sensorDefinitions ?? []);

builder.Services.AddSqLiteDatabaseAdapter(builder.Configuration);
builder.Services.AddDomain();
builder.Services.AddExternalServices();
builder.Services.AddHostedService<MeasurementScheduler>();
builder.Services.AddHostedService<SensorSync>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapMeasurementEndpoints();
app.MapSensorEndpoints();

app.Run();
