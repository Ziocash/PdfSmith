using Microsoft.AspNetCore.OpenApi;
using PdfSmith.BusinessLayer.OpenApi;

namespace PdfSmith.BusinessLayer.Extensions;
public static class OpenApiExtensions
{
    public static OpenApiOptions AddTimeZoneHeader(this OpenApiOptions options)
        => options.AddOperationTransformer<TimeZoneHeaderOperationTransformer>();
}
