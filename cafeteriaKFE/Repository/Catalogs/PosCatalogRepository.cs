using cafeteriaKFE.Core.DTOs;
using cafeteriaKFE.Data;
using Microsoft.EntityFrameworkCore;

namespace cafeteriaKFE.Repository.Catalogs
{
    public interface IPosCatalogRepository
    {
        Task<List<OptionDto>> GetSizesAsync();
        Task<List<OptionDto>> GetMilkTypesAsync();

        Task<OptionDto?> GetSizeByIdAsync(int sizeId);
        Task<OptionDto?> GetMilkTypeByIdAsync(int milkTypeId);
        Task<List<OptionDto>> GetTemperaturesAsync();
        Task<List<OptionDto>> GetSyrupsAsync();
        Task<OptionDto?> GetTemperatureByIdAsync(int id);
        Task<OptionDto?> GetSyrupByIdAsync(int id);

    }

    public class PosCatalogRepository : IPosCatalogRepository
    {
        private readonly PosDbContext _db;

        public PosCatalogRepository(PosDbContext db)
        {
            _db = db;
        }

        public async Task<List<OptionDto>> GetSizesAsync()
        {
            return await _db.Sizes.AsNoTracking()
                .Where(s => !s.Deleted)
                .OrderBy(s => s.Name)
                .Select(s => new OptionDto
                {
                    Id = s.SizeId,
                    Name = s.Name,
                    PriceDelta = s.PriceDelta
                })
                .ToListAsync();
        }

        public async Task<List<OptionDto>> GetMilkTypesAsync()
        {
            return await _db.MilkTypes.AsNoTracking()
                .Where(m => !m.Deleted)
                .OrderBy(m => m.Name)
                .Select(m => new OptionDto
                {
                    Id = m.MilkTypeId,
                    Name = m.Name,
                    PriceDelta = m.PriceDelta
                })
                .ToListAsync();
        }

        public async Task<OptionDto?> GetSizeByIdAsync(int sizeId)
        {
            return await _db.Sizes.AsNoTracking()
                .Where(s => s.SizeId == sizeId && !s.Deleted)
                .Select(s => new OptionDto
                {
                    Id = s.SizeId,
                    Name = s.Name,
                    PriceDelta = s.PriceDelta
                })
                .FirstOrDefaultAsync();
        }

        public async Task<OptionDto?> GetMilkTypeByIdAsync(int milkTypeId)
        {
            return await _db.MilkTypes.AsNoTracking()
                .Where(m => m.MilkTypeId == milkTypeId && !m.Deleted)
                .Select(m => new OptionDto
                {
                    Id = m.MilkTypeId,
                    Name = m.Name,
                    PriceDelta = m.PriceDelta
                })
                .FirstOrDefaultAsync();
        }
        public async Task<List<OptionDto>> GetTemperaturesAsync()
        {
            return await _db.Temperatures.AsNoTracking()
                .Where(t => !t.Deleted)
                .OrderBy(t => t.Label)
                .Select(t => new OptionDto { Id = t.TemperatureId, Name = t.Label, PriceDelta = 0 })
                .ToListAsync();
        }

        public async Task<OptionDto?> GetTemperatureByIdAsync(int id)
        {
            return await _db.Temperatures.AsNoTracking()
                .Where(t => t.TemperatureId ==id && !t.Deleted)
                .OrderBy(t => t.Label)
                .Select(t => new OptionDto { Id = t.TemperatureId, Name = t.Label, PriceDelta = 0 })
                .FirstOrDefaultAsync();
        }

        public async Task<List<OptionDto>> GetSyrupsAsync()
        {
            return await _db.Syrups.AsNoTracking()
                .Where(s => !s.Deleted)
                .OrderBy(s => s.Name)
                .Select(s => new OptionDto { Id = s.SyrupId, Name = s.Name, PriceDelta = s.PriceDelta /* o 0 */ })
                .ToListAsync();
        }

        public async Task<OptionDto?> GetSyrupByIdAsync(int id)
        {
            return await _db.Syrups.AsNoTracking()
                .Where(s => s.SyrupId == id && !s.Deleted)
                .OrderBy(s => s.Name)
                .Select(s => new OptionDto { Id = s.SyrupId, Name = s.Name, PriceDelta = s.PriceDelta /* o 0 */ })
                .FirstOrDefaultAsync();
        }
    }
}
