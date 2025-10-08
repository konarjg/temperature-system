namespace TemperatureSystem.Endpoints;

using Domain.Records;
using Domain.Services.Interfaces;
using Domain.Services.Util;
using Mappers;
using Microsoft.AspNetCore.Mvc;

public static class AuthEndpoints {

  private const string RefreshTokenCookie = "refreshToken";
  
  public static async Task<IResult> Login([FromBody] AuthRequest request, IAuthService authService, HttpResponse response) {
    AuthResult? result = await authService.LoginAsync(request.Email, request.Password);

    if (result == null) {
      return Results.Unauthorized();
    }
    
    response.Cookies.Append(RefreshTokenCookie, result.RefreshToken.Token, new CookieOptions() {
      HttpOnly = true,
      Expires = result.RefreshToken.Expires,
      Secure = false,
      SameSite = SameSiteMode.Lax
    });

    return Results.Ok(result.ToDto());
  }

  public static async Task<IResult> Refresh(IAuthService authService,
    HttpRequest request, HttpResponse response) {

    if (!request.Cookies.TryGetValue(RefreshTokenCookie, out string? refreshToken)) {
      return Results.Unauthorized();
    }

    AuthResult? result = await authService.RefreshAsync(refreshToken);
    
    if (result == null) {
      return Results.Unauthorized();
    }
    
    response.Cookies.Append(RefreshTokenCookie, result.RefreshToken.Token, new CookieOptions() {
      HttpOnly = true,
      Expires = result.RefreshToken.Expires,
      Secure = false,
      SameSite = SameSiteMode.Lax
    });

    return Results.Ok(result.ToDto());
  }

  public static async Task<IResult> Logout(IAuthService authService,
    HttpRequest request,
    HttpResponse response) {
    
    if (!request.Cookies.TryGetValue(RefreshTokenCookie, out string? refreshToken)) {
      return Results.Unauthorized();
    }
    
    response.Cookies.Delete(RefreshTokenCookie);
    return await authService.LogoutAsync(refreshToken) ? Results.Ok() : Results.Unauthorized();
  }

  public static async Task<IResult> Verify(string token, IAuthService authService) {
    switch (await authService.VerifyAsync(token)) {
      case OperationResult.NotFound:
        return Results.NotFound();
      
      case OperationResult.ServerError:
        return Results.InternalServerError();
      
      case OperationResult.Success:
        return Results.Ok();
    }

    return Results.BadRequest();
  }
  
  public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app) {
    RouteGroupBuilder group = app.MapGroup("/api/auth");
    
    group.MapPost("/",Login).AllowAnonymous();
    group.MapPut("/refresh",Refresh).AllowAnonymous();
    group.MapDelete("/logout",Logout).AllowAnonymous();
    group.MapGet("/verify/{token}",Verify).AllowAnonymous();
    
    return app;
  }
};
       
