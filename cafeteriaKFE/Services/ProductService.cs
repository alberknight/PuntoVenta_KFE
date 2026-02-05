using cafeteriaKFE.Core.Products.Request;
using cafeteriaKFE.Core.Products.Response;
using cafeteriaKFE.Models;
using cafeteriaKFE.Repository;
using cafeteriaKFE.Repository.Products;
using Microsoft.AspNetCore.Identity;

namespace cafeteriaKFE.Services;
public interface IProductService
{
    Task<List<GetProductsRequest>> GetAllAsync();
    Task<GetProductsRequest?> GetByIdAsync(int id);

    Task<IdentityResult> CreateAsync(CreateProductResponse dto);
    Task<IdentityResult> UpdateAsync(UpdateProductResponse dto);
    Task<IdentityResult> SoftDeleteAsync(int id);
}
public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<GetProductsRequest>> GetAllAsync()
    {
        var products = await _repo.GetAllAsync();

        return products.Select(p => new GetProductsRequest
        {
            ProductId = p.ProductId,
            ProductTypeId = p.ProductTypeId,
            ProductTypeName = p.ProductType?.Name ?? "",
            Name = p.Name,
            BarCode = p.BarCode,
            BasePrice = p.BasePrice,
            CreatedAt = p.CreatedAt
        }).ToList();
    }

    public async Task<GetProductsRequest?> GetByIdAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id);
        if (p is null) return null;

        return new GetProductsRequest
        {
            ProductId = p.ProductId,
            ProductTypeId = p.ProductTypeId,
            ProductTypeName = p.ProductType?.Name ?? "",
            Name = p.Name,
            BarCode = p.BarCode,
            BasePrice = p.BasePrice,
            CreatedAt = p.CreatedAt
        };
    }

    public async Task<IdentityResult> CreateAsync(CreateProductResponse dto)
    {
        // Regla de negocio real
        if (await _repo.ExistsBarCodeAsync(dto.BarCode))
        {
            return IdentityResult.Failed(
                new IdentityError { Description = "Ya existe un producto con ese código de barras." }
            );
        }

        var now = DateTime.UtcNow;

        var product = new Product
        {
            ProductTypeId = dto.ProductTypeId,
            Name = dto.Name.Trim(),
            BarCode = dto.BarCode,
            BasePrice = dto.BasePrice,
            CreatedAt = now,
            UpdatedAt = now,
            Deleted = false
        };

        await _repo.AddAsync(product);
        await _repo.SaveChangesAsync();

        return IdentityResult.Success;
    }


    public async Task<IdentityResult> UpdateAsync(UpdateProductResponse dto)
    {
        var product = await _repo.GetByIdAsync(dto.ProductId);
        if (product is null)
        {
            return IdentityResult.Failed(
                new IdentityError { Description = "Producto no encontrado." }
            );
        }

        // Regla de negocio real
        if (await _repo.ExistsBarCodeAsync(dto.BarCode, dto.ProductId))
        {
            return IdentityResult.Failed(
                new IdentityError { Description = "Ya existe otro producto con ese código de barras." }
            );
        }

        product.ProductTypeId = dto.ProductTypeId;
        product.Name = dto.Name.Trim();
        product.BarCode = dto.BarCode;
        product.BasePrice = dto.BasePrice;
        product.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(product);
        await _repo.SaveChangesAsync();

        return IdentityResult.Success;
    }


    public async Task<IdentityResult> SoftDeleteAsync(int id)
    {
        if (id <= 0)
            return IdentityResult.Failed(new IdentityError { Description = "Id inválido." });

        var product = await _repo.GetByIdAsync(id);
        if (product is null)
            return IdentityResult.Failed(new IdentityError { Description = "Producto no encontrado." });

        await _repo.SoftDeleteAsync(product);
        await _repo.SaveChangesAsync();

        return IdentityResult.Success;
    }
}
