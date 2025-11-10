using Bulky.DataAccess.Data;
using Bulky.Models.Models;
using Bulky.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.DbInitializer
{
	public class DbInitializer : IDbInitializer
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ApplicationDbContext _dbContext;
		public DbInitializer(
			UserManager<IdentityUser> userManager,
			RoleManager<IdentityRole> roleManager,
			ApplicationDbContext dbContext)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_dbContext = dbContext;
		}
		public void Initialize()
		{
			// Migrations if they are not applied
			try
			{
				if (_dbContext.Database.GetPendingMigrations().Count() > 0)
				{
					_dbContext.Database.Migrate();
				}
			}
			catch (Exception ex)
			{

			}

			// Create Roles if they are not created
			if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
			{
				_roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
				_roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
				_roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
				_roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();

				// If roles are not created, then we will create admin user
				_userManager.CreateAsync(new ApplicationUser
				{
					UserName = "admin@bulkybook.com",
					Email = "admin@bulkybook.com",
					Name = "Sudhansu Pore",
					PhoneNumber = "7548930850",
					StreetAddress = "123 Colony St",
					City = "Highly Clean",
					State = "WB",
					PostalCode = "73545",
				}, "Admin@123").GetAwaiter().GetResult();

				ApplicationUser? adminUser = _dbContext.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@bulkybook.com");
				_userManager.AddToRoleAsync(adminUser, SD.Role_Admin).GetAwaiter().GetResult();
			}

			return;
		}
	}
}
