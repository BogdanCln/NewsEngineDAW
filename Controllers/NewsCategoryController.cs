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
            return View();
        }


        // GET: All categories or a single news category with ID specified as request parameter
        [ActionName("category")]
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Show(int ID, string sortBy)
        {
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
            return View("Create");
        }

        // POST: Send the new news category data.
        [ActionName("new")]
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create([Bind(Exclude = "CategoryID, CreateDate")]NewsCategory category)
        {
            category.CreateDate = DateTime.Now;

            try
            {
                Debug.WriteLine(ModelState.IsValid);

                if (ModelState.IsValid)
                {
                    db.NewsCategories.Add(category);
                    db.SaveChanges();
                    TempData["redirectMessage"] = "The category has been published.";
                    TempData["redirectMessageClass"] = "success";
                    return Redirect("/categories/category/" + category.CategoryID);
                }
                else
                {
                    string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                    Debug.WriteLine(messages);

                    TempData["redirectMessage"] = "The category has not been published.";
                    TempData["redirectMessageClass"] = "error";
                    return View("Create", category);
                }
            }
            catch (Exception e)
            {
                TempData["redirectMessage"] = "The category has not been published.";
                TempData["redirectMessageClass"] = "error";
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
            return Redirect("/categories");
        }

        [NonAction]
        public IQueryable<NewsCategory> GetNewsCategories()
        {
            var queryResult = from categories in db.NewsCategories orderby categories.CreateDate descending select categories;
            return queryResult;
        }

        public IQueryable<News> GetNewsArticlesByCategory(int catID, string sortBy)
        {
            IQueryable<News> articles;
            switch (sortBy)
            {
                case "date-asc":
                    articles = from news in db.NewsArticles where news.Category.CategoryID == catID orderby news.PublishDate ascending select news;
                    break;
                case "date-desc":
                    articles = from news in db.NewsArticles where news.Category.CategoryID == catID orderby news.PublishDate descending select news;
                    break;
                case "name-asc":
                    articles = from news in db.NewsArticles where news.Category.CategoryID == catID orderby news.Title ascending select news;
                    break;
                case "name-desc":
                    articles = from news in db.NewsArticles where news.Category.CategoryID == catID orderby news.Title descending select news;
                    break;
                default:
                    articles = from news in db.NewsArticles where news.Category.CategoryID == catID orderby news.PublishDate descending select news;
                    break;
            }
            return articles;
        }

    }
}