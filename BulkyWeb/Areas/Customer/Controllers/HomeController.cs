using System.Diagnostics;
using Bulky.DataAccess.Repository.Implement;
using Bulky.DataAccess.Repository.Interface;
using Bulky.Models.Models;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BulkyWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			IEnumerable<Bulky.Models.Models.Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
			return View(productList);
		}
		public IActionResult Details(int productId)
		{
			ShoppingCart cart = new()
			{
				Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category"),
				Count = 1,
				ProductId = productId
			};
			if (cart == null)
			{
				return NotFound();
			}
			//Product product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category");
			//if(product == null)
			//{
			//	return NotFound();
			//}
			return View(cart);
		}
		[HttpPost]
		[Authorize]
		public IActionResult Details(ShoppingCart shoppingCart)
		{			
			var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
			shoppingCart.ApplicationUserId = userId;
			ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId
			&& u.ProductId == shoppingCart.ProductId);
			if (cartFromDb != null)
			{
				// Shoping Cart already exists
				cartFromDb.Count = cartFromDb.Count + shoppingCart.Count;
				_unitOfWork.ShoppingCart.Update(cartFromDb);
				_unitOfWork.Save();
			}
			else
			{
				// Add to Cart
				_unitOfWork.ShoppingCart.Add(shoppingCart);
				_unitOfWork.Save();
				HttpContext.Session.SetInt32(SD.SessionCart,
				_unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
			}
			TempData["Success"] = "Cart updated successfully";			

			return RedirectToAction(nameof(Index));
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
