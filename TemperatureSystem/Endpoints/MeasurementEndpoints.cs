namespace TemperatureSystem.Endpoints;

using System.ComponentModel.DataAnnotations;
using Domain.Entities;
using Domain.Entities.Util;
using Domain.Records;
using Domain.Services.Interfaces;
using Domain.Services.Util;
using Dto;
using ExternalServiceAdapters.NotificationService.Measurement;
using Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniValidation;

public record LatestQueryParameters(
  [FromQuery] long SensorId,
  [FromQuery][Range(1, 1000)] int Points
);

public record HistoryPageQueryParameters(
  [FromQuery] DateTime StartDate,
  [FromQuery] DateTime EndDate,
  [FromQuery][Range(1, 100)] int PageSize = 100,
  [FromQuery][Range(1, int.MaxValue)] int Page = 1,
  [FromQuery] long? SensorId = null
);

public record AggregatedHistoryQueryParameters(
  [FromQuery] DateTime StartDate, 
  [FromQuery] DateTime EndDate, 
  [FromQuery] MeasurementHistoryGranularity Granularity,
  [FromQuery] long SensorId
);

public static class MeasurementEndpoints {

  private const string GetByIdRoute = "GetMeasurementById";
  
  public static async Task<IResult> GetById(IMeasurementService measurementService, long id) {
    Measurement? measurement = await measurementService.GetByIdAsync(id);
    
    return measurement != null ? Results.Ok(measurement.ToDto()) : Results.NotFound();
  }
  
  public static async Task<IResult> GetLatest(IMeasurementService measurementService, [AsParameters] LatestQueryParameters query) {
    if (!MiniValidator.TryValidate(query, out IDictionary<string, string[]> errors)) {
      return Results.ValidationProblem(errors);
    }
    
    List<Measurement> measurements = await measurementService.GetLatestAsync(query.SensorId, query.Points);

    return Results.Ok(measurements.Select(m => m.ToDto()));
  }

  public static async Task<IResult> GetHistoryPage(IMeasurementService measurementService,
    [AsParameters] HistoryPageQueryParameters query) {
    
    if (!MiniValidator.TryValidate(query, out IDictionary<string, string[]> errors)) {
      return Results.ValidationProblem(errors);
    }

    PagedResult<Measurement> history = await measurementService.GetHistoryPageAsync(query.StartDate, query.EndDate, query.Page, query.PageSize, query.SensorId);
    
    return Results.Ok(history.ToDto());
  }

  public static async Task<IResult> GetAggregatedHistory(IMeasurementService measurementService,
    [AsParameters] AggregatedHistoryQueryParameters query) {

    List<AggregatedMeasurement> history = await measurementService.GetAggregatedHistoryForSensorAsync(query.StartDate, query.EndDate, query.Granularity, query.SensorId);

    return Results.Ok(history);
  }
  
  public static async Task<IResult> DeleteById(IMeasurementService measurementService, long id) {
    switch (await measurementService.DeleteByIdAsync(id)) {
      case OperationResult.NotFound:
        return Results.NotFound();
      
      case OperationResult.ServerError:
        return Results.InternalServerError();
      
      case OperationResult.Success:
        return Results.NoContent();
    }

    return Results.BadRequest();
  }

  public static IEndpointRouteBuilder MapMeasurementEndpoints(this IEndpointRouteBuilder app) {
    app.MapHub<MeasurementHub>("/hub/measurements").RequireAuthorization();
    
    RouteGroupBuilder group = app.MapGroup("/api/measurements"); 
    
    group.MapGet("/latest", GetLatest).RequireAuthorization(); 
    group.MapGet("/{id:long}", GetById).WithName(GetByIdRoute).RequireAuthorization(); 
    group.MapGet("/history",GetHistoryPage).RequireAuthorization();
    group.MapGet("/aggregated-history",GetAggregatedHistory).RequireAuthorization(); 
    
    group.MapDelete("/{id:long}",DeleteById).RequireAuthorization(new AuthorizeAttribute() {
      Roles = nameof(Role.Admin)
    }); 
    
    return app;
  }
}
