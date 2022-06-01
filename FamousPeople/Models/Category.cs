using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FamousPeople.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Название")]
        [Required]
        public string CategoryName { get; set; }
    }
}
