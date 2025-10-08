using System.Text;
using System.Text.Json.Serialization;
using DatabaseAdapters.Repositories.SqLite;
using Domain;
using Domain.Services.Util;
using ExternalServiceAdapters;
using ExternalServiceAdapters.NotificationService.Measurement;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TemperatureSystem.Endpoints;
using TemperatureSystem.HostedServices;
using TemperatureSystem.Swagger;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
  options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
  options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
  options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
    Name = "Authorization",
    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
  });
  
  options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
    {
      new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
        Reference = new Microsoft.OpenApi.Models.OpenApiReference {
          Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
          Id = "Bearer"
        }
      },
      []
    }
  });
  
  options.OperationFilter<IgnoreAnonymousActionsFilter>();
});
builder.Services.AddAuthentication(options => {
 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
 options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters {
   ValidateIssuer = true,
   ValidateAudience = true,
   ValidateLifetime = true,
   ValidateIssuerSigningKey = true,
   ValidIssuer = builder.Configuration["Jwt:Issuer"],
   ValidAudience = builder.Configuration["Jwt:Audience"],
   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
 }; 
});

builder.Services.AddAuthorization();

builder.Services.AddSqLiteDatabaseAdapter(builder.Configuration);
builder.Services.AddDomain();
builder.Services.AddExternalServices();
builder.Services.AddHostedService<MeasurementScheduler>();
builder.Services.AddHostedService<TokenCleanupService>();

WebApplication app = builder.Build();
string? env = builder.Configuration["Environment"];

if (env is "Dev") {
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapMeasurementEndpoints();
app.MapSensorEndpoints();
app.MapUserEndpoints();
app.MapAuthEndpoints();

if (!app.Environment.IsEnvironment("Testing")) {
  using IServiceScope scope = app.Services.CreateScope();
  
  try {
    DbContext dbContext = scope.ServiceProvider.GetRequiredService<SqLiteDatabaseContext>();
    dbContext.Database.Migrate();
  }
  catch (Exception ex) {
    ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
    throw;
  }
}

app.Run();

public partial class Program { }