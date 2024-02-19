using CollectionKnowledgeProject.Data;
using CollectionKnowledgeProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollectionKnowledgeProject.Controllers
{
    public class CategoriesController : Controller
    {
        public readonly ApplicationDbContext db;
        public readonly UserManager<ApplicationUser> _userManager;
        public readonly RoleManager<IdentityRole> _roleManager;
        public CategoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            int _perPage = 3;
            var categories = db.Categories.OrderBy(a => a.CreatedAt);
            if (TempData.ContainsKey("message"))
            {
                ViewBag.ErrorMessage = TempData["message"];
            }

            int totalItems = categories.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedCategories = categories.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.Categories = paginatedCategories;
            return View();
        }


        public IActionResult Show(int id, string sortBy = "createdAt", string sortOrder = "asc")
        {

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            Category category = db.Categories.Include(c => c.Questions).ThenInclude(q => q.User).FirstOrDefault(c => c.CategoryID == id);
            var questions = db.Questions.Include("Category").Include("User").Where(q => q.CategoryId == id);

            ViewBag.Category = category;

            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {

                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> questionIds = questions.Where(at => at.Title.Contains(search) || at.Content.Contains(search)).Select(a => a.QuestionID).ToList();

                List<int> questionIdsOfCommentsWithSearchString = db.Comments
                .Where(c => c.Content.Contains(search) && questions.Select(q => q.QuestionID).Contains((int)c.QuestionId))
                .Select(c => (int)c.QuestionId)
                .Distinct()
                .ToList();

                List<int> mergedIds = questionIds.Union(questionIdsOfCommentsWithSearchString).ToList();

                questions = db.Questions.Where(question => mergedIds.Contains(question.QuestionID)).Include("Category").Include("User").OrderBy(a => a.CreatedAt);
            }
            ViewBag.SearchString = search;

            questions = sortBy switch
            {
                "votes" => sortOrder == "asc" ? questions.OrderBy(q => q.Votes) : questions.OrderByDescending(q => q.Votes),
                _ => sortOrder == "asc" ? questions.OrderBy(q => q.CreatedAt) : questions.OrderByDescending(q => q.CreatedAt),
            };

            int _perPage = 5;

            int totalItems = questions.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset = 0;
            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedQuestions = questions.Skip(offset).Take(_perPage);
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.Questions = paginatedQuestions;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Categories/Show/" + category.CategoryID + "?search="
                + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Categories/Show/" + category.CategoryID + "?page";
            }

            return View();
        }


        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult Show([FromForm] Question question)
        {
            question.CreatedAt = DateTime.Now;
            question.Votes = 0;
            question.UserId = _userManager.GetUserId(User);

            Console.WriteLine(question.CategoryId);
            try
            {
                db.Questions.Add(question);
                db.SaveChanges();
                return Redirect("/Categories/Show/" + question.CategoryId);
            }
            catch (Exception ex)
            {
                return View();
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult New(Category category)
        {
            Console.WriteLine(category.CategoryName);
            if (ModelState.IsValid)
            {
                category.UserId = _userManager.GetUserId(User);
                category.CreatedAt = DateTime.Now;
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["message"] = "Categoria a fost adaugata";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Categoria nu a fost adaugata";
                return RedirectToAction("Index");
            }

        }


        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            Category c = db.Categories.Find(id);

            return View(c);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Category requestCategory)
        {
            Category category = db.Categories.Find(id);

            if (category != null)
            {
                category.CategoryName = requestCategory.CategoryName;

                db.SaveChanges();

                return RedirectToAction("Index", "Categories");
            }
            return NotFound();
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Category category = db.Categories.Include(c => c.Questions).FirstOrDefault(c => c.CategoryID == id);

            if (category != null)
            {
                if (User.IsInRole("Admin"))
                {
                    foreach(var question in category.Questions)
                    {
                        question.CategoryId = null;
                    }
                    db.Categories.Remove(category);
                    db.SaveChanges();

                    TempData["message"] = "Categoria a fost stearsa cu succes!";
                    return RedirectToAction("Index", "Categories");
                }
            }

            return NotFound();
        }
    }
}
