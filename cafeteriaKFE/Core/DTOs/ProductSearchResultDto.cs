namespace cafeteriaKFE.Core.DTOs
{
    public sealed class ProductSearchResultDto
    {
        public long ProductId { get; set; }
        public int BarCode { get; set; }
        public string Name { get; set; } = null!;
        public decimal BasePrice { get; set; }
    }
}
