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
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int currentUserId = Convert.ToInt32(userId);

            var taskList = await _context.GenTaskAssigns
                .Where(t => (!t.NTaskType.HasValue || t.NTaskType == 0) && (t.NComplete == null || t.NComplete == false) && t.DCompleteDate == null)
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
                                    .Where(u => u.NUserId == currentUserId)
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

        [HttpPost]
        public IActionResult CompleteTasks(List<int> selectedTaskIds)
        {
            if (selectedTaskIds == null || !selectedTaskIds.Any())
                return RedirectToAction("ViewTask");

            var tasks = _context.GenTaskAssigns
                .Where(t => selectedTaskIds.Contains(t.NTaskNo))
                .ToList();

            foreach (var task in tasks)
            {
                task.NComplete = true;              // bool column
                task.DCompleteDate = DateTime.Now;  // completion timestamp
            }

            _context.SaveChanges();

            return RedirectToAction("ViewTask");   // reload page
        }




        // --------------------------------------------
        // 📌 DAILY TASK (Multiple Add + Single Parent Entry)
        // --------------------------------------------

        [HttpPost]
        public async Task<IActionResult> SaveDailyTasks([FromBody] List<string> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                return Json(new { success = false, message = "Please add at least one task." });

            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "User not logged in." });

            int currentUserId = Convert.ToInt32(userId);
            var today = DateTime.Now.Date;

            // Step 1️⃣ - Check if today's self-task entry already exists
            var existingTaskAssign = await _context.GenTaskAssigns
                .FirstOrDefaultAsync(t => t.NFromUser == currentUserId &&
                                          t.NToUser == currentUserId &&
                                          t.DTaskDate.Value.Date == today);

            if (existingTaskAssign == null)
            {
                // Create a new parent entry
                existingTaskAssign = new GenTaskAssign
                {
                    NFromUser = currentUserId,
                    NToUser = currentUserId,
                    NTaskType = 1,
                    CTask = "Self Task",
                    DTaskDate = today
                };

                _context.GenTaskAssigns.Add(existingTaskAssign);
                await _context.SaveChangesAsync(); // to get new NTaskNo
            }

            // Step 2️⃣ - Add child tasks linked to same parent
            foreach (var taskText in tasks)
            {
                if (!string.IsNullOrWhiteSpace(taskText))
                {
                    var newTask = new GenDailyTask
                    {
                        NTask = existingTaskAssign.NTaskNo,
                        CDailyTask = taskText.Trim()
                    };
                    _context.GenDailyTasks.Add(newTask);
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Daily tasks saved successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetTodayDailyTasks()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "User not logged in." });

            int currentUserId = Convert.ToInt32(userId);
            var today = DateTime.Now.Date;

            // Get today's parent
            var todayTaskAssign = await _context.GenTaskAssigns
                .FirstOrDefaultAsync(t => t.NFromUser == currentUserId &&
                                          t.NToUser == currentUserId &&
                                          t.DTaskDate.Value.Date == today);

            if (todayTaskAssign == null)
                return Json(new { success = true, data = new List<object>() });

            // Get child daily tasks
            var dailyTasks = await _context.GenDailyTasks
                .Where(t => t.NTask == todayTaskAssign.NTaskNo)
                .Select(t => new { t.NUcode, t.CDailyTask })
                .ToListAsync();

            return Json(new { success = true, data = dailyTasks });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDailyTask(int id)
        {
            var task = await _context.GenDailyTasks.FindAsync(id);
            if (task == null)
                return Json(new { success = false, message = "Task not found." });

            _context.GenDailyTasks.Remove(task);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Task deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> EditDailyTask(int id, string CDailyTask)
        {
            var task = await _context.GenDailyTasks.FindAsync(id);
            if (task == null)
                return Json(new { success = false, message = "Task not found." });

            task.CDailyTask = CDailyTask;
            _context.GenDailyTasks.Update(task);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Task updated successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetDailyTasksForCalander(DateTime date)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "User not logged in." });

            int currentUserId = Convert.ToInt32(userId);
            // Convert date to yyyy-MM-dd format
            string formattedDate = date.ToString("yyyy-MM-dd");

            // Step 1: Get the self-assigned task for the selected date
            var sqlAssign = @"
        SELECT TOP 1 * 
        FROM GenTaskAssign
        WHERE NFromUser = {0} 
          AND NToUser = {0} 
          AND CAST(DTaskDate AS DATE) = {1}";

            var taskAssign = await _context.GenTaskAssigns
                .FromSqlRaw(sqlAssign, currentUserId, formattedDate)
                .FirstOrDefaultAsync();

            if (taskAssign == null)
                return Json(new { success = true, data = new List<object>() });

            // Step 2: Get related daily subtasks using raw SQL
            var sqlDailyTasks = @"
                SELECT NUcode, CDailyTask 
                FROM Gen_DailyTask 
                WHERE NTask = {0}";

            var dailyTasks = await _context.GenDailyTasks
                .FromSqlRaw(sqlDailyTasks, taskAssign.NTaskNo)
                .Select(t => new { t.NUcode, t.CDailyTask })
                .ToListAsync();

            return Json(new { success = true, data = dailyTasks });
        }





    }
}
