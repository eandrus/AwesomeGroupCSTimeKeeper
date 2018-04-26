using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CSTimeKeeper.Data;
using CSTimeKeeper.Models;
using Microsoft.AspNetCore.Identity;

namespace CSTimeKeeper.Controllers
{
    public class GroupsController : Controller
    {
        private readonly TimeKeeperDbContext _context;
        private readonly UserManager<User> _userManager;

        [TempData]
        public string StatusMessage { get; set; }

        public GroupsController(TimeKeeperDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Groups
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            project.LoadFullCascade(_context);

            GroupsViewModel model = new GroupsViewModel
            {
                Groups = project.Groups,
                Project = project,
                StatusMessage = StatusMessage
            };

            //model.Groups.Sort();
            return View(model);
        }

        // GET: Groups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await _context.Group
                .Include(g => g.Project)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        // GET: Groups/Create
        public IActionResult Create(int? id)
        {
            Group model = new Group { };

            ViewData["ProjectId"] = new SelectList(_context.Project.Where(p => p.Id == id), "Id", "Name", id);
            return View(model);
        }

        // POST: Groups/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,Name")] Group group)
        {
            if (ModelState.IsValid)
            {
                User currentUser = await _userManager.GetUserAsync(User);
                group.CreatedBy = currentUser.UserName;
                _context.Add(group);
                await _context.SaveChangesAsync();
                StatusMessage = $"Successfully created group \"{group.Name}\"";
                return RedirectToAction($"Index/{group.ProjectId}");
            }
            ViewData["ProjectId"] = new SelectList(_context.Project.Where(p => p.Id == group.ProjectId), "Id", "Description", group.ProjectId);
            return View(group);
        }

        // GET: Groups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await _context.Group.SingleOrDefaultAsync(m => m.Id == id);
            if (group == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", group.ProjectId);
            return View(group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,Name,CreatedBy,CreatedDate")] Group group)
        {
            if (id != group.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(group.Id))
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
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", group.ProjectId);
            return View(group);
        }

        // GET: Groups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var group = await _context.Group
                .Include(g => g.Project)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await _context.Group.SingleOrDefaultAsync(m => m.Id == id);
            _context.Group.Remove(group);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(int id)
        {
            return _context.Group.Any(e => e.Id == id);
        }
    }
}
