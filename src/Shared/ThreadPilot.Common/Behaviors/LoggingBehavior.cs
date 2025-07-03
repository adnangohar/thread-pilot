using System;
using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ThreadPilot.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestGuid = Guid.NewGuid();
        var requestJson = JsonSerializer.Serialize(request);
        
        _logger.LogInformation("Handling {RequestName} [{RequestGuid}] with data: {RequestData}", 
            requestName, requestGuid, requestJson);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        _logger.LogInformation("Handled {RequestName} [{RequestGuid}] in {ElapsedMilliseconds}ms", 
            requestName, requestGuid, stopwatch.ElapsedMilliseconds);

        return response;
    }
}
