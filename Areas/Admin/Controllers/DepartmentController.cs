using DACN.DTOs;
using DACN.DTOs.Request;
using DACN.DTOs.Respone;
using DACN.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DepartmentController : Controller
    {
        private readonly DepartmentRepository departmentRepository;
        public DepartmentController(DepartmentRepository departmentRepository)
        {
            this.departmentRepository = departmentRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<PartialViewResult> ListData(
         int page = 1, int pageSize = 5, string keySearch = "", 
         DateTime? fromDate = null, DateTime? DateTo = null,int isActive=-1)
        {
            var (entities, total) = await departmentRepository.GetPagedAsync(page, pageSize, keySearch, fromDate, DateTo, isActive);
            var data = entities.Select(j => new DepartmentRespone
            {
                Id = j.DepartmentId,
                DepartmentName = j.DepartmentName,
                Description = j.Description,
                IsActive = j.IsActive,
                CreatedAt = j.CreatedAt,
                UpdatedAt = j.UpdatedAt,
                IsDeleted = j.IsDeleted,
                // map thêm các trường cần thiết
            }).ToList();
            ViewBag.page = page;
            ViewBag.pageSize = pageSize;
            ViewBag.total = total;
            ViewBag.totalPage = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.stt = (page - 1) * pageSize;
            return PartialView(data);
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) // kiểm tra id hợp lệ
                return Json(new { success = false, message = "ID không hợp lệ" });

            var dep = await departmentRepository.GetByIdAsync(id);
            if (dep == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });

            // Map sang DTO
            var depDto = new DepartmentRespone
            {
                Id = dep.DepartmentId,
                DepartmentName = dep.DepartmentName,
                Description = dep.Description,
                IsActive = dep.IsActive,
                CreatedAt = dep.CreatedAt,
                UpdatedAt = dep.UpdatedAt,
                IsDeleted = dep.IsDeleted,
            };
            return PartialView("Detail", depDto);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) // kiểm tra id hợp lệ
                return Json(new { success = false, message = "ID không hợp lệ" });
            var dep = await departmentRepository.GetByIdAsync(id);
            if (dep == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi" });
            // Map sang DTO
            var depDto = new DepartmentRespone
            {
                Id = dep.DepartmentId,
                DepartmentName = dep.DepartmentName,
                Description = dep.Description,
                IsActive = dep.IsActive,
            };
            return PartialView("Edit", depDto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int Id,DepartmentRequest dto)
        {
            // 1. Validate ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }
            // 2. Gọi Repository
            try
            {
                await departmentRepository.UpdateDepartmentAsync(Id,dto);
                return Json(new { success = true, message = "Cập nhật tin phòng ban thành công!" });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Lấy message lỗi từ InnerException nếu có
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = errorMessage });
            }
        }
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) // kiểm tra id hợp lệ
                return Json(new { success = false, message = "ID không hợp lệ" });
            try
            {
                await departmentRepository.DeleteDepartmentAsync(id);
                return Json(new { success = true, message = "Xóa phòng ban thành công!" });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Lấy message lỗi từ InnerException nếu có
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = errorMessage });
            }
        }
        [HttpGet]
        public  IActionResult Create()
        {
            return PartialView("Create");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentRequest dto)
        {
            // 1. Validate ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, errors });
            }
            try
            {
                await departmentRepository.CreateDepartmentAsync(dto);

                return Json(new { success = true, message = "Đã tạo phòng ban thành công!" });
            }
            catch (Exception ex)
            {
                // Lấy message lỗi từ InnerException nếu có
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = errorMessage });
            }
        }

    }
}
