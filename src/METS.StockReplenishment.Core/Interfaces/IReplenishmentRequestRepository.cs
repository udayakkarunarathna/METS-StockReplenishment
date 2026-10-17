using METS.StockReplenishment.Core.DTOs;
using METS.StockReplenishment.Core.Entities;
using METS.StockReplenishment.Core.Enums;

namespace METS.StockReplenishment.Core.Interfaces;

public interface IReplenishmentRequestRepository
{
    Task<ReplenishmentRequest?> GetByIdAsync(int id);
    Task<PagedResultDto<ReplenishmentRequest>> GetFilteredAsync(RequestFilterDto filter);
    Task<ReplenishmentRequest> AddAsync(ReplenishmentRequest request);
    Task UpdateAsync(ReplenishmentRequest request);
    Task<string> GenerateRequestNumberAsync();
    Task<List<string>> GetDistinctLocationsAsync();
    Task<List<StockLocationOptionDto>> GetDistinctLocationOptionsAsync();
    Task<List<ArticleOptionDto>> GetDistinctArticleOptionsAsync();
    Task<List<StockBalanceDto>> GetStockBalancesAsync();
}
