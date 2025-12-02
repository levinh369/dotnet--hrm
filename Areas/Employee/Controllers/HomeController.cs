using DACN.DTOs.Respone;
using DACN.Models;
using DACN.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DACN.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class HomeController : Controller
    {
        private readonly JobPostingRepository jobPostingRepository;
        public HomeController(JobPostingRepository jobPostingRepository)
        {
            this.jobPostingRepository = jobPostingRepository;
        }
        public async Task<IActionResult> Index()
        {
            var jobs = await jobPostingRepository.GetAllActiveAsync();
            var response = jobs.Select(j => new JobPostingRespone
            {
                Id = j.Id,
                Title = j.Title,
                DepartmentName = j.Department?.DepartmentName,
                positionName = j.Position?.PositionName,
                SalaryRange = j.SalaryRange,
                PostedDate = j.PostedDate,
                ViewCount = j.ViewCount ?? 0,
                ExpirationDate = j.ExpirationDate,
                JobDescription = j.JobDescription,
            }).ToList();

            return View(response);
        }

    }
}
