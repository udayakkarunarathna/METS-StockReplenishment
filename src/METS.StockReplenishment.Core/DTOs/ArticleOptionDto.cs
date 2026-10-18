using System.ComponentModel.DataAnnotations;

namespace METS.StockReplenishment.Core.DTOs;

public class ArticleOptionDto
{
	public string ArticleNumber { get; set; } = string.Empty;

	[StringLength(500)]
	public string Description { get; set; } = string.Empty;
}