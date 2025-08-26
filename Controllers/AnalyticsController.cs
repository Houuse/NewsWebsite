using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsWebsite.Data;

namespace NewsWebsite.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalViews = await _context.Articles.SumAsync(static a => a.ViewCount ?? 0);
            var topArticles = await _context.Articles
                .OrderByDescending(static a => a.ViewCount ?? 0)
                .Take(5)
                .Include(static a => a.Category)
                .ToListAsync();
            var categoryPerformance = await _context.Categories
                .Select(static c => new { c.Name, ArticleCount = c.Articles.Count, ViewCount = c.Articles.Sum(static a => a.ViewCount ?? 0) })
                .ToListAsync();

            ViewBag.TotalViews = totalViews;
            ViewBag.TopArticles = topArticles;
            ViewBag.CategoryPerformance = categoryPerformance;
            return View();
        }

        public async Task<IActionResult> ArticleViewsReport()
        {
            var report = await _context.Articles
                .Select(static a => new { a.Title, a.ViewCount  })
                .OrderByDescending(static x => x.ViewCount
                )
                .ToListAsync();
            return View(report);
        }

        public async Task<IActionResult> TopArticles()
        {
            var articles = await _context.Articles
                .OrderByDescending(static a => a.ViewCount ?? 0)
                .Include(static a => a.Category)
                .Include(static a => a.Author)
                .Take(10)
                .ToListAsync();
            return View(articles);
        }
    }
}