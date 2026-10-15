namespace METS.StockReplenishment.Core.DTOs;

public class LineItemDto
{
    public int Id { get; set; }
    public string ArticleNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal RequestedQuantity { get; set; }
    public decimal? FulfilledQuantity { get; set; }
}
