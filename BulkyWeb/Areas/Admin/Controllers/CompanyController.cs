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
	//[Authorize(Roles = SD.Role_Admin)]
	public class CompanyController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		
		public CompanyController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;			
		}

		public IActionResult Index()
		{
			List<Company> companyList = _unitOfWork.Company.GetAll().ToList();

			return View(companyList);
		}

		[HttpGet]
		public IActionResult Upsert(int? id)
		{
			if (id == null || id == 0)
			{
				// Create
				return View(new Company());
			}
			else
			{
				// Update
				Company companyObj = _unitOfWork.Company.Get(u => u.Id == id);
				return View(companyObj);
			}
		}

		[HttpPost]
		public IActionResult Upsert(Company companyObj)
		{
			if (ModelState.IsValid)
			{
				if (companyObj.Id == 0)
				{
					_unitOfWork.Company.Add(companyObj);
					_unitOfWork.Save();
					TempData["Success"] = "Company created successfully";
				}
				else
				{
					_unitOfWork.Company.Update(companyObj);
					_unitOfWork.Save();
					TempData["Success"] = "Company updated successfully";
				}
				return RedirectToAction("Index");
			}
			else
			{
				return View(companyObj);
			}
		}

		//public IActionResult Edit(int? id)
		//{
		//	if (id == null || id == 0)
		//	{
		//		return NotFound();
		//	}
		//	Company? productFromDb = _unitOfWork.Company.Get(e => e.Id == id);
		//	if (productFromDb == null)
		//	{
		//		return NotFound();
		//	}

		//	return View(productFromDb);
		//}

		//[HttpPost]
		//public IActionResult Edit(Company obj)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		_unitOfWork.Company.Update(obj);
		//		_unitOfWork.Save();
		//		TempData["Success"] = "Company updated successfully";
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
		//	Company? productFromDb = _unitOfWork.Company.Get(e => e.Id == id);
		//	if (productFromDb == null)
		//	{
		//		return NotFound();
		//	}

		//	return View(productFromDb);
		//}

		//[HttpPost, ActionName("Delete")]
		//public IActionResult DeletePost(int? id)
		//{
		//	Company? obj = _unitOfWork.Company.Get(e => e.Id == id);
		//	if (obj == null)
		//	{
		//		return NotFound();
		//	}
		//	_unitOfWork.Company.Remove(obj);
		//	_unitOfWork.Save();
		//	TempData["Success"] = "Company deleted successfully";
		//	return RedirectToAction("Index");
		//}

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll()
		{
			List<Company> companies = _unitOfWork.Company.GetAll().ToList();
			return Json(new { data = companies });
		}

		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var companyToBeDeleted = _unitOfWork.Company.Get(e => e.Id == id);
			if (companyToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error while deleting" });
			}
			_unitOfWork.Company.Remove(companyToBeDeleted);
			_unitOfWork.Save();
			//TempData["Success"] = "Company deleted successfully";
			return Json(new { success = true, message = "Successfully deleted" });
		}
		#endregion
	}
}
