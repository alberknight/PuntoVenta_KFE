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
        Task<List<ProductSalesReportItem>> GetProductsSalesReportAsync(
            DateTime startDate,
            DateTime endDate,
            bool least,
            int take);
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
        public async Task<List<ProductSalesReportItem>> GetProductsSalesReportAsync(
    DateTime startDate, DateTime endDate, bool least, int take)
        {
            // Traemos lo necesario para recalcular el total vendido por línea
            var lines = await (
                from od in _db.OrderDetails.AsNoTracking()
                join o in _db.Orders.AsNoTracking() on od.OrderId equals o.OrderId
                join p in _db.Products.AsNoTracking() on od.ProductId equals p.ProductId
                where !od.Deleted && !o.Deleted && !p.Deleted
                   && o.CreatedAt >= startDate && o.CreatedAt < endDate
                select new
                {
                    p.ProductId,
                    p.Name,
                    p.BasePrice,
                    od.Quantity,
                    od.SizeId,
                    od.MilkTypeId
                }
            ).ToListAsync();

            if (lines.Count == 0) return new List<ProductSalesReportItem>();

            // Caches para deltas (evita N+1)
            var sizeIds = lines.Where(x => x.SizeId != null).Select(x => x.SizeId!.Value).Distinct().ToList();
            var milkIds = lines.Where(x => x.MilkTypeId != null).Select(x => x.MilkTypeId!.Value).Distinct().ToList();

            var sizeDelta = await _db.Sizes.AsNoTracking()
                .Where(s => !s.Deleted && sizeIds.Contains(s.SizeId))
                .ToDictionaryAsync(s => s.SizeId, s => s.PriceDelta);

            var milkDelta = await _db.MilkTypes.AsNoTracking()
                .Where(m => !m.Deleted && milkIds.Contains(m.MilkTypeId))
                .ToDictionaryAsync(m => m.MilkTypeId, m => m.PriceDelta);

            var grouped = lines
                .GroupBy(x => new { x.ProductId, x.Name })
                .Select(g =>
                {
                    int qty = g.Sum(x => x.Quantity);

                    decimal total = g.Sum(x =>
                    {
                        var sDelta = (x.SizeId != null && sizeDelta.TryGetValue(x.SizeId.Value, out var sd)) ? sd : 0m;
                        var mDelta = (x.MilkTypeId != null && milkDelta.TryGetValue(x.MilkTypeId.Value, out var md)) ? md : 0m;
                        var unit = x.BasePrice + sDelta + mDelta;
                        return unit * x.Quantity;
                    });

                    return new ProductSalesReportItem
                    {
                        ProductId = g.Key.ProductId,
                        Name = g.Key.Name,
                        QuantitySold = qty,
                        TotalSold = total
                    };
                });

            var ordered = least
                ? grouped.OrderBy(x => x.QuantitySold).ThenBy(x => x.Name)
                : grouped.OrderByDescending(x => x.QuantitySold).ThenBy(x => x.Name);

            return ordered.Take(take).ToList();
        }

    }
}
