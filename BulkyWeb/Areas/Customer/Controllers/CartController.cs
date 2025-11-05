using Bulky.DataAccess.Repository.Interface;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public ShoppingCartVM ShoppingCartVM { get; set; }

		public CartController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM = new()
			{
				ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
									includeProperties: "Product")
			};

			foreach (var cartItem in ShoppingCartVM.ShoppingCartList)
			{
				cartItem.Price = GetAmountBasedOnQuantity(cartItem);
				ShoppingCartVM.OrderTotal += cartItem.Price * cartItem.Count;
			}
			;
			return View(ShoppingCartVM);
		}

		public IActionResult Summary()
		{
			return View();
		}

		public IActionResult Plus(int cartId)
		{
			var cartItem = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
			cartItem.Count += 1;
			_unitOfWork.ShoppingCart.Update(cartItem);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Minus(int cartId)
		{
			var cartItem = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
			if (cartItem.Count <= 1)
			{
				// Remove the item from cart
				_unitOfWork.ShoppingCart.Remove(cartItem);
			}
			else
			{
				cartItem.Count -= 1;
				_unitOfWork.ShoppingCart.Update(cartItem);
			}
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Remove(int cartId)
		{
			var cartItem = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
			_unitOfWork.ShoppingCart.Remove(cartItem);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		private double GetAmountBasedOnQuantity(ShoppingCart shoppingCart)
		{
			if (shoppingCart.Count <= 50)
			{
				return shoppingCart.Product.Price;
			}
			else
			{
				if (shoppingCart.Count <= 100)
				{
					return shoppingCart.Product.Price50;
				}
				else
				{
					return shoppingCart.Product.Price100;
				}
			}
		}
	}
}
