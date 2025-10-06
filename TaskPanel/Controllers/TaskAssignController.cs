using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskPanel.Data;
using TaskPanel.Models;
namespace TaskPanel.Controllers
{
    [Authorize]
    public class TaskAssignController : Controller
    {
        private readonly DataContext _context;

        public TaskAssignController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> TaskAssign()
        {
            ViewBag.Users = await _context.GenUsers.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TaskAssign(GenTaskAssign Task, IFormFile TaskFile)
        {
            if (ModelState.IsValid)
            {
                if (TaskFile != null && TaskFile.Length > 0)
                {
                    // Ensure folder exists
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "TaskFiles");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique file name to prevent conflicts
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(TaskFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await TaskFile.CopyToAsync(stream);
                    }

                    // Save filename in DB (not full path)
                    Task.CFileName = uniqueFileName;
                }

                _context.GenTaskAssigns.Add(Task);
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewTask");
            }

            ViewBag.Users = await _context.GenUsers.ToListAsync();
            return View(Task);
        }


        [HttpGet]
        public async Task<IActionResult> ViewTask()
        {
            var Task = await _context.GenTaskAssigns.ToListAsync();
            return View(Task);
        }
    }
}
