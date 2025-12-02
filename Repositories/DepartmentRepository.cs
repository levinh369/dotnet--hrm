using DACN.Data;
using DACN.DTOs.Respone;
using DACN.DTOs.Request;
using DACN.Models;
using Microsoft.EntityFrameworkCore;
using static DACN.Enums.StatusEnums;
namespace DACN.Repositories
{
    public class DepartmentRepository
    {
        private readonly ApplicationDbContext db;

        public DepartmentRepository(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<List<DepartmentModel>> GetAllAsync()
        {
            return await db.Departments.ToListAsync();
        }
        public async Task<DepartmentModel?> GetByIdAsync(int id)
        {
            return await db.Departments
                .FirstOrDefaultAsync(j => j.DepartmentId == id);
        }
        public async Task<(List<DepartmentModel> Data, int Total)> GetPagedAsync(
        int page, int pageSize, string keySearch, DateTime? fromDate, DateTime? toDate, int isActive)
        {
            var query = db.Departments
                .AsNoTracking() // ✅ Không track, tránh DataReader giữ connection
                .Where(d => !d.IsDeleted);

            if (!string.IsNullOrEmpty(keySearch))
                query = query.Where(d => d.DepartmentName.Contains(keySearch));

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
        public async Task UpdateDepartmentAsync(int id, DepartmentRequest dto)
        {
            var dep = await db.Departments.FindAsync(id);
            if (dep == null || dep.IsDeleted)
                throw new ArgumentException("Department not found");
            dep.DepartmentName = dto.DepartmentName;
            dep.Description = dto.Description;
            dep.IsActive = dto.IsActive;
            dep.UpdatedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }
        public async Task DeleteDepartmentAsync(int id)
        {
            var dep = await db.Departments.FindAsync(id);
            if (dep == null || dep.IsDeleted)
                throw new ArgumentException("Department not found or already deleted");

            dep.IsDeleted = true;
            await db.SaveChangesAsync();
        }
        public async Task CreateDepartmentAsync(DepartmentRequest dto)
        {
            var dep = new DepartmentModel
            {
                DepartmentName= dto.DepartmentName,
                Description= dto.Description,
                IsActive= dto.IsActive,
                CreatedAt= DateTime.Now,
                IsDeleted= false
            };
            db.Departments.Add(dep);
            await db.SaveChangesAsync();
        }


    }
}
