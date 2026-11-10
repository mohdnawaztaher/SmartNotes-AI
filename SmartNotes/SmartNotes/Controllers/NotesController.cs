
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartNotes.Models;
using SmartNotes.Data;
using SmartNotes.Services;

namespace SmartNotes.Controllers
{
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAIService _openAIService;

        public NotesController(
            ApplicationDbContext context,
            OpenAIService openAIService)
        {
            _context = context;
            _openAIService = openAIService;
        }

        // Display all notes or search by title
        public async Task<IActionResult> Index(string searchString)
        {
            var notes = _context.Notes.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                notes = notes.Where(n => n.Title.Contains(searchString));
            }

            return View(await notes.ToListAsync());
        }

        // Display Create form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Save new note
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Note note)
        {
            if (ModelState.IsValid)
            {
                _context.Notes.Add(note);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(note);
        }

        // Display Edit form
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note == null)
                return NotFound();

            return View(note);
        }

        // Save edited note
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Note note)
        {
            if (id != note.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                _context.Notes.Update(note);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(note);
        }

        // Generate AI summary using Ollama
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Summarize(int id)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note == null)
                return NotFound();

            ViewBag.NoteTitle = note.Title;

            try
            {
                var summary = await _openAIService.SummarizeAsync(note.Content);
                ViewBag.Summary = summary;
            }
            catch (HttpRequestException)
            {
                ViewBag.Summary =
                    "AI service is unavailable. Make sure Ollama is running.";
            }
            catch (TaskCanceledException)
            {
                ViewBag.Summary =
                    "The AI request timed out. Please try again.";
            }

            return View("Summary");
        }

        // Display Delete confirmation page
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var note = await _context.Notes.FindAsync(id);

            if (note == null)
                return NotFound();

            return View(note);
        }

        // Delete note after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
