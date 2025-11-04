using BulkyWebRazor_Page.Data;
using BulkyWebRazor_Page.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Page.Pages.Categories
{
	[BindProperties]
	public class EditModel : PageModel
	{
		private readonly ApplicationDbContext _dbContext;

		public Category? Category { get; set; }

		public EditModel(ApplicationDbContext dbContext)
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
			if (!ModelState.IsValid)
			{
				return Page();
			}
			_dbContext.Categories.Update(Category);
			_dbContext.SaveChanges();
			TempData["Success"] = "Category updated successfully";
			return RedirectToPage("Index");
		}
	}
}

