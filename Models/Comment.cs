using System;
using System.ComponentModel.DataAnnotations;

namespace NewsWebsite.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public virtual int ArticleId { get; set; }
        public virtual Article Article { get; set; }

        public virtual string? AuthorId { get; set; }
        public virtual ApplicationUser? Author { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CommentDate { get; set; }
    }
}