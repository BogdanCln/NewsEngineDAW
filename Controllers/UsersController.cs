using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NewsEngineTemplate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewsEngineTemplate.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        // GET: Users
        public ActionResult Index()
        {
            List<ApplicationUser> users = (from user in db.Users
                                           orderby user.UserName
                                           select user).ToList();
            ViewBag.UsersList = users;
            return View();
        }

        [ActionName("edit")]
        public ActionResult Update(string ID)
        {
            ApplicationUser user = db.Users.Find(ID);
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId; 
            return View("Update", user);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();
            var roles = from role in db.Roles select role;
            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }

        [HttpPut]
        [ActionName("edit")]
        public ActionResult Update(string ID, ApplicationUser newData)
        {

            ApplicationUser user = db.Users.Find(ID);
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                if (TryUpdateModel(user))
                {
                    user.UserName = newData.UserName;
                    user.Email = newData.Email;
                    user.PhoneNumber = newData.PhoneNumber;
                    var roles = from role in db.Roles select role;
                    foreach (var role in roles)
                    {
                        UserManager.RemoveFromRole(ID, role.Name);
                    }
                    var selectedRole =
                    db.Roles.Find(HttpContext.Request.Params.Get("newRole"));
                    UserManager.AddToRole(ID, selectedRole.Name);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                return View("Update", user);
            }

        }
    }
}