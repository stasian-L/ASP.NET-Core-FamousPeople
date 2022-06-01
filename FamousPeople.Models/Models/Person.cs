using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamousPeople.Models
{
    public class Person
    {
        public Person()
        {
            
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "Изображение")]
        public string? ProfilePictureURL { get; set; }

        [Required]
        [Display(Name = "Имя")]
        public string FullName { get; set; }

        [Display(Name = "Биография")]
        public string Bio { get; set; }

        [Display(Name = "Подробная биография")]
        public string DetailedBio { get; set; }

        //Category
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
        [Display(Name = "Категория")]
        public int? CategoryId { get; set; }

        public bool IsApproved { get; set; }

        [Display(Name = "Дата рождения"), BindProperty, DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd'/'MM'/'yyyy}")]
        public DateTime BirthDate { get; set; }
    }
}
