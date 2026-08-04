using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Globalization;
using System.Threading.RateLimiting;

namespace NoteManagementAPI.Configuration;

internal static class AuthenticationRateLimitPolicies
{
    public const string Registration = "authentication-registration";
    public const string Login = "authentication-login";
    public const string Refresh = "authentication-refresh";

    public static IServiceCollection AddAuthenticationRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var leaseRetryAfter)
                    ? leaseRetryAfter
                    : GetPolicyWindow(context.HttpContext);
                context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds)
                    .ToString(CultureInfo.InvariantCulture);

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too many requests.",
                        Detail = "Try again after the rate-limit window resets."
                    },
                    options: null,
                    contentType: "application/problem+json",
                    cancellationToken: cancellationToken);
            };

            options.AddPolicy(
                Registration,
                context => CreateIpSlidingWindowPartition(
                    context,
                    permitLimit: 5,
                    window: TimeSpan.FromMinutes(10),
                    segmentsPerWindow: 10));
            options.AddPolicy(
                Login,
                context => CreateIpSlidingWindowPartition(
                    context,
                    permitLimit: 20,
                    window: TimeSpan.FromMinutes(1),
                    segmentsPerWindow: 6));
            options.AddPolicy(
                Refresh,
                context => CreateIpSlidingWindowPartition(
                    context,
                    permitLimit: 20,
                    window: TimeSpan.FromMinutes(1),
                    segmentsPerWindow: 6));
        });

        return services;
    }

    private static TimeSpan GetPolicyWindow(HttpContext context)
    {
        var policyName = context.GetEndpoint()?
            .Metadata.GetMetadata<EnableRateLimitingAttribute>()?
            .PolicyName;

        return policyName == Registration
            ? TimeSpan.FromMinutes(10)
            : TimeSpan.FromMinutes(1);
    }

    private static RateLimitPartition<string> CreateIpSlidingWindowPartition(
        HttpContext context,
        int permitLimit,
        TimeSpan window,
        int segmentsPerWindow)
    {
        var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                SegmentsPerWindow = segmentsPerWindow,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    }
}
