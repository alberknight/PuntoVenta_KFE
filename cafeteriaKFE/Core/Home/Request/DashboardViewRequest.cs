namespace cafeteriaKFE.Core.Home.Request
{
    public class DashboardViewRequest
    {
        public decimal SalesToday { get; set; }          // suma Total de Orders hoy
        public int TicketsToday { get; set; }            // count Orders hoy
        public decimal AvgTicketToday { get; set; }      // SalesToday / TicketsToday
        public int ProductsSoldToday { get; set; }       // sum OrderDetails.Quantity hoy

        public List<TopProductItem> TopProductsAllTime { get; set; } = [];
    }
    public class TopProductItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public int QuantitySold { get; set; }
    }
}
