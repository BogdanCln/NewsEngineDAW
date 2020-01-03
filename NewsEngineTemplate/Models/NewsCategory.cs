using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace NewsEngineTemplate.Models
{
    public class NewsCategory
    {
        [Key]
        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Category title is mandatory")]
        [MaxLength(30)]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual ICollection<News> NewsArticles { get; set; }

    }

    public class NewsCategoryDBContext : DbContext
    {
        public NewsCategoryDBContext() : base("NewsDBConnectionString") { }
        public DbSet<NewsCategory> NewsCategories { get; set; }
    }
}
