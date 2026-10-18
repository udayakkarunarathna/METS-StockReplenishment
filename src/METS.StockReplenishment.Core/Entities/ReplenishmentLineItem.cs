namespace METS.StockReplenishment.Core.Entities;

public class ReplenishmentLineItem
{
	public int Id { get; set; }
	public int ReplenishmentRequestId { get; set; }
	public string ArticleNumber { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public decimal RequestedQuantity { get; set; }
	public decimal? FulfilledQuantity { get; set; }

	public ReplenishmentRequest ReplenishmentRequest { get; set; } = null!;
}