using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewsWebsite.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        public string? ProfileImageUrl { get; set; }

        public virtual ICollection<Article> Articles { get; set; } =  new List<Article>();
        public virtual ICollection<Comment> Comments { get; set; } =  new List<Comment>();
    }
}