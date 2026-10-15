using System.Threading.Channels;
using METS.StockReplenishment.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace METS.StockReplenishment.Infrastructure.Services;

public interface IStockValidationQueue
{
    ValueTask QueueValidationAsync(int requestId);
}

public class StockValidationBackgroundService : BackgroundService, IStockValidationQueue
{
    private readonly Channel<int> _channel = Channel.CreateUnbounded<int>();
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StockValidationBackgroundService> _logger;

    public StockValidationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<StockValidationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async ValueTask QueueValidationAsync(int requestId)
    {
        await _channel.Writer.WriteAsync(requestId);
        _logger.LogInformation("Queued stock validation for request {RequestId}", requestId);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stock validation background service started");

        await foreach (var requestId in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var validationService = scope.ServiceProvider.GetRequiredService<IStockValidationService>();
                await validationService.ValidateStockAvailabilityAsync(requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock validation for request {RequestId}", requestId);
            }
        }
    }
}
