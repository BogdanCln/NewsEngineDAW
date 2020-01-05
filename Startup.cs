using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using NewsEngineTemplate.Models;
using Owin;

[assembly: OwinStartupAttribute(typeof(NewsEngineTemplate.Startup))]
namespace NewsEngineTemplate
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createAdminUserAndApplicationRoles();
        }

        private void createAdminUserAndApplicationRoles()
        {

            ApplicationDbContext context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new
            RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new
            UserStore<ApplicationUser>(context));
            // Se adauga rolurile aplicatiei
            if (!roleManager.RoleExists("Administrator"))
            {
                // Se adauga rolul de administrator
                var role = new IdentityRole();
                role.Name = "Administrator";
                roleManager.Create(role);

                // se adauga utilizatorul administrator
                var user = new ApplicationUser();
                user.UserName = "admin@news.news";
                user.Email = "admin@news.news";
                var adminCreated = UserManager.Create(user, "Password!1");
                if (adminCreated.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Administrator");
                }
            }
            if (!roleManager.RoleExists("Editor"))
            {
                var role = new IdentityRole();
                role.Name = "Editor";
                roleManager.Create(role);
            }
            if (!roleManager.RoleExists("User"))
            {
                var role = new IdentityRole();
                role.Name = "User";
                roleManager.Create(role);
            }
        }
    }

}
