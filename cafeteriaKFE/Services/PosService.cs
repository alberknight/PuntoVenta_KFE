using cafeteriaKFE.Core.DTOs;
using cafeteriaKFE.Core.Pos.Request;
using cafeteriaKFE.Core.Pos.Response;
using cafeteriaKFE.Repository.Catalogs;
using cafeteriaKFE.Repository.Orders;
using Microsoft.AspNetCore.Cors.Infrastructure;
using cafeteriaKFE.Repository.Products;

namespace cafeteriaKFE.Services
{
    public interface IPosService
    {
        Task<List<ProductSearchResultDto>> SearchProductsAsync(string query, int take = 10);
        Task<PosOptionsResponse> GetOptionsAsync();

        Task<decimal> CalculateLineTotalAsync(AddLineRequest line);
        Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request);
    }

    public class PosService : IPosService
    {
        private readonly IProductRepository _products;
        private readonly IPosCatalogRepository _catalog;
        private readonly IOrderRepository _orders;

        public PosService(IProductRepository products, IPosCatalogRepository catalog, IOrderRepository orders)
        {
            _products = products;
            _catalog = catalog;
            _orders = orders;
        }

        public Task<List<ProductSearchResultDto>> SearchProductsAsync(string query, int take = 10)
            => _products.SearchAsync(query, take);

        public async Task<PosOptionsResponse> GetOptionsAsync()
        {
            var sizes = await _catalog.GetSizesAsync();
            var milks = await _catalog.GetMilkTypesAsync();

            return new PosOptionsResponse
            {
                Sizes = sizes,
                MilkTypes = milks
            };
        }

        public async Task<decimal> CalculateLineTotalAsync(AddLineRequest line)
        {
            if (line.Quantity <= 0) throw new ArgumentException("Quantity inválida.");

            var product = await _products.GetBasicByIdAsync(line.ProductId)
                          ?? throw new InvalidOperationException("Producto no existe o está eliminado.");

            var size = await _catalog.GetSizeByIdAsync(line.SizeId)
                       ?? throw new InvalidOperationException("Size no existe o está eliminado.");

            var milk = await _catalog.GetMilkTypeByIdAsync(line.MilkTypeId)
                       ?? throw new InvalidOperationException("MilkType no existe o está eliminado.");

            var unitPrice = product.BasePrice + size.PriceDelta + milk.PriceDelta;
            return unitPrice * line.Quantity;
        }

        public async Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request)
        {
            if (request.Lines.Count == 0)
                throw new InvalidOperationException("No hay productos en la venta.");

            // Validar PaidMethodId aquí si quieres (tabla PaidMethods)
            // (por ahora asumimos que el front manda un ID válido)

            decimal subtotal = 0;

            // Construimos detalles y calculamos totales
            var details = new List<OrderDetailCreate>(request.Lines.Count);

            foreach (var line in request.Lines)
            {
                var lineTotal = await CalculateLineTotalAsync(line);
                subtotal += lineTotal;

                details.Add(new OrderDetailCreate
                {
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    SizeId = line.SizeId,
                    MilkTypeId = line.MilkTypeId
                });
            }

            var totalAmount = subtotal; // por ahora sin impuestos/desc

            var orderId = await _orders.CreateOrderAsync(
                subtotal: subtotal,
                totalAmount: totalAmount,
                isDelivery: request.IsDelivery,
                paidMethodId: request.PaidMethodId,
                details: details);

            return new CheckoutResponse
            {
                OrderId = orderId,
                Subtotal = subtotal,
                TotalAmount = totalAmount
            };
        }
    }
}
