using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bulky.DataAccess.Repository.Interface;
using System.Linq.Expressions;
using Bulky.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Bulky.DataAccess.Repository.Implement
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _dbContext;
		internal DbSet<T> dbSet;
		public Repository(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
			dbSet = dbContext.Set<T>();
			//_dbContext.Categories = dbSet
			_dbContext.Products.Include(u => u.Category);
		}

		public void Add(T entity)
		{
			dbSet.Add(entity);
		}

		public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null)
		{
			IQueryable<T> query = dbSet;
			query = query.Where(filter);
			if (!string.IsNullOrEmpty(includeProperties))
			{
				foreach (var includeProp in includeProperties.Split(
						 new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp);
				}
			}
			return query.FirstOrDefault();
		}

		public IEnumerable<T> GetAll(string? includeProperties = null)
		{
			IQueryable<T> query = dbSet;
			if (!string.IsNullOrEmpty(includeProperties))
			{
				foreach (var includeProp in includeProperties.Split(
					     new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProp);
				}
			}
			return query.ToList();
		}

		public void Remove(T entity)
		{
			dbSet.Remove(entity);
		}

		public void RemoveRange(IEnumerable<T> entity)
		{
			dbSet.RemoveRange(entity);
		}
	}
}
