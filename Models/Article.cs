using System;
using System.ComponentModel.DataAnnotations;

namespace NewsWebsite.Models
{
    public class Article
    {
        internal readonly int? ViewCount;

        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime PublishDate { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }

        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? AudioUrl { get; set; }

        public bool IsPublished { get; set; }

        public ICollection<Comment> Comments { get; set; }
        public ICollection<ArticleTag> ArticleTags { get; set; }
    }
}