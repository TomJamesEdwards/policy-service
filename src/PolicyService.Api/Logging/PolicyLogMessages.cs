using Microsoft.Extensions.Logging;
using PolicyService.Domain.Policies;

namespace PolicyService.Api.Logging;

internal static partial class PolicyLogMessages
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message =
            "Policy {PolicyReference} sold as {PolicyType}. "
            + "Trace identifier: {TraceId}")]
    internal static partial void PolicySold(
        ILogger logger,
        string policyReference,
        PolicyType policyType,
        string traceId);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message =
            "Policy {PolicyReference} cancelled. "
            + "Trace identifier: {TraceId}")]
    internal static partial void PolicyCancelled(
        ILogger logger,
        string policyReference,
        string traceId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message =
            "Policy {PolicyReference} renewed to {EndDate}. "
            + "Trace identifier: {TraceId}")]
    internal static partial void PolicyRenewed(
        ILogger logger,
        string policyReference,
        DateOnly endDate,
        string traceId);
}