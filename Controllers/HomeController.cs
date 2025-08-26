using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWebsite.Data;
using NewsWebsite.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NewsWebsite.Controllers
{
    public class HomeController(ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var categories = await context.Categories
                .Include(c => c.Articles).ThenInclude(a=>a.Author)
                .ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Search(string searchTerm, int page = 1)
        {
            int pageSize = 10;
            var query = context.Articles
                .Where(a => a.IsPublished && (string.IsNullOrEmpty(searchTerm) || a.Title.Contains(searchTerm) || a.Content.Contains(searchTerm)))
                .Include(a => a.Category)
                .Include(a => a.Author)
                .OrderByDescending(a => a.PublishDate);

            var articles = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);

            return View(articles);
        }
    }
}