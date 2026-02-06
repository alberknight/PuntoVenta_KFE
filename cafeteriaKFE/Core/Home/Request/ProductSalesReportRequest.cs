namespace cafeteriaKFE.Core.Home.Request
{
    public class ProductSalesReportRequest
    {
        public string Mode { get; set; } = "top"; // "top" o "low"
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
