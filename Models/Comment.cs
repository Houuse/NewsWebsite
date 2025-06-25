using System;
using System.ComponentModel.DataAnnotations;

namespace NewsWebsite.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int ArticleId { get; set; }
        public Article Article { get; set; }

        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CommentDate { get; set; }
    }
}