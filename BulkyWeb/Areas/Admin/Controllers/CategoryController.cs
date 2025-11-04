using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.Interface;
using Bulky.Models.Models;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class CategoryController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public CategoryController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			List<Category> categories = _unitOfWork.Category.GetAll().ToList();

			return View(categories);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Create(Category obj)
		{
			if (obj.Name == obj.DisplayOrder.ToString())
			{
				ModelState.AddModelError("Name", "The DisplayOrder can't exactly match the Name");
			}
			if (ModelState.IsValid)
			{
				_unitOfWork.Category.Add(obj);
				_unitOfWork.Save();
				TempData["Success"] = "Category created successfully";
				return RedirectToAction("Index");
			}
			return View();
		}

		public IActionResult Edit(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}
			Category? categoryFromDb = _unitOfWork.Category.Get(e=> e.Id == id);
			//Category? categoryFromDb1 = _dbContext.Categories.Where(e => e.Id == id).FirstOrDefault();
			if (categoryFromDb == null)
			{
				return NotFound();
			}

			return View(categoryFromDb);
		}

		[HttpPost]
		public IActionResult Edit(Category obj)
		{
			if (ModelState.IsValid)
			{
				_unitOfWork.Category.Update(obj);
				_unitOfWork.Save();
				TempData["Success"] = "Category updated successfully";
				return RedirectToAction("Index");
			}
			return View();
		}

		public IActionResult Delete(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}
			Category? categoryFromDb = _unitOfWork.Category.Get(e => e.Id == id);
			//Category? categoryFromDb1 = _dbContext.Categories.Where(e => e.Id == id).FirstOrDefault();
			if (categoryFromDb == null)
			{
				return NotFound();
			}

			return View(categoryFromDb);
		}

		[HttpPost, ActionName("Delete")]
		public IActionResult DeletePost(int? id)
		{
			Category? obj = _unitOfWork.Category.Get(e => e.Id == id);
			if (obj == null)
			{
				return NotFound();
			}
			_unitOfWork.Category.Remove(obj);
			_unitOfWork.Save();
			TempData["Success"] = "Category deleted successfully";
			return RedirectToAction("Index");
		}
	}
}
