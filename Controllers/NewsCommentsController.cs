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
    public class NewsCommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return Redirect("/news/");
        }

        // POST: Send the new comment data.
        [ActionName("new")]
        [HttpPost]
        [Authorize(Roles = "User, Editor, Administrator")]
        public ActionResult Create([Bind(Exclude = "UserID, PublishDate")]NewsComments comment)
        {
            comment.PublishDate = DateTime.Now;
            comment.UserID = User.Identity.GetUserId();

            try
            {
                Debug.WriteLine(ModelState.IsValid);

                if (ModelState.IsValid)
                {
                    db.NewsComments.Add(comment);
                    db.SaveChanges();
                    TempData["redirectMessage"] = "The comment has been published.";
                    TempData["redirectMessageClass"] = "success";

                    return Redirect("/news/article/" + comment.ArticleID);
                }
                else
                {
                    string messages = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));
                    Debug.WriteLine(messages);

                    TempData["redirectMessage"] = messages + " The comment has not been published.";
                    TempData["redirectMessageClass"] = "danger";
                    return Redirect("/news/article/" + comment.ArticleID);
                }
            }
            catch (Exception e)
            {
                TempData["redirectMessage"] = "The comment has not been published.";
                TempData["redirectMessageClass"] = "danger";
                return Redirect("/news/article/" + comment.ArticleID);
            }
        }

        [ActionName("edit")]
        [HttpGet]
        // GET: View for editing a comment
        [Authorize(Roles = "User, Editor,Administrator")]
        public ActionResult Update(int ID)
        {
            NewsComments comment = db.NewsComments.Find(ID);

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

            if (comment.UserID == User.Identity.GetUserId())
            {
                return View("Update", comment);
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
        [Authorize(Roles = "User, Editor,Administrator")]
        public ActionResult Update(int ID, NewsComments commentMod)
        {
            if (commentMod.UserID != User.Identity.GetUserId())
            {
                TempData["redirectMessage"] = "Permission denied";
                TempData["redirectMessageClass"] = "danger";
                return RedirectToAction("Index");
            }

            NewsComments comment = db.NewsComments.Find(ID);
            if (TryUpdateModel(comment))
            {
                if (ModelState.IsValid)
                {
                    comment.Content = commentMod.Content;
                    db.SaveChanges();
                    TempData["redirectMessage"] = "The comment has been modified";
                    TempData["redirectMessageClass"] = "success";
                    return Redirect("/news/article/" + comment.ArticleID);
                }
                else
                {
                    return View("Update", comment);
                }
            }
            else
            {
                TempData["redirectMessage"] = "The comment has not been modified - TryUpdateModel";
                TempData["redirectMessageClass"] = "danger";

                return View("Update", comment);
            }
        }

        // DELETE: Delete a comment
        [ActionName("delete")]
        [HttpDelete]
        [Authorize(Roles = "User, Editor,Administrator")]
        public ActionResult Delete(int ID)
        {
            try
            {
                NewsComments comment = db.NewsComments.Find(ID);
                if (comment.UserID == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                {
                    db.NewsComments.Remove(comment);
                    db.SaveChanges();

                    TempData["redirectMessage"] = "The comment has been deleted.";
                    TempData["redirectMessageClass"] = "info";
                }
                else
                {
                    TempData["redirectMessage"] = "Permission denied.";
                    TempData["redirectMessageClass"] = "danger";
                }
                return Redirect("/news/article/" + comment.ArticleID);
            }
            catch (Exception e)
            {
                TempData["redirectMessage"] = "The comment has not been deleted " + e.Message;
                TempData["redirectMessageClass"] = "danger";
                return Redirect("/news");
            }
        }
    }
}