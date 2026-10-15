namespace METS.StockReplenishment.Core.Interfaces;

public interface IStockValidationService
{
    Task ValidateStockAvailabilityAsync(int requestId);
}
