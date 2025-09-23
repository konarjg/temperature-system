namespace TemperatureSystem.Endpoints;

using Domain.Entities;
using Domain.Records;
using Domain.Services.Interfaces;
using Domain.Services.Util;
using Dto;
using Mappers;
using Microsoft.AspNetCore.Mvc;

public record HistoryQueryParameters(
  [FromQuery] DateTime StartDate,
  [FromQuery] DateTime EndDate,
  [FromQuery] long? SensorId = null
);

public record AggregatedHistoryQueryParameters(
  [FromQuery] DateTime StartDate, 
  [FromQuery] DateTime EndDate, 
  [FromQuery] MeasurementHistoryGranularity Granularity,
  [FromQuery] long SensorId
);

public static class MeasurementEndpoints {
  public static async Task<IResult> GetById(IMeasurementService measurementService, long id) {
    Measurement? measurement = await measurementService.GetByIdAsync(id);
    
    return measurement != null ? Results.Ok(measurement.ToDto()) : Results.NotFound();
  }
  
  public static async Task<IResult> GetLatest(IMeasurementService measurementService, [FromQuery] long sensorId) {
    Measurement? measurement = await measurementService.GetLatestAsync(sensorId);

    return measurement != null ? Results.Ok(measurement.ToDto()) : Results.NotFound();
  }

  public static async Task<IResult> GetHistory(IMeasurementService measurementService,
    [AsParameters] HistoryQueryParameters query) {

    List<Measurement> history = query.SensorId.HasValue ? await measurementService.GetHistoryForSensorAsync(query.StartDate, query.EndDate, query.SensorId.Value) 
                                  : await measurementService.GetHistoryAsync(query.StartDate,query.EndDate);
    
    return Results.Ok(history.Select(m => m.ToDto()));
  }

  public static async Task<IResult> GetAggregatedHistory(IMeasurementService measurementService,
    [AsParameters] AggregatedHistoryQueryParameters query) {

    List<AggregatedMeasurement> history = await measurementService.GetAggregatedHistoryForSensorAsync(query.StartDate, query.EndDate, query.Granularity, query.SensorId);

    return Results.Ok(history);
  }

  public static async Task<IResult> Create(IMeasurementService measurementService,
    [FromBody] CreateMeasurementDto data) {

    Measurement measurement = data.ToEntity();
    
    return await measurementService.CreateAsync(measurement) ? Results.CreatedAtRoute(nameof(GetById), new { id = measurement.Id }, measurement.ToDto()) : Results.UnprocessableEntity();
  }
  
  public static async Task<IResult> DeleteById(IMeasurementService measurementService, long id) {
    return await measurementService.DeleteByIdAsync(id) ? Results.NoContent() : Results.NotFound();
  }

  public static IEndpointRouteBuilder MapMeasurementEndpoints(this IEndpointRouteBuilder app) {
    RouteGroupBuilder group = app.MapGroup("/api/measurements"); 
    
    group.MapGet("/latest", GetLatest); 
    group.MapGet("/{id:long}", GetById).WithName(nameof(GetById));
    group.MapGet("/history",GetHistory);
    group.MapGet("/aggregated-history",GetAggregatedHistory);
    group.MapPost("/",Create);
    group.MapDelete("/{id:long}",DeleteById);

    return app;
  }
}
