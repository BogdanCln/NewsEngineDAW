using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace NewsEngineTemplate
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "News",
                url: "news/{action}/{ID}",
                defaults: new { controller = "News", action = "Index", ID = UrlParameter.Optional });

            routes.MapRoute(
                name: "Category",
                url: "categories/{action}/{ID}",
                defaults: new { controller = "NewsCategory", action = "Index", ID = UrlParameter.Optional });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "News", action = "Index" }
            );
        }
    }
}
