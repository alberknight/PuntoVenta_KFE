using cafeteriaKFE.Core.DTOs;
using cafeteriaKFE.Core.Pos.Request;
using cafeteriaKFE.Core.Pos.Response;
using cafeteriaKFE.Core.Orders.Response;
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
            return new PosOptionsResponse
            {
                Sizes = await _catalog.GetSizesAsync(),
                MilkTypes = await _catalog.GetMilkTypesAsync(),
                Temperatures = await _catalog.GetTemperaturesAsync(),
                Syrups = await _catalog.GetSyrupsAsync()
            };
        }

        // Si no tienes costo extra de crema, déjalo en 0.
        private const decimal WHIPPED_CREAM_PRICE = 0m;

        public async Task<decimal> CalculateLineTotalAsync(AddLineRequest line)
        {
            if (line.Quantity <= 0)
                throw new ArgumentException("Quantity inválida.");

            var product = await _products.GetBasicByIdAsync(line.ProductId)
                          ?? throw new InvalidOperationException("Producto no existe o está eliminado.");

            // FOOD: no requiere nada extra
            if (product.ProductTypeId == 5)
            {
                return product.BasePrice * line.Quantity;
            }

            // NO FOOD: requiere Size y MilkType (como definiste)
            if (line.SizeId is null || line.MilkTypeId is null)
                throw new InvalidOperationException("Este producto requiere Size y MilkType.");

            var size = await _catalog.GetSizeByIdAsync(line.SizeId.Value)
                       ?? throw new InvalidOperationException("Size no existe o está eliminado.");

            var milk = await _catalog.GetMilkTypeByIdAsync(line.MilkTypeId.Value)
                       ?? throw new InvalidOperationException("MilkType no existe o está eliminado.");

            decimal unitPrice = product.BasePrice + size.PriceDelta + milk.PriceDelta;

            // ProductType 2: requiere Temperature (normalmente sin costo)
            if (product.ProductTypeId == 3)
            {
                if (line.TemperatureId is null)
                    throw new InvalidOperationException("Este producto requiere Temperature.");

                var temp = await _catalog.GetTemperatureByIdAsync(line.TemperatureId.Value)
                           ?? throw new InvalidOperationException("Temperature no existe o está eliminado.");

                // unitPrice += temp.PriceDelta;
            }

            // ProductType 3: requiere Syrup (puede tener costo)
            if (product.ProductTypeId == 2)
            {
                if (line.SyrupId is null)
                    throw new InvalidOperationException("Este producto requiere Syrup.");

                var syrup = await _catalog.GetSyrupByIdAsync(line.SyrupId.Value)
                            ?? throw new InvalidOperationException("Syrup no existe o está eliminado.");

                // Si tu Syrup tiene PriceDelta, lo sumas. Si no, deja 0.
                unitPrice += syrup.PriceDelta;
            }

            // ProductType 4: crema batida opcional
            if (product.ProductTypeId == 4)
            {
                var hasWhip = line.HasWhippedCream ?? false;
                if (hasWhip)
                {
                    unitPrice += WHIPPED_CREAM_PRICE;
                }
            }

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
            var details = new List<OrderDetailCreateResponse>(request.Lines.Count);

            foreach (var line in request.Lines)
            {
                var lineTotal = await CalculateLineTotalAsync(line);
                subtotal += lineTotal;

                details.Add(new OrderDetailCreateResponse
                {
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    SizeId = line.SizeId,
                    MilkTypeId = line.MilkTypeId,
                    TemperatureId = line.TemperatureId,
                    SyrupId = line.SyrupId,
                    HasWhippedCream = line.HasWhippedCream ?? false
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
