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


        private NewsDBContext newsDB = new NewsDBContext();
        private NewsCategoryDBContext categoriesDB = new NewsCategoryDBContext();

        // GET: All news list
        public ActionResult Index()
        {
            Debug.WriteLine("Hello World");
            IQueryable<News> articles = GetNewsArticles();
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

            return View();
        }


        // GET: All news list or a single news article with ID specified as request parameter
        [ActionName("article")]
        public ActionResult Show(int ID)
        {
            try
            {
                News article = newsDB.NewsArticles.Find(ID);
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
        [ActionName("new")]
        [HttpPost]
        public ActionResult Create([Bind(Exclude = "ID")] News article)
        {
            article.PublishDate = DateTime.Now;
            try
            {
                Debug.WriteLine(ModelState.IsValid);

                if (ModelState.IsValid)
                {
                    newsDB.NewsArticles.Add(article);
                    newsDB.SaveChanges();
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
                    TempData["redirectMessage"] = "The article has not been published.";
                    TempData["redirectMessageClass"] = "error";
                    return View("Create", article);
                }
            }
            catch (Exception e)
            {
                ViewBag.Categories = GetAllCategories();
                TempData["redirectMessage"] = "The article has not been published.";
                TempData["redirectMessageClass"] = "error";
                return View("Create", article);
            }
        }

        [ActionName("edit")]
        [HttpGet]
        // GET: View for editing a news article
        public ActionResult Update(int ID)
        {
            News article = newsDB.NewsArticles.Find(ID);
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

            return View("Update", article);
        }

        // PUT: Send the updated news article data.
        [ActionName("edit")]
        [HttpPut]
        public ActionResult Update(int ID, News articleMod)
        {
            News article = newsDB.NewsArticles.Find(ID);
            if (TryUpdateModel(article))
            {
                if (ModelState.IsValid)
                {
                    article.Title = articleMod.Title;
                    article.Content = articleMod.Content;
                    newsDB.SaveChanges();
                    TempData["redirectMessage"] = "The article has been modified - invalid ModelState";
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
        public ActionResult Delete(int ID)
        {
            try
            {
                News article = newsDB.NewsArticles.Find(ID);
                newsDB.NewsArticles.Remove(article);
                newsDB.SaveChanges();

                TempData["redirectMessage"] = "The article has been deleted.";
                TempData["redirectMessageClass"] = "info";
            }
            catch (Exception e)
            {
                TempData["redirectMessage"] = "The article has not been deleted.";
                TempData["redirectMessageClass"] = "danger";
            }

            return Redirect("/news");
        }

        [NonAction]
        public IQueryable<News> GetNewsArticles()
        {
            var articles = from news in newsDB.NewsArticles orderby news.PublishDate descending select news;
            return articles;
        }

        [NonAction]
        // Returns all the categories
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from cat in categoriesDB.NewsCategories
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