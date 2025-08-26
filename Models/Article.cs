using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public virtual Category Category { get; set; }

        public string AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }

        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? AudioUrl { get; set; }

        public bool IsPublished { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}