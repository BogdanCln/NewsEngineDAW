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
        private NewsDBContext newsDB = new NewsDBContext();
        private NewsCategoryDBContext categoriesDB = new NewsCategoryDBContext();

        // GET: All categories
        public ActionResult Index()
        {
            IQueryable<NewsCategory> categories = GetNewsCategories();
            ViewBag.categories = categories;
            return View();
        }


        // GET: All categories or a single news category with ID specified as request parameter
        [ActionName("category")]
        public ActionResult Show(int ID)
        {
            try
            {
                NewsCategory category = categoriesDB.NewsCategories.Find(ID);
                ViewBag.news = GetNewsArticlesByCategory(ID);
                return View("Show", category);
            }
            catch (Exception e)
            {
                ViewBag.errorMessage = "Couldn't find news category #" + ID.ToString();
                return View("Error");
            }
        }

        // GET: View for adding a news category.
        [ActionName("new")]
        public ActionResult Create()
        {
            return View("Create");
        }

        // POST: Send the new news category data.
        [ActionName("new")]
        [HttpPost]
        public ActionResult Create([Bind(Exclude = "CategoryID, CreateDate")]NewsCategory category)
        {
            category.CreateDate = DateTime.Now;

            try
            {
                Debug.WriteLine(ModelState.IsValid);

                if (ModelState.IsValid)
                {
                    categoriesDB.NewsCategories.Add(category);
                    categoriesDB.SaveChanges();
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
        // GET: View for editing a news category
        public ActionResult Update(int ID)
        {
            NewsCategory category = categoriesDB.NewsCategories.Find(ID);
            return View("Update", category);
        }

        // PUT: Send the updated news article data.
        [ActionName("edit")]
        [HttpPut]
        public ActionResult Update(int ID, NewsCategory categoryMod)
        {
            try
            {
                NewsCategory category = categoriesDB.NewsCategories.Find(ID);
                if (TryUpdateModel(category))
                {
                    category.Title = categoryMod.Title;
                    category.Description = categoryMod.Description;
                    categoriesDB.SaveChanges();
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
        public ActionResult Delete(int ID)
        {
            try
            {
                NewsCategory category = categoriesDB.NewsCategories.Find(ID);
                categoriesDB.NewsCategories.Remove(category);
                categoriesDB.SaveChanges();
            }
            catch (Exception e) { }
            // #TODO
            return Redirect("/categories");
        }

        [NonAction]
        public IQueryable<NewsCategory> GetNewsCategories()
        {
            var queryResult = from categories in categoriesDB.NewsCategories orderby categories.CreateDate descending select categories;
            return queryResult;
        }

        public IQueryable<News> GetNewsArticlesByCategory(int catID)
        {
            var articles = from news in newsDB.NewsArticles where news.Category.CategoryID == catID orderby news.PublishDate descending select news;
            return articles;
        }

    }
}