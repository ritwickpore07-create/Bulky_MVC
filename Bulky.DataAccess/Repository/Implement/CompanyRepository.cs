using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.Interface;
using Bulky.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.Implement
{
	public class CompanyRepository : Repository<Company>, ICompanyRepository
	{
		private readonly ApplicationDbContext _dbContext;
		public CompanyRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
			_dbContext = dbContext;
		}		

		public void Update(Company company)
		{
			_dbContext.Companies.Update(company);
		}
	}
}
