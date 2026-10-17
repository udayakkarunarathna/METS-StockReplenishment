using METS.StockReplenishment.Core.DTOs;
using METS.StockReplenishment.Core.Entities;
using METS.StockReplenishment.Core.Enums;
using METS.StockReplenishment.Core.Exceptions;
using METS.StockReplenishment.Core.Interfaces;

namespace METS.StockReplenishment.Infrastructure.Services;

public class ReplenishmentRequestService : IReplenishmentRequestService
{
    private readonly IReplenishmentRequestRepository _repository;
    private readonly IStockValidationQueue _validationQueue;

    public ReplenishmentRequestService(
        IReplenishmentRequestRepository repository,
        IStockValidationQueue validationQueue)
    {
        _repository = repository;
        _validationQueue = validationQueue;
    }

    public async Task<ReplenishmentRequestDto> GetByIdAsync(int id)
    {
        var request = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("ReplenishmentRequest", id);
        return MapToDto(request);
    }

    public async Task<PagedResultDto<ReplenishmentRequestDto>> GetFilteredAsync(RequestFilterDto filter)
    {
        // Validate pagination parameters
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 10;
        if (filter.PageSize > 50) filter.PageSize = 50;

        var result = await _repository.GetFilteredAsync(filter);
        return new PagedResultDto<ReplenishmentRequestDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<ReplenishmentRequestDto> CreateAsync(CreateReplenishmentRequestDto dto)
    {
        var requestNumber = await _repository.GenerateRequestNumberAsync();
        var now = DateTime.UtcNow;

        var request = new ReplenishmentRequest
        {
            RequestNumber = requestNumber,
            Status = RequestStatus.Draft,
            Priority = dto.Priority,
            StockLocationCode = dto.StockLocationCode,
            StockLocationName = dto.StockLocationName,
            RequestedBy = dto.RequestedBy,
            RequestedAt = now,
            StockValidationStatus = StockValidationStatus.NotChecked,
            CreatedAt = now,
            UpdatedAt = now,
            LineItems = dto.LineItems.Select(li => new ReplenishmentLineItem
            {
                ArticleNumber = li.ArticleNumber,
                Description = li.Description,
                RequestedQuantity = li.RequestedQuantity
            }).ToList()
        };

        var created = await _repository.AddAsync(request);
        return MapToDto(created);
    }

    public async Task<ReplenishmentRequestDto> UpdateAsync(int id, UpdateReplenishmentRequestDto dto)
    {
        var request = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("ReplenishmentRequest", id);

        if (request.Status != RequestStatus.Draft)
            throw new BusinessRuleException("Only draft requests can be updated.");

        request.StockLocationCode = dto.StockLocationCode;
        request.StockLocationName = dto.StockLocationName;
        request.Priority = dto.Priority;
        request.UpdatedAt = DateTime.UtcNow;

        // Replace line items
        request.LineItems.Clear();
        request.LineItems = dto.LineItems.Select(li => new ReplenishmentLineItem
        {
            ReplenishmentRequestId = id,
            ArticleNumber = li.ArticleNumber,
            Description = li.Description,
            RequestedQuantity = li.RequestedQuantity
        }).ToList();

        await _repository.UpdateAsync(request);
        return MapToDto(request);
    }

    public async Task<ReplenishmentRequestDto> SubmitAsync(int id)
    {
        var request = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("ReplenishmentRequest", id);

        if (request.Status != RequestStatus.Draft)
            throw new BusinessRuleException("Only draft requests can be submitted.");

        if (!request.LineItems.Any())
            throw new BusinessRuleException("Cannot submit a request with no line items.");

        request.Status = RequestStatus.Submitted;
        request.SubmittedAt = DateTime.UtcNow;
        request.StockValidationStatus = StockValidationStatus.Pending;
        request.StockValidationMessage = "Stock validation queued...";
        request.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(request);

        // Queue async stock validation (non-blocking)
        await _validationQueue.QueueValidationAsync(id);

        return MapToDto(request);
    }

    public async Task<ReplenishmentRequestDto> ApproveAsync(int id, ApproveRequestDto dto)
    {
        var request = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("ReplenishmentRequest", id);

        if (request.Status != RequestStatus.Submitted)
            throw new BusinessRuleException("Only submitted requests can be approved.");

        request.Status = RequestStatus.Approved;
        request.ReviewedBy = dto.ReviewedBy;
        request.ReviewedAt = DateTime.UtcNow;
        request.RejectionReason = string.IsNullOrWhiteSpace(dto.Reason) ? null : dto.Reason.Trim();
        request.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(request);
        return MapToDto(request);
    }

    public async Task<ReplenishmentRequestDto> RejectAsync(int id, RejectRequestDto dto)
    {
        var request = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("ReplenishmentRequest", id);

        if (request.Status != RequestStatus.Submitted)
            throw new BusinessRuleException("Only submitted requests can be rejected.");

        request.Status = RequestStatus.Rejected;
        request.ReviewedBy = dto.ReviewedBy;
        request.ReviewedAt = DateTime.UtcNow;
        request.RejectionReason = dto.Reason.Trim();
        request.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(request);
        return MapToDto(request);
    }

    public async Task<ReplenishmentRequestDto> FulfillAsync(int id, FulfillRequestDto dto)
    {
        var request = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("ReplenishmentRequest", id);

        if (request.Status != RequestStatus.Approved)
            throw new BusinessRuleException("Only approved requests can be fulfilled.");

        foreach (var item in dto.Items)
        {
            var lineItem = request.LineItems.FirstOrDefault(li => li.Id == item.LineItemId)
                ?? throw new NotFoundException("LineItem", item.LineItemId);

            if (item.FulfilledQuantity > lineItem.RequestedQuantity)
                throw new BusinessRuleException(
                    $"Fulfilled quantity ({item.FulfilledQuantity}) cannot exceed requested quantity ({lineItem.RequestedQuantity}) for item {lineItem.ArticleNumber}.");

            lineItem.FulfilledQuantity = item.FulfilledQuantity;
        }

        request.Status = RequestStatus.Fulfilled;
        request.FulfilledAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(request);
        return MapToDto(request);
    }

    public async Task<StockValidationResultDto> GetStockValidationStatusAsync(int id)
    {
        var request = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("ReplenishmentRequest", id);

        return new StockValidationResultDto
        {
            RequestId = request.Id,
            Status = request.StockValidationStatus.ToString(),
            Message = request.StockValidationMessage
        };
    }

    public async Task<List<string>> GetDistinctLocationsAsync()
    {
        return await _repository.GetDistinctLocationsAsync();
    }

    public async Task<List<StockLocationOptionDto>> GetDistinctLocationOptionsAsync()
    {
        return await _repository.GetDistinctLocationOptionsAsync();
    }

    public async Task<List<ArticleOptionDto>> GetDistinctArticleOptionsAsync()
    {
        return await _repository.GetDistinctArticleOptionsAsync();
    }

    private static ReplenishmentRequestDto MapToDto(ReplenishmentRequest request)
    {
        return new ReplenishmentRequestDto
        {
            Id = request.Id,
            RequestNumber = request.RequestNumber,
            Status = request.Status.ToString(),
            Priority = request.Priority.ToString(),
            StockLocationCode = request.StockLocationCode,
            StockLocationName = request.StockLocationName,
            RequestedBy = request.RequestedBy,
            RequestedAt = request.RequestedAt,
            SubmittedAt = request.SubmittedAt,
            ReviewedBy = request.ReviewedBy,
            ReviewedAt = request.ReviewedAt,
            RejectionReason = request.RejectionReason,
            FulfilledAt = request.FulfilledAt,
            StockValidationStatus = request.StockValidationStatus.ToString(),
            StockValidationMessage = request.StockValidationMessage,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            LineItems = request.LineItems.Select(li => new LineItemDto
            {
                Id = li.Id,
                ArticleNumber = li.ArticleNumber,
                Description = li.Description,
                RequestedQuantity = li.RequestedQuantity,
                FulfilledQuantity = li.FulfilledQuantity
            }).ToList()
        };
    }
}
