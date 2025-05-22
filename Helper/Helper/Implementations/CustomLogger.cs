using Helper.Interfaces;
using Microsoft.Extensions.Logging;

namespace Helper.Implementations;

public class CustomLogger : ICustomLogger
{
    readonly ILogger<CustomLogger> _logger;

    public CustomLogger(ILogger<CustomLogger> logger) => _logger = logger;

    public void LogError(string layerName, Exception? ex, string customMsg)
    {
        _logger.LogError(ex, $"An error occurred at {DateTime.Now} in {layerName} layer, error custom message is '{customMsg}'");
    }
}
