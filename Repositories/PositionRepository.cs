using DACN.Data;
using DACN.DTOs.Request;
using DACN.Models;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace DACN.Repositories
{
    public class PositionRepository
    {
        private readonly ApplicationDbContext db;

        public PositionRepository(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<List<PositionModel>> GetAllAsync()
        {
            return await db.Positions.ToListAsync();
        }
        public async Task<PositionModel?> GetByIdAsync(int id)
        {
            return await db.Positions
                .FirstOrDefaultAsync(j => j.PositionId == id);
        }
        public async Task<(List<PositionModel> Data, int Total)> GetPagedAsync(
        int page, int pageSize, string keySearch, DateTime? fromDate, DateTime? toDate, int isActive)
        {
            var query = db.Positions
                .AsNoTracking() // ✅ Không track, tránh DataReader giữ connection
                .Where(d => !d.IsDeleted);

            if (!string.IsNullOrEmpty(keySearch))
                query = query.Where(d => d.PositionName.Contains(keySearch));

            if (fromDate.HasValue)
                query = query.Where(d => d.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(d => d.CreatedAt <= toDate.Value);

            if (isActive != -1)
                query = query.Where(d => d.IsActive == (isActive == 1));

            // ✅ EF sẽ tự đóng connection sau mỗi truy vấn
            int total = await query.CountAsync();

            var data = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }
        public async Task UpdatePositionAsync(int id, PositionRequest dto)
        {
            var dep = await db.Positions.FindAsync(id);
            if (dep == null || dep.IsDeleted)
                throw new ArgumentException("Department not found");
            dep.PositionName = dto.PositionName;
            dep.Description = dto.Description;
            dep.IsActive = dto.IsActive;
            dep.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }
        public async Task DeletePositionAsync(int id)
        {
            var dep = await db.Positions.FindAsync(id);
            if (dep == null || dep.IsDeleted)
                throw new ArgumentException("Position not found or already deleted");

            dep.IsDeleted = true;
            await db.SaveChangesAsync();
        }
        public async Task CreatePositionAsync(PositionRequest dto)
        {
            var pos = new PositionModel
            {
                PositionName = dto.PositionName,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };
            db.Positions.Add(pos);
            await db.SaveChangesAsync();
        }
    }
}
