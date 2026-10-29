using AIResumeBuilder.Data;
using AIResumeBuilder.Models;
using AIResumeBuilder.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIResumeBuilder.Controllers;

[Authorize]
public class ResumesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _users;
    private readonly GeminiResumeService _ai;
    public ResumesController(ApplicationDbContext db, UserManager<IdentityUser> users, GeminiResumeService ai)
    { _db = db; _users = users; _ai = ai; }

    public async Task<IActionResult> Index() =>
        View(await _db.Resumes.Where(r => r.UserId == _users.GetUserId(User)).OrderByDescending(r => r.UpdatedAt).ToListAsync());

    public IActionResult Create() => View("Edit", new Resume());

    public async Task<IActionResult> Edit(int id)
    {
        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == _users.GetUserId(User));
        return resume == null ? NotFound() : View(resume);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(Resume model)
    {
        ModelState.Remove(nameof(model.UserId));
        if (!ModelState.IsValid) return View("Edit", model);
        var userId = _users.GetUserId(User)!;
        if (model.Id == 0)
        {
            model.UserId = userId;
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
            _db.Resumes.Add(model);
        }
        else
        {
            var existing = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == model.Id && r.UserId == userId);
            if (existing == null) return NotFound();
            existing.Title = model.Title; existing.FullName = model.FullName; existing.Email = model.Email;
            existing.Phone = model.Phone; existing.Location = model.Location; existing.LinkedInUrl = model.LinkedInUrl;
            existing.PortfolioUrl = model.PortfolioUrl; existing.Summary = model.Summary; existing.Education = model.Education;
            existing.Experience = model.Experience; existing.Projects = model.Projects; existing.Skills = model.Skills;
            existing.Certifications = model.Certifications; existing.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Preview), new { id = model.Id == 0 ? _db.Resumes.Local.Last().Id : model.Id });
    }

    public async Task<IActionResult> Preview(int id)
    {
        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == _users.GetUserId(User));
        return resume == null ? NotFound() : View(resume);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var resume = await _db.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == _users.GetUserId(User));
        if (resume != null) { _db.Resumes.Remove(resume); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    public record AiRequest(string Task, string Content);
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Improve([FromBody] AiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length > 8000)
            return BadRequest(new { error = "Enter text between 1 and 8,000 characters." });
        try { return Json(new { text = await _ai.ImproveAsync(request.Task, request.Content, HttpContext.RequestAborted) }); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { error = ex.Message }); }
    }
}
