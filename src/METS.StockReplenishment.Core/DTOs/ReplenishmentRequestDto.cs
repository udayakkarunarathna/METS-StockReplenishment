using METS.StockReplenishment.Core.Enums;

namespace METS.StockReplenishment.Core.DTOs;

public class ReplenishmentRequestDto
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string StockLocationCode { get; set; } = string.Empty;
    public string StockLocationName { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? FulfilledAt { get; set; }
    public string StockValidationStatus { get; set; } = string.Empty;
    public string? StockValidationMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<LineItemDto> LineItems { get; set; } = new();
}
