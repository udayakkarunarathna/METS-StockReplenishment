using System.ComponentModel.DataAnnotations;

namespace METS.StockReplenishment.Core.DTOs;

public class ApproveRequestDto
{
    [StringLength(100)]
    public string ReviewedBy { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Reason { get; set; }
}
