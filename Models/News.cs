﻿using NewsEngineTemplate.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NewsEngineTemplate.Models
{
    public class News
    {
        [Key]
        public int ID { get; set; }

        public Boolean isProposal { get; set; } = false;

        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Title is mandatory")]
        [StringLength(80, ErrorMessage = "Title too long. Maximum allowed is 80 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is mandatory")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Category is mandatory")]
        public int CategoryID { get; set; }

        public virtual NewsCategory Category { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }

        public virtual ICollection<NewsComments> NewsComments { get; set; }

        public DateTime PublishDate { get; set; }
    }
}