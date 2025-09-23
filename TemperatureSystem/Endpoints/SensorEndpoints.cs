namespace TemperatureSystem.Endpoints;

using Domain.Entities;
using Domain.Services.Interfaces;
using Mappers;

public static class SensorEndpoints {
  public static async Task<IResult> GetAll(ISensorService sensorService) {
    List<Sensor> sensors = await sensorService.GetAllAsync();
    return Results.Ok(sensors.Select(s => s.ToDto()).ToList());
  }
  
  public static IEndpointRouteBuilder MapSensorEndpoints(this IEndpointRouteBuilder app) {
    RouteGroupBuilder group = app.MapGroup("/api/sensors"); 
    
    group.MapGet("/", GetAll); 
    return app;
  }
}
