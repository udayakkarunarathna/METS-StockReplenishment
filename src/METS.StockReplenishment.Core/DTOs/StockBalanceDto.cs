namespace METS.StockReplenishment.Core.DTOs;

public class StockBalanceDto
{
	public string ArticleNumber { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public decimal CurrentBalance { get; set; }
	public decimal LowStockThreshold { get; set; }
	public bool IsLowStock => CurrentBalance <= LowStockThreshold;
}