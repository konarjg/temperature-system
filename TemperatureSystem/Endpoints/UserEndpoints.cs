namespace TemperatureSystem.Endpoints;

using System.Security.Claims;
using Domain.Entities;
using Domain.Entities.Util;
using Domain.Services.Interfaces;
using Domain.Services.Util;
using Dto;
using Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public static class UserEndpoints {

  private const string GetByIdRoute = "GetUserById";
  
  public static async Task<IResult> GetById(long id,
    IUserService userService) {

    User? user = await userService.GetByIdAsync(id);
    return user != null ? Results.Ok(user.ToDto()) : Results.NotFound();
  }
  
  public static async Task<IResult> Register([FromBody] UserRequest request,
    IAuthService authService) {

    RegisterResult result = await authService.RegisterAsync(request.ToDomainCreateDto());
    
    switch (result.State) {
      case RegisterState.Conflict:
        return Results.Conflict();
      
      case RegisterState.ServerError:
        return Results.InternalServerError();
      
      case RegisterState.Success:
        return Results.CreatedAtRoute(GetByIdRoute, new { id = result.User.Id}, result.User.ToDto());
    }
    
    return Results.BadRequest();
  }

  public static async Task<IResult> UpdateCredentialsById(long id,
    [FromBody] UserRequest data, IUserService userService, ClaimsPrincipal user) {
    
    string currentUserIdString = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    
    if (!long.TryParse(currentUserIdString, out long currentUserId)) {
      return Results.Unauthorized();
    }
    
    if (!user.IsInRole(nameof(Role.Admin)) && currentUserId != id) {
      return Results.Forbid(); 
    }
    
    switch (await userService.UpdateCredentialsByIdAsync(id,data.ToDomainUpdateDto())) {
      case OperationResult.NotFound:
        return Results.NotFound();
      
      case OperationResult.ServerError:
        return Results.InternalServerError();
      
      case OperationResult.Success:
        return Results.Ok();
    }

    return Results.BadRequest();
  }

  public static async Task<IResult> UpdateRoleById(long id,
    [FromBody] UserRoleRequest data, IUserService userService) {
    
    switch (await userService.UpdateRoleByIdAsync(id,data.ToDomainDto())) {
      case OperationResult.NotFound:
        return Results.NotFound();
      
      case OperationResult.ServerError:
        return Results.InternalServerError();
      
      case OperationResult.Success:
        return Results.Ok();
    }

    return Results.BadRequest();
  }

  public static async Task<IResult> DeleteById(long id, IUserService userService, ClaimsPrincipal user) {
    string currentUserIdString = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    
    if (!long.TryParse(currentUserIdString, out long currentUserId)) {
      return Results.Unauthorized();
    }
    
    if (!user.IsInRole(nameof(Role.Admin)) && currentUserId != id) {
      return Results.Forbid(); 
    }
    
    switch (await userService.DeleteByIdAsync(id)) {
      case OperationResult.NotFound:
        return Results.NotFound();
      
      case OperationResult.ServerError:
        return Results.InternalServerError();
      
      case OperationResult.Success:
        return Results.NoContent();
    }

    return Results.BadRequest();
  }
  
  public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app) {
    RouteGroupBuilder  group = app.MapGroup("/api/users");

    group.MapGet("/{id:long}",GetById).WithName(GetByIdRoute).RequireAuthorization();
    group.MapPost("/",Register).AllowAnonymous();
    group.MapPut("/{id:long}/credentials",UpdateCredentialsById).RequireAuthorization();
    
    group.MapPut("/{id:long}/role",UpdateRoleById).RequireAuthorization(new AuthorizeAttribute() {
      Roles = nameof(Role.Admin)
    });
    
    group.MapDelete("/{id:long}",DeleteById).RequireAuthorization();
    return app;
  }
}
