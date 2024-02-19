using CollectionKnowledgeProject.Data;
using CollectionKnowledgeProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.RegularExpressions;

namespace CollectionKnowledgeProject.Controllers
{
    public class QuestionsController : Controller
    {
        public readonly ApplicationDbContext db;
        public readonly UserManager<ApplicationUser> _userManager;
        public readonly RoleManager<IdentityRole> _roleManager;
        public QuestionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            int _perPage = 3;
            var questions = db.Questions.Include("Category").Include(q => q.User).OrderBy(a => a.CreatedAt);
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
                ViewBag.Alert = TempData["messageType"];
            }

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
            return View();
        }

        public IActionResult Show(int id, string sortBy = "createdAt", string sortOrder = "asc")
        {

            Question question = db.Questions.Include(q => q.Comments).Include(q => q.Category).Include(q => q.User).FirstOrDefault(q => q.QuestionID == id);
            var comments = db.Comments.Include("Question").Include("User").Where(q => q.QuestionId == id);

            comments = sortBy switch
            {
                "votes" => sortOrder == "asc" ? comments.OrderBy(q => q.Votes) : comments.OrderByDescending(q => q.Votes),
                _ => sortOrder == "asc" ? comments.OrderBy(q => q.CreatedAt) : comments.OrderByDescending(q => q.CreatedAt),
            };

            var categories = db.Categories.OrderBy(a => a.CreatedAt);

            ViewBag.Question = question;
            ViewBag.Categories = categories;
            ViewBag.Comments = comments;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View();
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.CreatedAt = DateTime.Now;
            comment.Votes = 0;
            comment.UserId = _userManager.GetUserId(User);
            Console.WriteLine("DN ADJMAWIDJAOPWDIKAWPDOIKAWPODIAWPODWAIP");
            Console.WriteLine(comment.QuestionId);
            try
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Questions/Show/" + comment.QuestionId);
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            return View();
        }


        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult New(Question question)
        {
            question.Votes = 0;
            question.UserId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
            {
                db.Questions.Add(question);
                db.SaveChanges();
                TempData["message"] = "Intrebarea a fost adaugata";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Intrebarea nu a fost adaugata";
                return View();
            }

        }


        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Question question = db.Questions.Include(q => q.Comments).FirstOrDefault(q => q.QuestionID == id);

            if (question != null)
            {
                if (question.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    foreach (var comment in question.Comments.ToList())
                    {
                        db.Comments.Remove(comment);
                    }

                    db.Questions.Remove(question);
                    db.SaveChanges();

                    return Redirect("/Categories/Show/" + question.CategoryId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                    return RedirectToAction("Index", "Categories");
                }
            }

            return NotFound();
        }


        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Question q = db.Questions.Find(id);

            return View(q);
        }


        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult Edit(int id, Question requestQuestion)
        {
            Question question = db.Questions.Find(id);

            if (question != null)
            {
                if (question.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    question.Title = requestQuestion.Title;
                    question.Content = requestQuestion.Content;

                    db.SaveChanges();

                    return Redirect("/Categories/Show/" + question.CategoryId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                    return RedirectToAction("Index", "Categories");
                }
            }
            else
            {
                return NotFound();
            }

        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult ChangeCategory(int questionId, int categoryId)
        {
            Question question = db.Questions.Find(questionId);
            Category category = db.Categories.Find(categoryId);
     
            if (question != null && category != null)
            {
                if (question.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    if (question.CategoryId == categoryId)
                    {
                        TempData["message"] = "Articolul este deja in aceasta categorie.";
                        return RedirectToAction("Index", "Categories");
                    }

                    if (question.CategoryId.HasValue && question.Category != null)
                    {
                        var oldCategory = db.Categories.FirstOrDefault(c => c.CategoryID == question.CategoryId.Value);
                        if (oldCategory != null && oldCategory.Questions != null)
                        {
                            oldCategory.Questions.Remove(question); 
                        }
                    }

                    if (category.Questions == null) category.Questions = new List<Question>();
                    category.Questions.Add(question);

                    question.CategoryId = category.CategoryID;
                    question.Category = category;

                    db.SaveChanges();

                    return Redirect("/Questions/Show/" + questionId);
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                    return RedirectToAction("Index", "Categories");
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult Vote(int questionId, int voteValue)
        {
            var question = db.Questions.Find(questionId);
            if (question != null)
            {
                question.Votes += voteValue;
                db.SaveChanges();
            }

            return RedirectToAction("Show", new { id = questionId });
        }

    }
}

