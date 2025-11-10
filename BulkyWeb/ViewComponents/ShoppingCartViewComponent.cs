using Bulky.DataAccess.Repository.Interface;
using Bulky.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.ViewComponents
{
	public class ShoppingCartViewComponent : ViewComponent
	{
		private readonly IUnitOfWork _unitOfWork;
		public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

			if (claim == null)
			{
				HttpContext.Session.Clear();
				return View(0);
			}
			else
			{
				if (HttpContext.Session.GetInt32(SD.SessionCart) == null)
				{
					var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Count();
					HttpContext.Session.SetInt32(SD.SessionCart, count);
				}
				return View(HttpContext.Session.GetInt32(SD.SessionCart));
			}
		}
	}
}
