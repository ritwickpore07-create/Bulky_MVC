using Bulky.DataAccess.Repository.Interface;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}

		public IActionResult Index()
		{
			List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

			return View(products);
		}

		[HttpGet]
		public IActionResult Upsert(int? id)
		{
			IEnumerable<SelectListItem> categoryList = _unitOfWork.Category.GetAll().Select
				(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString()
				});
			//ViewBag.CategoryList = CategoryList;
			//ViewData["CategoryList"] = CategoryList;

			ProductVM productVM = new()
			{
				CategoryList = categoryList,
				Product = new Product()
			};
			if (id == null || id == 0)
			{
				// Create
				return View(productVM);
			}
			else
			{
				// Update
				productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
				return View(productVM);
			}
		}

		[HttpPost]
		public IActionResult Upsert(ProductVM productVM, IFormFile? file)
		{
			if (ModelState.IsValid)
			{
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				if (file != null)
				{
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
					string productPath = Path.Combine(wwwRootPath, @"images/product");

					if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
					{
						// Delete the old Image
						var normalizedPath = productVM.Product.ImageUrl
											 .TrimStart('/', '\\') // remove leading slash
											 .Replace('/', Path.DirectorySeparatorChar); // convert to Windows-style path
						var oldImagePath = Path.Combine(wwwRootPath, normalizedPath);
						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}

					using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}

					productVM.Product.ImageUrl = @"/images/product/" + fileName;
				}

				if (productVM.Product.Id == 0)
				{
					_unitOfWork.Product.Add(productVM.Product);
					_unitOfWork.Save();
					TempData["Success"] = "Product created successfully";
				}
				else
				{
					_unitOfWork.Product.Update(productVM.Product);
					_unitOfWork.Save();
					TempData["Success"] = "Product updated successfully";
				}				
				return RedirectToAction("Index");
			}
			else
			{
				productVM.CategoryList = _unitOfWork.Category.GetAll().Select
						(u => new SelectListItem
						{
							Text = u.Name,
							Value = u.Id.ToString()
						});
				return View(productVM);
			}
		}

		//public IActionResult Edit(int? id)
		//{
		//	if (id == null || id == 0)
		//	{
		//		return NotFound();
		//	}
		//	Product? productFromDb = _unitOfWork.Product.Get(e => e.Id == id);
		//	if (productFromDb == null)
		//	{
		//		return NotFound();
		//	}

		//	return View(productFromDb);
		//}

		//[HttpPost]
		//public IActionResult Edit(Product obj)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		_unitOfWork.Product.Update(obj);
		//		_unitOfWork.Save();
		//		TempData["Success"] = "Product updated successfully";
		//		return RedirectToAction("Index");
		//	}
		//	return View();
		//}

		//public IActionResult Delete(int? id)
		//{
		//	if (id == null || id == 0)
		//	{
		//		return NotFound();
		//	}
		//	Product? productFromDb = _unitOfWork.Product.Get(e => e.Id == id);
		//	if (productFromDb == null)
		//	{
		//		return NotFound();
		//	}

		//	return View(productFromDb);
		//}

		//[HttpPost, ActionName("Delete")]
		//public IActionResult DeletePost(int? id)
		//{
		//	Product? obj = _unitOfWork.Product.Get(e => e.Id == id);
		//	if (obj == null)
		//	{
		//		return NotFound();
		//	}
		//	_unitOfWork.Product.Remove(obj);
		//	_unitOfWork.Save();
		//	TempData["Success"] = "Product deleted successfully";
		//	return RedirectToAction("Index");
		//}

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll()
		{
			List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
			return Json(new { data = products });
		}

		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			Product? productToBeDeleted = _unitOfWork.Product.Get(e => e.Id == id);
			if (productToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}

			// Delete the old Image
			var normalizedPath = productToBeDeleted.ImageUrl
								 .TrimStart('/', '\\') // remove leading slash
								 .Replace('/', Path.DirectorySeparatorChar); // convert to Windows-style path
			var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, normalizedPath);
			if (System.IO.File.Exists(oldImagePath))
			{
				System.IO.File.Delete(oldImagePath);
			}
			_unitOfWork.Product.Remove(productToBeDeleted);
			_unitOfWork.Save();
			//TempData["Success"] = "Product deleted successfully";
			return Json(new { success = true, message = "Successfully deleted" });
		}
		#endregion
	}
}
