namespace TemperatureSystem.Swagger;

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class IgnoreAnonymousActionsFilter : IOperationFilter {
  public void Apply(OpenApiOperation operation, OperationFilterContext context) { 
    IList<object> endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
    bool isAnonymous = endpointMetadata.Any(em => em is AllowAnonymousAttribute);
    
    if (!isAnonymous) {
      return;
    }
    
    operation.Security = new List<OpenApiSecurityRequirement>();
  }
}
