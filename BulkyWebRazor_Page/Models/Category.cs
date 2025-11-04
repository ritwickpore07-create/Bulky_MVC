using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyWebRazor_Page.Models
{
	public class Category
	{
		[Key]
		public int Id { get; set; }
		[Required]
		[MaxLength(30)]
		[DisplayName("Category Name")]
		public string Name { get; set; }
		[Range(1, 150, ErrorMessage = "Display Order must be 1 to 150")]
		[DisplayName("Display Order")]
		public int DisplayOrder { get; set; }
	}
}
