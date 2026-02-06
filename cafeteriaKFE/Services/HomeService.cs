using cafeteriaKFE.Repository.Home;
using cafeteriaKFE.Core.Home.Request;

namespace cafeteriaKFE.Services
{
    public interface IHomeService
    {
        Task<DashboardViewRequest> GetDashboardAsync();
        Task<ProductSalesReportViewRequest> GetProductsSalesReportAsync(ProductSalesReportRequest req);
    }
    public class HomeService : IHomeService
    {
        private readonly IDashboardRepository _repo;

        public HomeService(IDashboardRepository repo)
        {
            _repo = repo;
        }

        public async Task<DashboardViewRequest> GetDashboardAsync()
        {
            // Hoy (hora del servidor). Si tu BD guarda UTC, cambia a UtcNow.Date.
            var tiempo = DateTime.UtcNow.Date.ToUniversalTime(); // Keep user's time calculation for main KPIs
            var from = tiempo.AddDays(-3);
            var to = tiempo.AddDays(3);

            var sales = await _repo.GetSalesTotalAsync(from, to);
            var tickets = await _repo.GetTicketsCountAsync(from, to);
            var productsSold = await _repo.GetProductsSoldAsync(from, to);

            // Calculate dates specifically for "today's" chart based on user's request
            var tiempoChart = DateTime.UtcNow.Date.ToUniversalTime();
            var fromChart = tiempoChart.AddDays(-2);
            var toChart = tiempoChart.AddDays(2);
            var productsSoldForChart = await _repo.GetProductsSoldTodayForChartAsync(fromChart, toChart);


            return new DashboardViewRequest
            {
                SalesToday = sales,
                TicketsToday = tickets,
                AvgTicketToday = tickets > 0 ? Math.Round(sales / tickets, 2) : 0m,
                ProductsSoldToday = productsSold,
                TopProductsAllTime = await _repo.GetTopProductsAllTimeAsync(3),
                ProductsSoldTodayForChart = productsSoldForChart // Populated here
            };
        }
        public async Task<ProductSalesReportViewRequest> GetProductsSalesReportAsync(ProductSalesReportRequest req)
        {
            // defaults
            var start = (req.From ?? DateTime.Today.AddDays(-7)).Date;
            var end = ((req.To ?? DateTime.Today).Date).AddDays(1); // inclusivo

            var least = string.Equals(req.Mode, "low", StringComparison.OrdinalIgnoreCase);

            var items = await _repo.GetProductsSalesReportAsync(start, end, least, take: 100);

            return new ProductSalesReportViewRequest
            {
                Mode = least ? "low" : "top",
                From = start,
                To = end.AddDays(-1), // para que el input muestre la fecha final real
                Items = items
            };
        }

    }
}
