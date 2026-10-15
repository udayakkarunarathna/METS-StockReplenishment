using METS.StockReplenishment.Core.Enums;

namespace METS.StockReplenishment.Core.Entities;

public class ReplenishmentRequest
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    public RequestPriority Priority { get; set; } = RequestPriority.Normal;
    public string StockLocationCode { get; set; } = string.Empty;
    public string StockLocationName { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? FulfilledAt { get; set; }
    public StockValidationStatus StockValidationStatus { get; set; } = StockValidationStatus.NotChecked;
    public string? StockValidationMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ReplenishmentLineItem> LineItems { get; set; } = new();
}
