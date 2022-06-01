using FamousPeople.Data;
using FamousPeople.Models;
using FamousPeople.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FamousPeople.Controllers
{
    [Authorize(Roles = UserRoles.Admin)] //определяется доступ ко всем экшнметодам
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        string imagePath = @"\images\";

        public PersonController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            IEnumerable<Person> objList = _db.People.OrderBy(x => x.FullName);

            return View(objList);
        }

        [AllowAnonymous]
        public IActionResult Filter(string searchString)
        {
            var allPeople = _db.People;

            if (!string.IsNullOrEmpty(searchString))
            {
                var filteredResult = allPeople.Where(n => n.FullName.ToLower().Contains(searchString.ToLower())).ToList();

                /*var filteredResultNew =
                    allPeople.Where(n => string.Equals(n.FullName, searchString, StringComparison.CurrentCultureIgnoreCase)
                    || string.Equals(n.Description, searchString, StringComparison.CurrentCultureIgnoreCase)).ToList();*/

                return View("Index", filteredResult);
            }

            return View("Index", allPeople);
        }

        [AllowAnonymous]
        public IActionResult FilterByCat(Category category)
        {
            var allPeople = _db.People;

            if (category != null)
            {
                var filteredResult = allPeople.Where(n => n.Category == category).ToList();

                return View("Index", filteredResult);
            }

            return View("Index", allPeople);
        }

        [AllowAnonymous]
        public IActionResult FilterByDate()
        {
            var allPeople = _db.People;

            var filteredResult = allPeople.Where(n => n.BirthDate.Date == DateTime.Now.Date).ToList();

            if (filteredResult.Count > 0)
                return View("Index", filteredResult);

            return View("Index", allPeople);
        }

        //GET: Movies/Details/1
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _db.People.Include(u => u.Category).FirstOrDefaultAsync(m => m.Id == id);

            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        //Get - UPSERT
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.User}")] //определяется доступ ко всем экшнметодам
        public IActionResult Upsert(int? id)
        {
            PersonViewModel productVM = new PersonViewModel()
            {
                Person = new Person(),
                CategorySelectList = _db.Categories.Select(u => new SelectListItem
                {
                    Text = u.CategoryName,
                    Value = u.Id.ToString()
                })
            };

            if (id == null)
            {
                return View(productVM);// create new product
            }
            else
            {
                productVM.Person = _db.People.Find(id.GetValueOrDefault());
                if (productVM.Person == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }
        }

        //POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.User}")] //определяется доступ ко всем экшнметодам
        public IActionResult Upsert(PersonViewModel personVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files; //тут будет хранится файл (var - IFormFileCollection), улавливается самостоятельно
                string webRootPath = _webHostEnvironment.WebRootPath; //путь ка папке wwwroot
                if (personVM.Person.Id == 0)//определяем для создания или обновления вызывается
                {
                    //creating
                    string upload = webRootPath + imagePath;//полный путь к нужной папке
                    string filename = Guid.NewGuid().ToString();//для имени файла используется случайный guid (глобальный уникальный идентификатор )
                    string extension = Path.GetExtension(files[0].FileName);//расширение для файла, присваивается из файла, который уже был загружен?????

                    using (var fileStream = new FileStream(Path.Combine(upload, filename + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);//копируется файл в новое местоположение
                    }
                    personVM.Person.ProfilePictureURL = filename + extension;

                    _db.Add(personVM.Person);
                }
                else
                {
                    //updating

                    //var objFromDb = _db.Product.AsNoTracking().FirstOrDefault(u=>u.Id == productVM.Product.Id);
                    var objFromDb = _db.People.AsNoTracking().FirstOrDefault(u => u.Id == personVM.Person.Id);


                    if (files.Count > 0)
                    {
                        string upload = webRootPath + imagePath;
                        string filename = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        var oldFile = Path.Combine(upload, objFromDb.ProfilePictureURL);// ссылка на старое фото

                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);//если файл фото существует, то удаляем его
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, filename + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);//копируется файл в новое местоположение
                        }

                        personVM.Person.ProfilePictureURL = filename + extension;
                    }
                    else
                    {
                        personVM.Person.ProfilePictureURL = objFromDb.ProfilePictureURL;
                    }

                    _db.Update(personVM.Person);
                }


                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            personVM.CategorySelectList = _db.Categories.Select(u => new SelectListItem
            {
                Text = u.CategoryName,
                Value = u.Id.ToString()
            });

            // TempData[WC.Error] = "Error";
            return View(personVM);
        }


        // GET: PersonController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PersonController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Person person)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Add(person);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            return View(person);
        }

        // GET: PersonController/Edit/5

        public ActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var obj = _db.People.Find(id.GetValueOrDefault());

            if (obj == null)
                return NotFound();

            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Person person)
        {
            if (id != person.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(person);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            return View();
        }

        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            Person person = _db.People.Include(u => u.Category).FirstOrDefault(u => u.Id == id);//eager loading

            if (person == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }

            return View(person);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var person = await _db.People.FindAsync(id);
            if (person == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                string upload = _webHostEnvironment.WebRootPath + imagePath;

                var oldFile = Path.Combine(upload, person.ProfilePictureURL);

                if (System.IO.File.Exists(oldFile))
                {
                    System.IO.File.Delete(oldFile);
                }

                _db.People.Remove(person);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }
    }
}
