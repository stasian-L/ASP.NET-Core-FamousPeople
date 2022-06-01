using FamousPeople.Models;

namespace FamousPeople.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Person> People { get; set; }

        public IEnumerable<Category> Categories { get; set; }
    }
}
