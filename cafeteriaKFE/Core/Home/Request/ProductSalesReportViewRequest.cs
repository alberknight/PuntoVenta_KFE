namespace cafeteriaKFE.Core.Home.Request
{
    public class ProductSalesReportViewRequest
    {
        public string Mode { get; set; } = "top";
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public List<ProductSalesReportItem> Items { get; set; } = [];
    }
    public class ProductSalesReportItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public int QuantitySold { get; set; }
        public decimal TotalSold { get; set; }
    }
}
