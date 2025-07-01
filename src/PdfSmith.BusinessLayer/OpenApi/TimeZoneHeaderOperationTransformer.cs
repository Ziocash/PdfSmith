using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using PdfSmith.BusinessLayer.Services;

namespace PdfSmith.BusinessLayer.OpenApi;
internal class TimeZoneHeaderOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Parameters ??= [];

        if (!operation.Parameters.Any(p => p.Name == TimeZoneService.X_TIME_ZONE && p.In == ParameterLocation.Header))
        {
            operation.Parameters.Add(new()
            {
                Name = TimeZoneService.X_TIME_ZONE,
                In = ParameterLocation.Header,
                Required = false,
                Schema = new()
                {
                    Type = "string"
                }
            });
        }

        return Task.CompletedTask;
    }
}
