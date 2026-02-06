using cafeteriaKFE.Repository.Home;
using cafeteriaKFE.Core.Home.Request;

namespace cafeteriaKFE.Services
{
    public interface IHomeService
    {
        Task<DashboardViewRequest> GetDashboardAsync();
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
            var from = DateTime.Today;
            var to = from.AddDays(1);

            var sales = await _repo.GetSalesTotalAsync(from, to);
            var tickets = await _repo.GetTicketsCountAsync(from, to);
            var productsSold = await _repo.GetProductsSoldAsync(from, to);

            return new DashboardViewRequest
            {
                SalesToday = sales,
                TicketsToday = tickets,
                AvgTicketToday = tickets > 0 ? Math.Round(sales / tickets, 2) : 0m,
                ProductsSoldToday = productsSold,
                TopProductsAllTime = await _repo.GetTopProductsAllTimeAsync(3)
            };
        }
    }
}
