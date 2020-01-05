using Microsoft.AspNet.Identity;
using NewsEngineTemplate.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace NewsEngineTemplate.Controllers
{
    public class NewsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: All news list
        //[Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Index()
        {
            List<News> articles = GetNewsArticles(null);
            ViewBag.news = articles;

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

            ViewBag.isAdmin = User.IsInRole("Administrator");
            ViewBag.isEditor = User.IsInRole("Editor");
            ViewBag.isUser = User.IsInRole("User");
            ViewBag.userID = User.Identity.GetUserId();

            return View();
        }

        public ActionResult Search(string searchExp)
        {
            Debug.WriteLine("?Search for " + searchExp);
            List<News> articles = GetNewsArticles(searchExp);
            ViewBag.news = articles;
            ViewBag.isAdmin = User.IsInRole("Administrator");
            ViewBag.isEditor = User.IsInRole("Editor");
            ViewBag.isUser = User.IsInRole("User");
            ViewBag.userID = User.Identity.GetUserId();

            return View("Index");
        }

        // GET: All news list or a single news article with ID specified as request parameter
        [ActionName("article")]
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Show(int ID)
        {
            try
            {
                News article = db.NewsArticles.Find(ID);
                article.Categories = GetAllCategories();

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

                ViewBag.isAdmin = User.IsInRole("Administrator");
                ViewBag.isEditor = User.IsInRole("Editor");
                ViewBag.isUser = User.IsInRole("User");
                ViewBag.userID = User.Identity.GetUserId();

                return View("Show", article);
            }
            catch (Exception e)
            {
                //ViewBag.errorMessage = "Couldn't find news article #" + ID.ToString();
                //return View("Error");
                return Redirect("/news");
            }
        }

        // GET: View for adding a news article.
        [ActionName("new")]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Create()
        {
            ViewBag.Categories = GetAllCategories();

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

        // POST: Receive the new news article form-data.
        [HttpPost]
        [ActionName("new")]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Create([Bind(Exclude = "ID, PublishDate")] News article)
        {
            article.UserID = User.Identity.GetUserId();
            article.PublishDate = DateTime.Now;

            try
            {
                if (ModelState.IsValid)
                {
                    db.NewsArticles.Add(article);
                    db.SaveChanges();
                    TempData["redirectMessage"] = "The article has been published.";
                    TempData["redirectMessageClass"] = "success";
                    return Redirect("/news/article/" + article.ID);
                }
                else
                {
                    string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                    Debug.WriteLine(messages);
                    ViewBag.Categories = GetAllCategories();
                    TempData["redirectMessage"] = messages;
                    TempData["redirectMessageClass"] = "danger";
                    return View("Create", article);
                }
            }
            catch (Exception e)
            {
                string messages = string.Join("; ", ModelState.Values
                                    .SelectMany(x => x.Errors)
                                    .Select(x => x.ErrorMessage));
                Debug.WriteLine(e.Message + messages);
                ViewBag.Categories = GetAllCategories();
                TempData["redirectMessage"] = e.Message + " " + messages;
                TempData["redirectMessageClass"] = "danger";
                return Redirect("/news/new/");
            }
        }

        [ActionName("edit")]
        [HttpGet]
        // GET: View for editing a news article
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Update(int ID)
        {
            News article = db.NewsArticles.Find(ID);
            article.Categories = GetAllCategories();

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

            if (article.UserID == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                return View("Update", article);
            }
            else
            {
                TempData["redirectMessage"] = "Permission denied";
                TempData["redirectMessageClass"] = "danger";
                return RedirectToAction("Index");
            }

        }

        // PUT: Send the updated news article data.
        [ActionName("edit")]
        [HttpPut]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Update(int ID, News articleMod)
        {
            News article = db.NewsArticles.Find(ID);
            if (TryUpdateModel(article))
            {
                if (ModelState.IsValid)
                {
                    article.Title = articleMod.Title;
                    article.Content = articleMod.Content;
                    db.SaveChanges();
                    TempData["redirectMessage"] = "The article has been modified";
                    TempData["redirectMessageClass"] = "success";
                    return Redirect("/news/article/" + article.ID);
                }
                else
                {
                    article.Categories = GetAllCategories();
                    return View("Update", article);
                }
            }
            else
            {
                TempData["redirectMessage"] = "The article has not been modified - TryUpdateModel";
                TempData["redirectMessageClass"] = "danger";

                article.Categories = GetAllCategories();
                return View("Update", article);
            }
        }

        // DELETE: Delete an article of news.
        [ActionName("delete")]
        [HttpDelete]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Delete(int ID)
        {
            try
            {
                News article = db.NewsArticles.Find(ID);
                if (article.UserID == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                {
                    db.NewsArticles.Remove(article);
                    db.SaveChanges();

                    TempData["redirectMessage"] = "The article has been deleted.";
                    TempData["redirectMessageClass"] = "info";
                }
                else
                {
                    TempData["redirectMessage"] = "Permission denied.";
                    TempData["redirectMessageClass"] = "danger";
                }
            }
            catch (Exception e)
            {
                TempData["redirectMessage"] = "The article has not been deleted.";
                TempData["redirectMessageClass"] = "danger";
            }
            return Redirect("/news");

        }

        [NonAction]
        public List<News> GetNewsArticles(string searchExp)
        {
            IQueryable<News> articles = from news in db.NewsArticles orderby news.PublishDate descending select news;
            if (searchExp != null)
            {
                articles = articles.Where(a => a.Title.Contains(searchExp));
            }
            return articles.ToList();
        }

        [NonAction]
        // Returns all the categories
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from cat in db.NewsCategories
                             select cat;

            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryID.ToString(),
                    Text = category.Title.ToString()
                });
            }

            // returnam lista de categorii
            return selectList;
        }

    }
}