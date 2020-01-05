using Microsoft.AspNet.Identity;
using NewsEngineTemplate.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewsEngineTemplate.Controllers
{
    public class NewsCategoryController : Controller
    {
        //private NewsDBContext newsDB = new NewsDBContext();
        //private NewsCategoryDBContext categoriesDB = new NewsCategoryDBContext();
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: All categories
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Index()
        {
            IQueryable<NewsCategory> categories = GetNewsCategories();
            ViewBag.categories = categories;

            ViewBag.isAdmin = User.IsInRole("Administrator");
            ViewBag.isEditor = User.IsInRole("Editor");
            ViewBag.isUser = User.IsInRole("User");
            ViewBag.userID = User.Identity.GetUserId();

            if (TempData.ContainsKey("redirectMessage"))
            {
                ViewBag.notification = TempData["redirectMessage"].ToString();
                if (TempData.ContainsKey("redirectMessageClass"))
                {
                    ViewBag.notificationClass = TempData["redirectMessageClass"].ToString();
                }
                else
                {
                    ViewBag.notificationClass = "info";
                }
            }
            return View();
        }


        // GET: All categories or a single news category with ID specified as request parameter
        [ActionName("category")]
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Show(int ID, string sortBy)
        {
            ViewBag.isAdmin = User.IsInRole("Administrator");
            ViewBag.isEditor = User.IsInRole("Editor");
            ViewBag.isUser = User.IsInRole("User");
            ViewBag.userID = User.Identity.GetUserId();

            if (TempData.ContainsKey("redirectMessage"))
            {
                ViewBag.notification = TempData["redirectMessage"].ToString();
                if (TempData.ContainsKey("redirectMessageClass"))
                {
                    ViewBag.notificationClass = TempData["redirectMessageClass"].ToString();
                }
                else
                {
                    ViewBag.notificationClass = "info";
                }
            }

            try
            {
                NewsCategory category = db.NewsCategories.Find(ID);
                ViewBag.news = GetNewsArticlesByCategory(ID, sortBy);
                return View("Show", category);
            }
            catch (Exception e)
            {
                ViewBag.errorMessage = "Couldn't find news category #" + ID.ToString();
                return View("Error");
            }
        }

        // GET: View for adding a news category.
        [Authorize(Roles = "Administrator")]
        [ActionName("new")]
        public ActionResult Create()
        {
            ViewBag.isAdmin = User.IsInRole("Administrator");
            ViewBag.isEditor = User.IsInRole("Editor");
            ViewBag.isUser = User.IsInRole("User");
            ViewBag.userID = User.Identity.GetUserId();

            if (TempData.ContainsKey("redirectMessage"))
            {
                ViewBag.notification = TempData["redirectMessage"].ToString();
                if (TempData.ContainsKey("redirectMessageClass"))
                {
                    ViewBag.notificationClass = TempData["redirectMessageClass"].ToString();
                }
                else
                {
                    ViewBag.notificationClass = "info";
                }
            }

            return View("Create");
        }

        // POST: Send the new news category data.
        [ActionName("new")]
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create([Bind(Exclude = "CategoryID, CreateDate")]NewsCategory category)
        {
            category.CreateDate = DateTime.Now;

            ViewBag.isAdmin = User.IsInRole("Administrator");
            ViewBag.isEditor = User.IsInRole("Editor");
            ViewBag.isUser = User.IsInRole("User");
            ViewBag.userID = User.Identity.GetUserId();

            try
            {
                Debug.WriteLine(ModelState.IsValid);

                if (ModelState.IsValid)
                {
                    db.NewsCategories.Add(category);
                    db.SaveChanges();
                    TempData["redirectMessage"] = "The category has been published.";
                    TempData["redirectMessageClass"] = "success";

                    return Redirect("/categories");
                }
                else
                {
                    string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                    Debug.WriteLine(messages);

                    TempData["redirectMessage"] = messages + " The category has not been published.";
                    TempData["redirectMessageClass"] = "danger";
                    return View("Create", category);
                }
            }
            catch (Exception e)
            {
                TempData["redirectMessage"] = "The category has not been published.";
                TempData["redirectMessageClass"] = "danger";
                return View("Create", category);
            }
        }

        [ActionName("edit")]
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        // GET: View for editing a news category
        public ActionResult Update(int ID)
        {
            NewsCategory category = db.NewsCategories.Find(ID);
            return View("Update", category);
        }

        // PUT: Send the updated news article data.
        [ActionName("edit")]
        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public ActionResult Update(int ID, NewsCategory categoryMod)
        {
            try
            {
                NewsCategory category = db.NewsCategories.Find(ID);
                if (TryUpdateModel(category))
                {
                    category.Title = categoryMod.Title;
                    category.Description = categoryMod.Description;
                    db.SaveChanges();
                    return Redirect("/categories/category/" + category.CategoryID);
                }
                else throw (new Exception("TryUpdateModel failed"));
            }
            catch (Exception e)
            {
                ViewBag.errorMessage = e.Message;
                return View("Error");
            }
        }

        // DELETE: Delete an article of news.
        [ActionName("delete")]
        [HttpDelete]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int ID)
        {
            try
            {
                NewsCategory category = db.NewsCategories.Find(ID);
                db.NewsCategories.Remove(category);
                db.SaveChanges();
            }
            catch (Exception e) { }
            // #TODO

            TempData["redirectMessage"] = "The category has been deleted.";
            TempData["redirectMessageClass"] = "warning";
            return Redirect("/categories");
        }

        [NonAction]
        public IQueryable<NewsCategory> GetNewsCategories()
        {
            var queryResult = from categories in db.NewsCategories orderby categories.CreateDate descending select categories;
            return queryResult;
        }

        public List<News> GetNewsArticlesByCategory(int catID, string sortBy)
        {
            List<News> articles;
            switch (sortBy)
            {
                case "date-asc":
                    articles = (from news in db.NewsArticles where news.Category.CategoryID == catID orderby news.PublishDate ascending select news).ToList();
                    break;
                case "date-desc":
                    articles = (from news in db.NewsArticles where news.Category.CategoryID == catID orderby news.PublishDate descending select news).ToList();
                    break;
                case "name-asc":
                    articles = (from news in db.NewsArticles where news.Category.CategoryID == catID orderby news.Title ascending select news).ToList();
                    break;
                case "name-desc":
                    articles = (from news in db.NewsArticles where news.Category.CategoryID == catID orderby news.Title descending select news).ToList();
                    break;
                default:
                    articles = (from news in db.NewsArticles where news.Category.CategoryID == catID orderby news.PublishDate descending select news).ToList();
                    break;
            }
            return articles;
        }

    }
}