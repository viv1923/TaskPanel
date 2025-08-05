using Microsoft.AspNetCore.Mvc;
using TaskPanel.Data;
using TaskPanel.Models;

namespace TaskPanel.Controllers
{
    public class TaskAssignController : Controller
    {
        private readonly DataContext _context;

        public TaskAssignController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult TaskAssign()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> TaskAssign(GenTaskAssign Task)
        {
            if (ModelState.IsValid)
            {
                _context.GenTaskAssigns.Add(Task);
                await _context.SaveChangesAsync();
                return View();
            }

            return View();
        }
    }
}
