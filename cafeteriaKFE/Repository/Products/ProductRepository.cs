using cafeteriaKFE.Core.DTOs;
using cafeteriaKFE.Data;
using Microsoft.EntityFrameworkCore;

namespace cafeteriaKFE.Repository.Products
{
    public interface IProductRepository
    {
        Task<List<ProductSearchResultDto>> SearchAsync(string query, int take = 10);
        Task<ProductSearchResultDto?> GetBasicByIdAsync(long productId);

        // Utilidad: obtener precio base + barcode + nombre
    }
    public class ProductRepository : IProductRepository
    {
        private readonly PosDbContext _db;

        public ProductRepository(PosDbContext db)
        {
            _db = db;
        }

        public async Task<List<ProductSearchResultDto>> SearchAsync(string query, int take = 10)
        {
            query = (query ?? "").Trim();
            if (query.Length == 0) return [];

            var products = _db.Products.AsNoTracking().Where(p => !p.Deleted);

            // Si es numérico: buscar por BarCode
            if (int.TryParse(query, out var barCode))
            {
                return await products
                    .Where(p => p.BarCode == barCode)
                    .Select(p => new ProductSearchResultDto
                    {
                        ProductId = p.ProductId,
                        BarCode = p.BarCode,
                        Name = p.Name,
                        BasePrice = p.BasePrice
                    })
                    .Take(take)
                    .ToListAsync();
            }

            // Si es texto: buscar por nombre
            return await products
                .Where(p => EF.Functions.Like(p.Name, $"%{query}%"))
                .OrderBy(p => p.Name)
                .Select(p => new ProductSearchResultDto
                {
                    ProductId = p.ProductId,
                    BarCode = p.BarCode,
                    Name = p.Name,
                    BasePrice = p.BasePrice
                })
                .Take(take)
                .ToListAsync();
        }

        public async Task<ProductSearchResultDto?> GetBasicByIdAsync(long productId)
        {
            return await _db.Products.AsNoTracking()
                .Where(p => p.ProductId == productId && !p.Deleted)
                .Select(p => new ProductSearchResultDto
                {
                    ProductId = p.ProductId,
                    BarCode = p.BarCode,
                    Name = p.Name,
                    BasePrice = p.BasePrice
                })
                .FirstOrDefaultAsync();
        }
    }
}
