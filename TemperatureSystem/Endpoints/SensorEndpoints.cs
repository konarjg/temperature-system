namespace TemperatureSystem.Endpoints;

using Domain.Entities;
using Domain.Entities.Util;
using Domain.Services.Interfaces;
using Domain.Services.Util;
using Dto;
using ExternalServiceAdapters.NotificationService.Sensor;
using Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public static class SensorEndpoints {

  private const string GetByIdRoute = "GetSensorById";
  
  public static async Task<IResult> GetById(long id,
    ISensorService sensorService) {

    Sensor? sensor = await sensorService.GetByIdAsync(id);
    return sensor != null ? Results.Ok(sensor.ToDto()) : Results.NotFound();
  }

  public static async Task<IResult> GetAll([FromQuery] SensorState? state, ISensorService sensorService) {

    List<Sensor> sensors = state != null ? await sensorService.GetAllByStateAsync(state.Value) : await sensorService.GetAllAsync();
    return Results.Ok(sensors.Select(s => s.ToDto()).ToList());
  }

  public static async Task<IResult> Create([FromBody] SensorRequest data,
    ISensorService sensorService) {

    Sensor sensor = data.ToEntity();
    return await sensorService.CreateAsync(sensor) ? Results.CreatedAtRoute(GetByIdRoute,  new { id = sensor.Id }, sensor.ToDto()) : Results.UnprocessableEntity();
  }

  public static async Task<IResult> UpdateById(long id,
    [FromBody] SensorRequest data, ISensorService sensorService) {

    switch (await sensorService.UpdateDefinitionByIdAsync(id,data.ToDomainDto())) {
      case OperationResult.NotFound:
        return Results.NotFound();
      
      case OperationResult.ServerError:
        return Results.InternalServerError();
      
      case OperationResult.Success:
        return Results.Ok();
    }

    return Results.BadRequest();
  }

  public static async Task<IResult> DeleteById(long id, ISensorService sensorService) {
    switch (await sensorService.DeleteByIdAsync(id)) {
      case OperationResult.NotFound:
        return Results.NotFound();
      
      case OperationResult.ServerError:
        return Results.InternalServerError();
      
      case OperationResult.Success:
        return Results.NoContent();
    }

    return Results.BadRequest();
  }

  public static IEndpointRouteBuilder MapSensorEndpoints(this IEndpointRouteBuilder app) {
    app.MapHub<SensorHub>("/hub/sensors").RequireAuthorization();
    
    RouteGroupBuilder group = app.MapGroup("/api/sensors");

    group.MapGet("/{id:long}",GetById).WithName(GetByIdRoute).RequireAuthorization();
    group.MapGet("/",GetAll).RequireAuthorization();
    group.MapPost("/",Create).RequireAuthorization(new AuthorizeAttribute() {
      Roles = nameof(Role.Admin)
    });
    
    group.MapPut("/{id:long}",UpdateById).RequireAuthorization(new AuthorizeAttribute() {
      Roles = nameof(Role.Admin)
    });
    
    group.MapDelete("/{id:long}",DeleteById).RequireAuthorization(new AuthorizeAttribute() {
      Roles = nameof(Role.Admin)
    });
    
    return app;
  }
}
