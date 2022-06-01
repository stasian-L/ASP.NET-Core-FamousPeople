using FamousPeople.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FamousPeople.ViewModels
{
    public class PersonViewModel
    {
        public Person Person { get; set; }
        public IEnumerable<SelectListItem>? CategorySelectList { get; set; }
    }
}
