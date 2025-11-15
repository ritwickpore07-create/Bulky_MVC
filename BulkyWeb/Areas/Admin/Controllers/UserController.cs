using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.Interface;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class UserController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public UserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager,
							  RoleManager<IdentityRole> roleManager)
		{
			_unitOfWork = unitOfWork;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult RoleManagement(string userId)
		{
			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("User ID is required.");
			}

			// Safely fetch user with company
			var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties: "Company");

			if (applicationUser == null)
			{
				return NotFound("User not found.");
			}

			// Build ViewModel
			var roleManagementVM = new RoleManagementVM
			{
				ApplicationUser = applicationUser,
				RoleList = _roleManager.Roles.Select(r => new SelectListItem
				{
					Text = r.Name,
					Value = r.Id
				}).ToList(),
				CompanyList = _unitOfWork.Company.GetAll().Select(c => new SelectListItem
				{
					Text = c.Name,
					Value = c.Id.ToString()
				}).ToList()
			};

			// Safely assign role name			
			var role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == userId)).GetAwaiter().GetResult().FirstOrDefault();
			roleManagementVM.ApplicationUser.Role = role ?? "Unknown";

			return View(roleManagementVM);
		}


		[HttpPost]
		public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
		{
			var user = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagementVM.ApplicationUser.Id);
			if (user == null)
			{
				return NotFound("User not found.");
			}

			var userRole = _unitOfWork.ApplicationUser.Get(u => u.Id == user.Id);
			
			var roles = _userManager.GetRolesAsync(user).Result;

			string newRoleId = roleManagementVM.ApplicationUser.Role;

			// Use FindByIdAsync to avoid LINQ translation issues
			var identityRole = _roleManager.FindByIdAsync(newRoleId).Result;
			if (identityRole == null)
			{
				return BadRequest($"Role with ID '{newRoleId}' does not exist.");
			}

			string oldRoleName = roles.FirstOrDefault(); // null if no role assigned
			string newRoleName = identityRole.Name;

			if (!roles.Any())
			{
				_userManager.AddToRoleAsync(user, newRoleName).GetAwaiter().GetResult();
			}

			//// Defensive check to avoid null reference
			//string oldRoleName = string.Empty;
			//if (userRole != null && !string.IsNullOrWhiteSpace(userRole.Id))
			//{
			//	var oldRole = _roleManager.FindByIdAsync(userRole.Id).Result;
			//	oldRoleName = oldRole.Name;				
			//}
			
			//string oldRole = _dbContext.Roles.FirstOrDefault(r => r.Id == userRole.RoleId)?.Name;
			//string newRole = roleManagementVM.ApplicationUser.Role;

			if (oldRoleName != newRoleName)
			{
				if (newRoleName == SD.Role_Company)
				{
					user.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
				}
				if (oldRoleName == SD.Role_Company)
				{
					user.CompanyId = null;
				}

				_unitOfWork.ApplicationUser.Update(user);
				_unitOfWork.Save();

				if (!string.IsNullOrWhiteSpace(oldRoleName))
				{
					_userManager.RemoveFromRoleAsync(user, oldRoleName).GetAwaiter().GetResult();
				}
				_userManager.AddToRoleAsync(user, newRoleName).GetAwaiter().GetResult();
			}
			else
			{
				if (oldRoleName == SD.Role_Company && user.CompanyId != roleManagementVM.ApplicationUser.CompanyId)
				{
					user.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
					_unitOfWork.ApplicationUser.Update(user);
					_unitOfWork.Save();
				}
			}
			return RedirectToAction("Index");
		}

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll()
		{
			List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "Company").ToList();			

			foreach (var user in objUserList)
			{
				user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault() ?? "Unknown";

				if (user.Company == null)
				{
					user.Company = new Company()
					{
						Name = ""
					};
				}
			}
			return Json(new { data = objUserList });
		}

		[HttpPost]
		public IActionResult LockUnlock([FromBody] string id)
		{
			var objFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
			if (objFromDb == null)
			{
				return Json(new { success = false, message = "Error while locking/unlocking" });
			}
			else
			{
				if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
				{
					// user is currently locked, we will unlock them
					objFromDb.LockoutEnd = DateTime.Now;
				}
				else
				{
					objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
				}
				_unitOfWork.Save();
			}

			return Json(new { success = true, message = "Operation Successful" });
		}
		#endregion
	}
}
