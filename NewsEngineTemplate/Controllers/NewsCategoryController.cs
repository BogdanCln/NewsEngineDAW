using NewsEngineTemplate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewsEngineTemplate.Controllers
{
    public class NewsCategoryController : Controller
    {
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
                ViewBag.category = category;
                return View("Show");
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
        public ActionResult Create(NewsCategory category)
        {
            category.CreateDate = DateTime.Now;

            try
            {
                categoriesDB.NewsCategories.Add(category);
                categoriesDB.SaveChanges();
                return Redirect("/categories/category/" + category.CategoryID);
            }
            catch (Exception e)
            {
                ViewBag.errorMessage = e.Message;
                return View("Error");
            }
        }

        [ActionName("edit")]
        [HttpGet]
        // GET: View for editing a news category
        public ActionResult Update(int ID)
        {
            NewsCategory category = categoriesDB.NewsCategories.Find(ID);
            ViewBag.category = category;
            return View("Update");
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
    }
}