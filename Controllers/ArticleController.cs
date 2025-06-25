using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NewsWebsite.Data;
using NewsWebsite.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NewsWebsite.Controllers
{
    public class ArticleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArticleController(ApplicationDbContext context)
        {
            _context = context;
        }
        // عرض صفحة إضافة الخبر
        public async Task<IActionResult> Create()
        {
            if (_context.Categories == null)
            {
                return NotFound("لا توجد فئات متاحة.");
            }
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        
        // عرض قائمة الأخبار
        public async Task<IActionResult> Index(string category, bool? isPublished)
        {
            var query = _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(a => a.Category.Name == category);
            }

            if (isPublished.HasValue)
            {
                query = query.Where(a => a.IsPublished == isPublished.Value);
            }

            var articles = await query.ToListAsync();
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Name", "Name");
            return View(articles);
        }

        // حذف خبر
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // تعديل خبر
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", article.CategoryId);
            return View(article);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string category, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Articles
                .Where(a => a.IsPublished && (string.IsNullOrEmpty(category) || a.Category.Name == category))
                .Include(a => a.Category)
                .Include(a => a.Author)
                .OrderByDescending(a => a.PublishDate);

            var articles = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Category = category;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)pageSize);

            return View(articles);
        }

        public async Task<IActionResult> Details(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.Comments).ThenInclude(c => c.Author)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsPublished);

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }
    }
}
