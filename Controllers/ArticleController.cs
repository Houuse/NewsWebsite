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
    public class ArticleController(ApplicationDbContext context) : Controller
    {
        // عرض صفحة إضافة الخبر
        [Authorize(Roles = "Admin,Editor,Author")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }


        // عرض قائمة الأخبار
        public async Task<IActionResult> Index(string category, bool? isPublished, int page = 1)
        {
            int pageSize = 6; // عدد المقالات بكل صفحة

            var query = context.Articles
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

            int totalArticles = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);

            var articles = await query
                .OrderByDescending(a => a.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Categories = new SelectList(await context.Categories.ToListAsync(), "Name", "Name");
            ViewBag.Category = category;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.IsPublished = isPublished;

            return View(articles);
        }


        // حذف خبر
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Editor,Author")]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            context.Articles.Remove(article);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string category, int page = 1)
        {
            int pageSize = 10;
            var query = context.Articles
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
            var article = await context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.Comments).ThenInclude(c => c.Author)
                .Include(a => a.Tags)
                .FirstOrDefaultAsync(a => a.Id == id && a.IsPublished);

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Editor,Author")]
        public async Task<IActionResult> Create(Article article, IFormFile? image, IFormFile? video, IFormFile? audio)
        {
            if (ModelState.IsValid)
            {
                // Handle file uploads (simplified for local storage; use AWS S3 in production)
                if (image != null)
                {
                    var imagePath = Path.Combine("wwwroot/uploads/images",
                        Guid.NewGuid().ToString() + Path.GetExtension(image.FileName));
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    article.ImageUrl = "/" + imagePath.Replace("wwwroot/", "");
                }
                // Similar handling for video and audio

                article.PublishDate = DateTime.UtcNow;
                article.AuthorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                context.Articles.Add(article);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await context.Categories.ToListAsync(), "Id", "Name");
            return View(article);
        }
        [Authorize(Roles = "Admin,Editor,Author")]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            ViewBag.Categories =
                new SelectList(await context.Categories.ToListAsync(), "Id", "Name", article.CategoryId);
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Editor,Author")]
        public async Task<IActionResult> Edit(int id, Article article, IFormFile? image, IFormFile? video,
            IFormFile? audio)
        {
            if (id != article.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle file uploads similarly to Create
                    var existingArticle = await context.Articles.FindAsync(id);
                    existingArticle.Title = article.Title;
                    existingArticle.Content = article.Content;
                    existingArticle.CategoryId = article.CategoryId;
                    existingArticle.IsPublished = article.IsPublished;
                    // Update ImageUrl, VideoUrl, AudioUrl if new files uploaded

                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await context.Articles.AnyAsync(a => a.Id == id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories =
                new SelectList(await context.Categories.ToListAsync(), "Id", "Name", article.CategoryId);
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Editor,Author")]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var article = await context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            article.IsPublished = !article.IsPublished;
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}