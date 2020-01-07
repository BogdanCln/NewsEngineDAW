using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NewsEngineTemplate.Models
{
    public class NewsComments
    {
        [Key]
        public int ID { get; set; }

        public string UserID { get; set; }

        [Required]
        public int ArticleID { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required(ErrorMessage = "Comment content is mandatory")]
        [StringLength(500, ErrorMessage = "Comment too long. Maximum allowed is 500 characters.")]
        public string Content { get; set; }

        public DateTime PublishDate { get; set; }

        public virtual News Article { get; set; }
    }
}