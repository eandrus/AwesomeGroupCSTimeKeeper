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
using Microsoft.AspNetCore.Authorization;

namespace CSTimeKeeper.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly TimeKeeperDbContext _context;
        private readonly UserManager<User> _userManager;

        [TempData]
        public string StatusMessage { get; set; }

        public ProjectsController(TimeKeeperDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Projects
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
                return NotFound();

            Course course = await _context.Course.FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();

            //Load all the things
            course.LoadFullCascade(_context);

            ProjectsViewModel model = new ProjectsViewModel
            {
                Projects = course.Projects,
                Course = course,
                StatusMessage = StatusMessage
            };
            model.Projects.Sort();

            return View(model);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Course)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create(int? id)
        {
            Project model = new Project
            {
                Deadline = DateTime.Today.AddMonths(1).AddMinutes(-1)
            };

            ViewData["CourseId"] = new SelectList(_context.Course.Where(c => c.Id == id), "Id", "Name", id);
            return View(model);
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> Create([Bind("CourseId,Name,Description,Deadline")] Project project)
        {
            if (ModelState.IsValid)
            {
                //validate user
                User currentUser = await _userManager.GetUserAsync(User);
                if (!currentUser.Administrator && !_context.Course.Any(c => c.Id == project.CourseId && c.InstructorId == currentUser.Id))
                {
                    StatusMessage = "You are not authorized to create a project in this course";
                    return RedirectToAction($"Index/{ project.CourseId }");
                }

                project.CreatedBy = _userManager.GetUserName(User);

                _context.Add(project);
                await _context.SaveChangesAsync();
                StatusMessage = $"Successfully created project \"{ project.Name }\"";
                return RedirectToAction($"Index/{ project.CourseId }");
            }
            ViewData["CourseId"] = new SelectList(_context.Course.Where(c => c.Id == project.CourseId), "Id", "Id", project.CourseId);
            return View(project);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project.SingleOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Course.Where(c => c.Id == project.CourseId), "Id", "Name", project.CourseId);
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CourseId,Name,Description,Deadline")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //validate user
                    User currentUser = await _userManager.GetUserAsync(User);
                    if (!currentUser.Administrator && !_context.Course.Any(c => c.Id == project.CourseId && c.InstructorId == currentUser.Id))
                    {
                        StatusMessage = "You are not authorized to create a project in this course";
                        return RedirectToAction($"Index/{ project.CourseId }");
                    }

                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction($"Index/{ project.CourseId }");
            }
            ViewData["CourseId"] = new SelectList(_context.Course, "Id", "Id", project.CourseId);
            return View(project);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Course)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Project.SingleOrDefaultAsync(m => m.Id == id);

            //validate user
            User currentUser = await _userManager.GetUserAsync(User);
            if (!currentUser.Administrator && !_context.Course.Any(c => c.Id == project.CourseId && c.InstructorId == currentUser.Id))
            {
                StatusMessage = "You are not authorized to create a project in this course";
                return RedirectToAction($"Index/{ project.CourseId }");
            }

            _context.Project.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction($"Index/{ project.CourseId }");
        }

        private bool ProjectExists(int id)
        {
            return _context.Project.Any(e => e.Id == id);
        }
    }
}
