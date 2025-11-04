using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.Interface;
using Bulky.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.Implement
{
	public class ProductRepository : Repository<Product>, IProductRepository
	{
		private readonly ApplicationDbContext _dbContext;
		public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
			_dbContext = dbContext;
		}

		public void Update(Product product)
		{
			//_dbContext.Products.Update(product);
			var objFromDb = _dbContext.Products.FirstOrDefault(u => u.Id == product.Id);
			if (objFromDb != null)
			{
				objFromDb.Title = product.Title;
				objFromDb.Author = product.Author;
				objFromDb.Description = product.Description;
				objFromDb.Price = product.Price;
				objFromDb.ListPrice = product.ListPrice;
				objFromDb.Price50 = product.Price50;
				objFromDb.Price100 = product.Price100;
				objFromDb.ISBN = product.ISBN;
				objFromDb.CategoryId = product.CategoryId;
				//objFromDb.Category = product.Category;
				if (product.ImageUrl != null)
				{
					objFromDb.ImageUrl = product.ImageUrl;
				}
			}
		}
	}
}
