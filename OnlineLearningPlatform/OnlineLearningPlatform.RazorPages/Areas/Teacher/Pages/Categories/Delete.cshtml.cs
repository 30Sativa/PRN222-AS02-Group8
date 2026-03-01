using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.RazorPages.Hubs;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Categories
{
    [Authorize(Roles = "Teacher")]
    public class DeleteModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<DataHub> _hub;

        public DeleteModel(ICategoryService categoryService, IHubContext<DataHub> hub)
        {
            _categoryService = categoryService;
            _hub = hub;
        }

        public Category Category { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();

            Category = category;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToPage("/Categories/Index", new { area = "Teacher" });
            }

            // Broadcast realtime: danh mục bị xóa → Teacher list tự remove row ngay
            await _hub.Clients.All.SendAsync("CategoryDeleted", new { categoryId = id });

            TempData["SuccessMessage"] = "Danh mục đã được xóa thành công.";
            return RedirectToPage("/Categories/Index", new { area = "Teacher" });
        }
    }
}
