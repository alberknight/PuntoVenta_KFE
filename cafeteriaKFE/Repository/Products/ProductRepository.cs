using cafeteriaKFE.Core.DTOs;
using cafeteriaKFE.Data;
using cafeteriaKFE.Models;
using Microsoft.EntityFrameworkCore;

namespace cafeteriaKFE.Repository.Products
{
    public interface IProductRepository
    {
        Task<List<ProductSearchResultDto>> SearchAsync(string query, int take = 10);
        Task<ProductSearchResultDto?> GetBasicByIdAsync(long productId);
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);

        Task<bool> ExistsBarCodeAsync(int barCode, int? excludeProductId = null);

        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task SoftDeleteAsync(Product product);
        Task SaveChangesAsync();

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
                        BasePrice = p.BasePrice,
                        ProductTypeId = p.ProductTypeId
                    })
                    .Take(take)
                    .ToListAsync();
            }

            // Si es texto: buscar por nombre
            return await products
                .Where(p => EF.Functions.Like(
        EF.Functions.Collate(p.Name, "Latin1_General_CI_AI"),
        $"%{query}%"))
                .OrderBy(p => p.Name)
                .Select(p => new ProductSearchResultDto
                {
                    ProductId = p.ProductId,
                    BarCode = p.BarCode,
                    Name = p.Name,
                    BasePrice = p.BasePrice,
                    ProductTypeId = p.ProductTypeId
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
                    BasePrice = p.BasePrice,
                    ProductTypeId = p.ProductTypeId,
                })
                .FirstOrDefaultAsync();
        }
        public Task<List<Product>> GetAllAsync()
        {
            return _db.Products
                .Include(p => p.ProductType)
                .Where(p => !p.Deleted && !p.ProductType.Deleted)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public Task<Product?> GetByIdAsync(int id)
        {
            return _db.Products
                .Include(p => p.ProductType)
                .Where(p => p.ProductId == id && !p.Deleted)
                .FirstOrDefaultAsync();
        }

        public Task<bool> ExistsBarCodeAsync(int barCode, int? excludeProductId = null)
        {
            var q = _db.Products.AsQueryable().Where(p => !p.Deleted && p.BarCode == barCode);

            if (excludeProductId.HasValue)
                q = q.Where(p => p.ProductId != excludeProductId.Value);

            return q.AnyAsync();
        }

        public Task AddAsync(Product product)
        {
            _db.Products.Add(product);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Product product)
        {
            _db.Products.Update(product);
            return Task.CompletedTask;
        }

        public Task SoftDeleteAsync(Product product)
        {
            product.Deleted = true;
            product.UpdatedAt = DateTime.UtcNow;
            _db.Products.Update(product);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
