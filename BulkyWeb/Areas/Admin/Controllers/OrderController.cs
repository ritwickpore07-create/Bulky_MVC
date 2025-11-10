using Bulky.DataAccess.Repository.Interface;
using Bulky.Models.Models;
using Bulky.Models.ViewModels;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BulkyWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
		public OrderVM OrderVM { get; set; }
		public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Details(int orderId)
		{
			OrderVM = new()
			{
				OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
				OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
			};
			//ViewBag.DebugStatus = OrderVM.OrderHeader.OrderStatus;
			return View(OrderVM);
		}

		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult UpdateOrderDetail()
		{
			var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
			orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
			orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
			orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
			orderHeaderFromDb.City = OrderVM.OrderHeader.City;
			orderHeaderFromDb.State = OrderVM.OrderHeader.State;
			orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
			if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
			{
				orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
			}
			if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
			{
				orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
			}

			_unitOfWork.OrderHeader.Update(orderHeaderFromDb);
			_unitOfWork.Save();
			TempData["Success"] = "Order Details Updated Successfully";

			return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
		}

		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult StartProcessing()
		{
			_unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
			_unitOfWork.Save();
			TempData["Success"] = "Order Status Updated to In Process";
			return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
		}

		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult ShipOrder()
		{
			var orderStatus = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
			orderStatus.ShippingDate = System.DateTime.Now;
			orderStatus.Carrier = OrderVM.OrderHeader.Carrier;
			orderStatus.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
			orderStatus.OrderStatus = SD.StatusShipped;
			if (orderStatus.PaymentStatus == SD.PaymentStatusDelayedPayment)
			{
				orderStatus.PaymentDueDate = DateOnly.FromDateTime(System.DateTime.Now.AddDays(20));
			}

			_unitOfWork.OrderHeader.Update(orderStatus);
			_unitOfWork.Save();
			TempData["Success"] = "Order Shipped Updated Successfully";
			return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
		}

		[HttpPost]
		[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
		public IActionResult CancelOrder()
		{
			var orderStatus = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
			if (orderStatus.PaymentStatus == SD.StatusApproved)
			{
				var options = new RefundCreateOptions
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderStatus.PaymentIntentId
				};

				var service = new RefundService();
				Refund refund = service.Create(options);
				_unitOfWork.OrderHeader.UpdateStatus(orderStatus.Id, SD.StatusCancelled, SD.StatusRefunded);
			}
			else
			{
				_unitOfWork.OrderHeader.UpdateStatus(orderStatus.Id, SD.StatusCancelled, SD.StatusCancelled);
			}
			
			_unitOfWork.Save();
			TempData["Success"] = "Order Cancelled Successfully";
			return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
		}

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orders;

			if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
			{
				orders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
			}
			else
			{
				var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
				var userId = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

				orders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId,
									includeProperties: "ApplicationUser");
			}
			int count = status switch
			{
				"pending" => orders.Count(o => o.OrderStatus == SD.StatusPending),
				"inprocess" => orders.Count(o => o.OrderStatus == SD.StatusInProcess),
				"completed" => orders.Count(o => o.OrderStatus == SD.StatusShipped),
				"approved" => orders.Count(o => o.OrderStatus == SD.StatusApproved),
				"cancelled" => orders.Count(o => o.OrderStatus == SD.StatusCancelled),
				_ => orders.Count()
			};

			switch (status)
			{
				case "pending":
					orders = orders.Where(o => o.OrderStatus == SD.StatusPending);
					break;
				case "inprocess":
					orders = orders.Where(o => o.OrderStatus == SD.StatusInProcess);
					break;
				case "completed":
					orders = orders.Where(o => o.OrderStatus == SD.StatusShipped);
					break;
				case "approved":
					orders = orders.Where(o => o.OrderStatus == SD.StatusApproved);
					break;
				case "cancelled":
					orders = orders.Where(o => o.OrderStatus == SD.StatusCancelled);
					break;
				default:
					break;
			}
			return Json(new { data = orders });
		}
		#endregion
	}
}
