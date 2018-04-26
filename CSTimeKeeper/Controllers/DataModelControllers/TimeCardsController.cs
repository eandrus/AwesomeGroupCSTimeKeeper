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
    public class TimeCardsController : Controller
    {
        private readonly TimeKeeperDbContext _context;
        private readonly UserManager<User> _userManager;

        [TempData]
        public string StatusMessage { get; set; }

        public TimeCardsController(TimeKeeperDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? id)
        {

            GroupMember member = _context.GroupMember.Include(m => m.Student).FirstOrDefault(m => m.Id == id);

            if (member == null)
                return NotFound();

            User currentUser = await _userManager.GetUserAsync(User);
            member.LoadFullCascade(_context);
            TimeCardsViewModel model;
            //if (currentUser.Administrator || member.StudentCourse.Course.InstructorId == currentUser.Id || member.Project.StudentInProject(currentUser.Id))
            //{

            //}
            //else
            //{
            //    model = new TimeCardsViewModel
            //    {
            //        GroupMember = new GroupMember(),
            //        StatusMessage = "Error: You are not authorized to view this page"
            //    };
            //}

            model = new TimeCardsViewModel
            {
                GroupMember = member,
                StatusMessage = StatusMessage
            };

            return View(model);
        }

        // GET: TimeCards/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var timeEntry = await _context.TimeEntry
                .Include(t => t.GroupMember)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (timeEntry == null)
            {
                return NotFound();
            }

            return View(timeEntry);
        }

        // GET: TimeCards/Create
        public async Task<IActionResult> Create(int? id)
        {
            GroupMember groupMember = _context.GroupMember.Include(m => m.StudentCourse).Include(m => m.Student).FirstOrDefault(m => m.Id == id);
            if (groupMember == null)
            {
                return NotFound();
            }
			TimeEntry model = new TimeEntry();
			model.StopTime = DateTime.Now;
            _context.Entry(groupMember.StudentCourse).Reference(m => m.Course).Load();
            User currentUser = await _userManager.GetUserAsync(User);
            ViewData["GroupMemberId"] = new SelectList(_context.GroupMember.Include(m => m.Student).Where(m => m.Id == groupMember.Id), "Id", "StudentName");
            return View(model);
        }

        // POST: TimeCards/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GroupMemberId,Justification,StartTime,StopTime")] TimeEntry timeEntry)
        {
            User currentUser = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                timeEntry.CreatedBy = currentUser.UserName;

                _context.Add(timeEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction($"Index/{ timeEntry.GroupMemberId }");
            }
            ViewData["GroupMemberId"] = new SelectList(_context.GroupMember.Include(m => m.Student).Where(m => m.Id == timeEntry.GroupMemberId), "Id", "StudentName", timeEntry.GroupMemberId);
            return View(timeEntry);
        }

        // GET: TimeCards/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timeEntry = await _context.TimeEntry.SingleOrDefaultAsync(m => m.Id == id);
            if (timeEntry == null)
            {
                return NotFound();
            }
            ViewData["GroupMemberId"] = new SelectList(_context.GroupMember.Where(m => m.Id == timeEntry.GroupMemberId), "Id", "Id", timeEntry.GroupMemberId);
            return View(timeEntry);
        }

        // POST: TimeCards/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[Authorize(Roles = "Administrator, Instructor")]
		public async Task<IActionResult> Edit(int id, [Bind("Id,GroupMemberId,Justification,StartTime,StopTime,CreatedDate,CreatedBy")] TimeEntry timeEntry)
        {
            if (id != timeEntry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
					_context.Update(timeEntry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TimeEntryExists(timeEntry.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index/" + timeEntry.GroupMemberId);
            }
            ViewData["GroupMemberId"] = new SelectList(_context.GroupMember, "Id", "Id", timeEntry.GroupMemberId);
			User currentUser = await _userManager.GetUserAsync(User);
			return View();
        }

        // GET: TimeCards/Delete/5
        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var timeEntry = await _context.TimeEntry
                .Include(t => t.GroupMember)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (timeEntry == null)
            {
                return NotFound();
            }

            return View(timeEntry);
        }

        [Authorize(Roles = "Administrator, Instructor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var timeEntry = await _context.TimeEntry.Include(t => t.GroupMember).SingleOrDefaultAsync(m => m.Id == id);
            _context.Entry(timeEntry.GroupMember).Reference(t => t.StudentCourse).Load();
            _context.Entry(timeEntry.GroupMember.StudentCourse).Reference(t => t.Course).Load();
            User currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Administrator || timeEntry.GroupMember.StudentCourse.Course.InstructorId == currentUser.Id)
            {
                _context.TimeEntry.Remove(timeEntry);
                await _context.SaveChangesAsync();
            }
            else
            {
                StatusMessage = "You are not authorized to delete that time entry";
            }
            return RedirectToAction($"Index/{ timeEntry.GroupMemberId }");
        }

        private bool TimeEntryExists(int id)
        {
            return _context.TimeEntry.Any(e => e.Id == id);
        }
    }
}
