using System.ComponentModel.DataAnnotations;

namespace METS.StockReplenishment.Core.DTOs;

public class CreateLineItemDto
{
    [Required]
    [StringLength(50)]
    public string ArticleNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Requested quantity must be greater than zero.")]
    public decimal RequestedQuantity { get; set; }
}
