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
            // Load users for dropdown (still used for NToUser)
            ViewBag.Users = await _context.GenUsers.ToListAsync();

            // Get current logged-in user info from claims or cookies
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var userName = User.Identity?.Name;

            // Pre-fill the model
            var model = new GenTaskAssign
            {
                NFromUser = Convert.ToInt32(userId),
                DTaskDate = DateTime.Now // current date
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TaskAssign(GenTaskAssign Task, IFormFile? TaskFile)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .Select(e => new { Field = e.Key, Errors = e.Value.Errors.Select(er => er.ErrorMessage) })
                    .ToList();

                Console.WriteLine("Model validation failed:");
                foreach (var err in errors)
                    Console.WriteLine($"{err.Field}: {string.Join(", ", err.Errors)}");

                ViewBag.Users = await _context.GenUsers.ToListAsync();
                return View(Task);
            }

            if (ModelState.IsValid)
            {
                if (TaskFile != null && TaskFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "TaskFiles");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(TaskFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await TaskFile.CopyToAsync(stream);
                    }

                    Task.CFileName = uniqueFileName;
                }

                // Override FromUser just in case someone tampers with form
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                Task.NFromUser = Convert.ToInt32(userId);

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
            var today = DateTime.Now.Date;
            var taskList = await _context.GenTaskAssigns
                .Select(t => new TaskWithUserVM
                {
                    NTaskNo = t.NTaskNo,
                    CTask = t.CTask,
                    DTaskDate = t.DTaskDate,
                    DDeadLine = t.DDeadLine,
                    CFileName = t.CFileName,
                    FromUserName = _context.GenUsers
                                    .Where(u => u.NUserId == t.NFromUser)
                                    .Select(u => u.CUserName)
                                    .FirstOrDefault(),
                    ToUserName = _context.GenUsers
                                    .Where(u => u.NUserId == t.NToUser)
                                    .Select(u => u.CUserName)
                                    .FirstOrDefault(),
                    //logic for due status
                    DueStatusColor = t.DDeadLine == null ? "bg-secondary text-white"
                                    : t.DDeadLine.Value.Date < today ? "bg-danger text-white"
                                    : t.DDeadLine.Value.Date == today ? "bg-warning text-dark"
                                    : "bg-success text-white",

                    DueStatusText = t.DDeadLine == null ? "No Due Date"
                                    : t.DDeadLine.Value.Date < today ? $"Overdue: {t.DDeadLine.Value:dd-MM-yyyy}"
                                    : t.DDeadLine.Value.Date == today ? "Due Today"
                                    : t.DDeadLine.Value.ToString("dd-MM-yyyy")
                })
                .ToListAsync();

            return View(taskList);
        }

        [HttpGet]
        public IActionResult DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Upload", "TaskFiles", fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType, fileName);
        }

        [HttpPost]
        public IActionResult DeleteTask(int id)
        {
            var task = _context.GenTaskAssigns.FirstOrDefault(t => t.NTaskNo == id);
            if (task == null)
                return Json(new { success = false, message = "Task not found" });

            _context.GenTaskAssigns.Remove(task);
            _context.SaveChanges();

            return Json(new { success = true });
        }

    }
}
