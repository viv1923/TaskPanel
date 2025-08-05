using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskPanel.Data;
using TaskPanel.Models;

namespace TaskPanel.Controllers
{
    public class GenUsersController : Controller
    {
        private readonly DataContext _context;

        public GenUsersController(DataContext context)
        {
            _context = context;
        }

        // GET: GenUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.GenUsers.ToListAsync());
        }

        // GET: GenUsers/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genUser = await _context.GenUsers
                .FirstOrDefaultAsync(m => m.NUserId == id);
            if (genUser == null)
            {
                return NotFound();
            }

            return View(genUser);
        }

        // GET: GenUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GenUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NUserId,CUserName,CPassword,CEmailId,NMobileNo,CDescription")] GenUser genUser)
        {
            if (ModelState.IsValid)
            {
                _context.Add(genUser);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(genUser);
        }

        // GET: GenUsers/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genUser = await _context.GenUsers.FindAsync(id);
            if (genUser == null)
            {
                return NotFound();
            }
            return View(genUser);
        }

        // POST: GenUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("NUserId,CUserName,CPassword,CEmailId,NMobileNo,CDescription")] GenUser genUser)
        {
            if (id != genUser.NUserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(genUser);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GenUserExists(genUser.NUserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(genUser);
        }

        // GET: GenUsers/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genUser = await _context.GenUsers
                .FirstOrDefaultAsync(m => m.NUserId == id);
            if (genUser == null)
            {
                return NotFound();
            }

            return View(genUser);
        }

        // POST: GenUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var genUser = await _context.GenUsers.FindAsync(id);
            if (genUser != null)
            {
                _context.GenUsers.Remove(genUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GenUserExists(long id)
        {
            return _context.GenUsers.Any(e => e.NUserId == id);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.GenUsers
                    .FirstOrDefaultAsync(u => u.CUserName == model.CUserName && u.CPassword == model.CPassword);

                if (user != null)
                {
                    // Placeholder for session/auth logic
                    HttpContext.Session.SetString("Username", user.CUserName);

                    return RedirectToAction("Index", "Home"); // or wherever you want after login
                }

                ModelState.AddModelError(string.Empty, "Invalid username or password");
            }

            return View(model);
        }
    }
}
