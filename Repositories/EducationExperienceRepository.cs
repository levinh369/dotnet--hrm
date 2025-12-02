using DACN.Data;
using DACN.Models;
using Microsoft.EntityFrameworkCore;

namespace DACN.Repositories
{
    public class EducationExperienceRepository
    {
        private readonly ApplicationDbContext db;

        public EducationExperienceRepository(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task AddAsync(EducationExperienceModel edu)
        {
            db.educationExperiences.Add(edu);
            await db.SaveChangesAsync();
        }
    }
}
