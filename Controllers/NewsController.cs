using Microsoft.AspNet.Identity;
using NewsEngineTemplate.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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

        // GET: All news list
        [Authorize(Roles = "Editor,Administrator")]
        [ActionName("proposals")]
        public ActionResult Proposals()
        {
            List<News> articles = GetNewsProposals(null);
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
            ViewBag.userID = User.Identity.GetUserId();

            return View("Proposals");
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
        //[Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Show(int ID)
        {
            ViewBag.isAdmin = User.IsInRole("Administrator");
            ViewBag.isEditor = User.IsInRole("Editor");
            ViewBag.isUser = User.IsInRole("User");
            ViewBag.userID = User.Identity.GetUserId();

            try
            {
                News article = db.NewsArticles.Find(ID);
                article.Categories = GetAllCategories();
                //article

                if (article.isProposal == true && (!User.IsInRole("Administrator") && !User.IsInRole("Editor")))
                {
                    TempData["redirectMessage"] = "Permission denied";
                    TempData["redirectMessageClass"] = "danger";
                    return RedirectToAction("Index");
                }

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

        // GET: View for adding an external news article.
        [ActionName("new-external")]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult CreateExternal()
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

            return View("CreateExternal");
        }

        // GET: View for proposing a news article.
        [ActionName("new-propose")]
        [Authorize(Roles = "User, Editor,Administrator")]
        public ActionResult CreateProposal()
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

            return View("CreateProposal");
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
                    TempData["redirectMessage"] = messages;
                    TempData["redirectMessageClass"] = "danger";

                    ViewBag.Categories = GetAllCategories();
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


        // POST: Receive a comment form-data.
        [HttpPost]
        [ActionName("new-comment")]
        [Authorize(Roles = "Editor, Administrator, User")]
        public ActionResult CreateComment(string content)
        {
            TempData["redirectMessage"] = "Not implemented";
            TempData["redirectMessageClass"] = "info";

            return Redirect("/news");
        }

        // POST: Receive the external news article form-data.
        [HttpPost]
        [ActionName("new-external")]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult CreateExternal(string URI)
        {
            // Only BBC.com scraping for now
            if (parseBBCURI(URI) == null)
            {
                Debug.WriteLine("Invalid BBC URI");

                TempData["redirectMessage"] = "Invalid BBC URI";
                TempData["redirectMessageClass"] = "warning";
                return Redirect("/news/new-external");
            }
            Debug.WriteLine(parseBBCURI(URI));

            string htmlCode = "";
            using (WebClient client = new WebClient())
            {
                try
                {
                    htmlCode = client.DownloadString(parseBBCURI(URI));

                    TempData["redirectMessage"] = "Got article data from BBC";
                    TempData["redirectMessageClass"] = "success";

                    //Debug.WriteLine(htmlCode);
                    News article = new News();

                    string titlePattern = @"(\""headline.*?\"","")";
                    string contentPattern = @"(\""description.*?\"","")";

                    foreach (Match m in Regex.Matches(htmlCode, titlePattern))
                    {
                        if (m.Value.Length > 15)
                        {
                            string inPattern = @"(\""headline.*?\"":\""*\s*)";
                            article.Title = Regex.Replace(m.Value, inPattern, "");
                            int endCut = article.Title.Length - 3;
                            article.Title = article.Title.Remove(endCut);
                            article.Title = article.Title + " [BBC.com]";
                        }
                    }

                    foreach (Match m in Regex.Matches(htmlCode, contentPattern))
                    {
                        if (m.Value.Length > 18)
                        {
                            string inPattern = @"(\""description.*?\"":\""*\s*)";
                            article.Content = Regex.Replace(m.Value, inPattern, "");
                            //article.Content = m.Value.Substring(15);

                            int endCut = article.Content.Length - 3;
                            article.Content = article.Content.Remove(endCut);
                            article.Content = article.Content + "\nRead the full story on " + URI;
                            article.Content = article.Content.Replace("\\n", System.Environment.NewLine);
                        }
                    }

                    ViewBag.Categories = GetAllCategories();
                    return View("Create", article);
                }
                catch (Exception err)
                {
                    TempData["redirectMessage"] = err.Message;
                    TempData["redirectMessageClass"] = "warning";
                    return Redirect("/news/new-external");
                }
            }
        }

        // POST: Receive the new news article proposal form-data.
        [HttpPost]
        [ActionName("new-proposal")]
        [Authorize(Roles = "User, Editor,Administrator")]
        public ActionResult CreateProposal([Bind(Exclude = "ID, PublishDate")] News article)
        {
            article.UserID = User.Identity.GetUserId();
            article.PublishDate = DateTime.Now;
            article.isProposal = true;

            try
            {
                if (ModelState.IsValid)
                {
                    db.NewsArticles.Add(article);
                    db.SaveChanges();
                    TempData["redirectMessage"] = "The article has been submitted for proposal.";
                    TempData["redirectMessageClass"] = "success";
                    return Redirect("/news");
                }
                else
                {
                    string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                    Debug.WriteLine(messages);
                    TempData["redirectMessage"] = messages;
                    TempData["redirectMessageClass"] = "danger";

                    ViewBag.Categories = GetAllCategories();
                    return View("CreateProposal", article);
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

            if (article.UserID == User.Identity.GetUserId() || User.IsInRole("Administrator") || User.IsInRole("Editor") && article.isProposal)
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
            IQueryable<News> articles = from news in db.NewsArticles where news.isProposal == false orderby news.PublishDate descending select news;
            if (searchExp != null)
            {
                articles = articles.Where(a => a.Title.Contains(searchExp));
            }
            return articles.ToList();
        }

        [NonAction]
        public List<News> GetNewsProposals(string searchExp)
        {
            IQueryable<News> articles = from news in db.NewsArticles where news.isProposal == true orderby news.PublishDate descending select news;
            if (searchExp != null)
            {
                articles = articles.Where(a => a.Title.Contains(searchExp));
            }
            return articles.ToList();
        }

        [NonAction]
        public List<NewsComments> GetNewsComments(int newsID)
        {
            IQueryable<NewsComments> comments = from c in db.NewsComments where c.ArticleID == newsID orderby c.PublishDate descending select c;
            return comments.ToList();
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

        [NonAction]
        private string parseBBCURI(string input)
        {
            if (input.Substring(0, 8) == "https://")
            {
                input = input.Substring(8);
            }
            else if (input.Substring(0, 7) == "http://")
            {
                input = input.Substring(7);
            }


            if (input.Substring(0, 4) == "www.")
            {
                input = input.Substring(4);
            }

            if (input.Substring(0, 7) != "bbc.com")
            {
                return null;
            }
            else
                return "http://" + input;

        }

    }
}