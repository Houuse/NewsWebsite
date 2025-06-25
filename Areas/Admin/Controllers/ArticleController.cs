using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NewsWebsite.Data;
using NewsWebsite.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NewsWebsite.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin,Editor")]
    public class ArticleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArticleController(ApplicationDbContext context)
        {
            _context = context;
        }

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

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            var article = new Article();
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Article article, IFormFile? image, IFormFile? video, IFormFile? audio)
        {
            if (ModelState.IsValid)
            {
                // Handle file uploads (simplified for local storage; use AWS S3 in production)
                if (image != null)
                {
                    var imagePath = Path.Combine("wwwroot/uploads/images", Guid.NewGuid().ToString() + Path.GetExtension(image.FileName));
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    article.ImageUrl = "/" + imagePath.Replace("wwwroot/", "");
                }
                // Similar handling for video and audio

                article.PublishDate = DateTime.UtcNow;
                article.AuthorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                _context.Articles.Add(article);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View(article);
        }

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
        public async Task<IActionResult> Edit(int id, Article article, IFormFile? image, IFormFile? video, IFormFile? audio)
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
                    var existingArticle = await _context.Articles.FindAsync(id);
                    existingArticle.Title = article.Title;
                    existingArticle.Content = article.Content;
                    existingArticle.CategoryId = article.CategoryId;
                    existingArticle.IsPublished = article.IsPublished;
                    // Update ImageUrl, VideoUrl, AudioUrl if new files uploaded

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Articles.AnyAsync(a => a.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", article.CategoryId);
            return View(article);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            article.IsPublished = !article.IsPublished;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}