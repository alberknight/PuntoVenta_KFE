using cafeteriaKFE.Data;
using cafeteriaKFE.Models;
using cafeteriaKFE.Core.Orders.Response;

namespace cafeteriaKFE.Repository.Orders
{
    public interface IOrderRepository
    {
        Task<long> CreateOrderAsync(
            decimal subtotal,
            decimal totalAmount,
            bool isDelivery,
            int paidMethodId,
            List<OrderDetailCreateResponse> details);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly PosDbContext _db;

        public OrderRepository(PosDbContext db)
        {
            _db = db;
        }

        public async Task<long> CreateOrderAsync(
            decimal subtotal,
            decimal totalAmount,
            bool isDelivery,
            int paidMethodId,
            List<OrderDetailCreateResponse> details)
        {
            if (details.Count == 0)
                throw new InvalidOperationException("No hay productos para registrar.");

            await using var tx = await _db.Database.BeginTransactionAsync();

            var now = DateTime.UtcNow;

            var order = new Order
            {
                Subtotal = subtotal,
                TotalAmount = totalAmount,
                IsDelivery = isDelivery,
                PaidMethodId = paidMethodId,
                CreatedAt = now,
                UpdatedAt = now,
                Deleted = false
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            var orderDetails = details.Select(d => new OrderDetail
            {
                OrderId = order.OrderId,
                ProductId = d.ProductId,
                Quantity = d.Quantity,
                SizeId = d.SizeId,
                MilkTypeId = d.MilkTypeId,
                TemperatureId = d.TemperatureId,
                SyrupId = d.SyrupId,
                HasWhippedCream = d.HasWhippedCream,
                CreatedAt = now,
                UpdatedAt = now,
                Deleted = false
            }).ToList();

            _db.OrderDetails.AddRange(orderDetails);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();
            return order.OrderId;
        }
    }
}
