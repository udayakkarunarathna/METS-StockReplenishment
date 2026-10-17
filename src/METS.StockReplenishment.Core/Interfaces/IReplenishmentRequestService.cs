using METS.StockReplenishment.Core.DTOs;

namespace METS.StockReplenishment.Core.Interfaces;

public interface IReplenishmentRequestService
{
    Task<ReplenishmentRequestDto> GetByIdAsync(int id);
    Task<PagedResultDto<ReplenishmentRequestDto>> GetFilteredAsync(RequestFilterDto filter);
    Task<ReplenishmentRequestDto> CreateAsync(CreateReplenishmentRequestDto dto);
    Task<ReplenishmentRequestDto> UpdateAsync(int id, UpdateReplenishmentRequestDto dto);
    Task<ReplenishmentRequestDto> SubmitAsync(int id);
    Task<ReplenishmentRequestDto> ApproveAsync(int id, ApproveRequestDto dto);
    Task<ReplenishmentRequestDto> RejectAsync(int id, RejectRequestDto dto);
    Task<ReplenishmentRequestDto> FulfillAsync(int id, FulfillRequestDto dto);
    Task<StockValidationResultDto> GetStockValidationStatusAsync(int id);
    Task<List<string>> GetDistinctLocationsAsync();
    Task<List<StockLocationOptionDto>> GetDistinctLocationOptionsAsync();
    Task<List<ArticleOptionDto>> GetDistinctArticleOptionsAsync();
    Task<List<StockBalanceDto>> GetStockBalancesAsync();
}
