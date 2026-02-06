using cafeteriaKFE.Core.Home.Request;
using cafeteriaKFE.Data;
using Microsoft.EntityFrameworkCore;

namespace cafeteriaKFE.Repository.Home
{
    public interface IDashboardRepository
    {
        Task<decimal> GetSalesTotalAsync(DateTime from, DateTime to);
        Task<int> GetTicketsCountAsync(DateTime from, DateTime to);
        Task<int> GetProductsSoldAsync(DateTime from, DateTime to);

        Task<List<TopProductItem>> GetTopProductsAllTimeAsync(int top);
    }
    public class DashboardRepository : IDashboardRepository
    {
        private readonly PosDbContext _db;

        public DashboardRepository(PosDbContext db)
        {
            _db = db;
        }

        public async Task<decimal> GetSalesTotalAsync(DateTime from, DateTime to)
        {
            return await _db.Orders.AsNoTracking()
                .Where(o => !o.Deleted && o.CreatedAt >= from && o.CreatedAt < to)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
        }

        public async Task<int> GetTicketsCountAsync(DateTime from, DateTime to)
        {
            return await _db.Orders.AsNoTracking()
                .Where(o => !o.Deleted && o.CreatedAt >= from && o.CreatedAt < to)
                .CountAsync();
        }

        public async Task<int> GetProductsSoldAsync(DateTime startDate, DateTime endDate)
        {
            return await (
                from od in _db.OrderDetails.AsNoTracking()
                join o in _db.Orders.AsNoTracking() on od.OrderId equals o.OrderId
                where !od.Deleted
                   && !o.Deleted
                   && o.CreatedAt >= startDate
                   && o.CreatedAt < endDate
                select (int?)od.Quantity
            ).SumAsync() ?? 0;
        }


        public async Task<List<TopProductItem>> GetTopProductsAllTimeAsync(int top)
        {
            return await (
                from od in _db.OrderDetails.AsNoTracking()
                join p in _db.Products.AsNoTracking() on od.ProductId equals p.ProductId
                join o in _db.Orders.AsNoTracking() on od.OrderId equals o.OrderId
                where !od.Deleted && !p.Deleted && !o.Deleted
                group od by new { p.ProductId, p.Name } into g
                orderby g.Sum(x => x.Quantity) descending
                select new TopProductItem
                {
                    ProductId = g.Key.ProductId,
                    Name = g.Key.Name,
                    QuantitySold = g.Sum(x => x.Quantity)
                }
            ).Take(top).ToListAsync();
        }
    }
}
