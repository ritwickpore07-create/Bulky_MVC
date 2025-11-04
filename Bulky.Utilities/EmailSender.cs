using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Utilities
{
	public class EmailSender : IEmailSender
	{
		// Logic to send email will be implemented later
		public Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			return Task.CompletedTask;
		}
	}
}
