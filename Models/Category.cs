using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewsWebsite.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public virtual ICollection<Article> Articles { get; set; }  = new List<Article>();
    }
}