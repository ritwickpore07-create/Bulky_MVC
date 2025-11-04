using BulkyWebRazor_Page.Data;
using BulkyWebRazor_Page.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Page.Pages.Categories
{
	[BindProperties]
	public class CreateModel : PageModel
	{
		private readonly ApplicationDbContext _dbContext;

		public Category Category { get; set; }

		public CreateModel(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public void OnGet()
		{
			//CategoryList = _dbContext.Categories.ToList();
		}

		public IActionResult OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}
			_dbContext.Categories.Add(Category);
			_dbContext.SaveChanges();
			TempData["Success"] = "Category created successfully";
			return RedirectToPage("Index");
		}
	}
}
