using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewsWebsite.Models;

namespace NewsWebsite.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ArticleTag> ArticleTags { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure composite key for ArticleTag
            builder.Entity<ArticleTag>()
                .HasKey(at => new { at.ArticleId, at.TagId });

            // Seed initial categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Trending" },
                new Category { Id = 2, Name = "Political" },
                new Category { Id = 3, Name = "Economic" },
                new Category { Id = 4, Name = "Cultural" }
            );
        }
    }
}