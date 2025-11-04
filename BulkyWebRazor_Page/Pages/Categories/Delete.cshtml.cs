using BulkyWebRazor_Page.Data;
using BulkyWebRazor_Page.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Page.Pages.Categories
{
	[BindProperties]
	public class DeleteModel : PageModel
	{
		private readonly ApplicationDbContext _dbContext;

		public Category? Category { get; set; }

		public DeleteModel(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		//public void OnGet(int? id)
		//{
		//	if (id != null && id != 0)
		//	{
		//		Category = _dbContext.Categories.Find(id);
		//	}
		//}

		public IActionResult OnGet(int? id)
		{
			if (id == null || id == 0)
			{
				return RedirectToPage("Index");
			}

			Category = _dbContext.Categories.Find(id);
			if (Category == null)
			{
				return NotFound();
			}

			return Page();
		}

		public IActionResult OnPost()
		{
			Category? obj = _dbContext.Categories.Find(Category.Id);
			if (obj == null)
			{
				return NotFound();
			}
			_dbContext.Categories.Remove(obj);
			_dbContext.SaveChanges();
			TempData["Success"] = "Category deleted successfully";
			return RedirectToPage("Index");
		}
	}
}
