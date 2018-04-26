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
    public class GroupMembersController : Controller
    {
        private readonly TimeKeeperDbContext _context;
		[TempData]
		public string StatusMessage { get; set; }
		private readonly UserManager<User> _userManager;

        public GroupMembersController(TimeKeeperDbContext context, UserManager<User> userManager)
        {
            _context = context;
			_userManager = userManager;
        }

        // GET: GroupMembersyeah 
        public async Task<IActionResult> Index(int? id)
        {
			if (id == null)
			{
				return NotFound();
			}
			Group group = _context.Group.FirstOrDefault(g => g.Id == id);
			group.LoadFullCascade(_context);

			GroupMembersViewModel model = new GroupMembersViewModel();
			User currentUser = await _userManager.GetUserAsync(User);
			model.userInGroup = _context.GroupMember.Any(m => m.StudentId == currentUser.Id && m.GroupId == group.Id);
			model.group = group;
			model.StatusMessage = StatusMessage;
			model.currentUser = await _userManager.GetUserAsync(User);
			return View(model);
		}

        // GET: GroupMembers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMember
                .Include(g => g.Group)
                .Include(g => g.Project)
                .Include(g => g.Student)
                .Include(g => g.StudentCourse)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (groupMember == null)
            {
                return NotFound();
            }

            return View(groupMember);
        }

        // GET: GroupMembers/Create
        public async Task<IActionResult> Create(int? id)
        {
			User currentUser = await _userManager.GetUserAsync(User);
			Group group = _context.Group.Include(c => c.Project).FirstOrDefault(g => g.Id == id);
			Course course = _context.Course.FirstOrDefault(c => c.Id == group.Project.CourseId);
			List<StudentCourse> list = _context.StudentCourse.Where(c => c.CourseId == course.Id).ToList();
			List<string> ids = new List<string>();
			foreach(StudentCourse thing in list)
			{
				ids.Add(thing.StudentId);
			}
            ViewData["GroupId"] = new SelectList(_context.Group.Where(g => g.Id == id), "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Project.Where(p => p.Id == group.ProjectId), "Id", "Name");
			if(currentUser.Administrator || currentUser.Instructor)
			{
				ViewData["StudentId"] = new SelectList(_context.User.Where(u => ids.Contains(u.Id)), "Id", "FullName");
			}
			else
			{
				ViewData["StudentId"] = new SelectList(_context.User.Where(u => u.Id == currentUser.Id), "Id", "FullName");
			}
            return View();
        }

        // POST: GroupMembers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentId,GroupId,ProjectId")] GroupMember groupMember)
        {
            if (ModelState.IsValid)
            {
				var exists = _context.GroupMember.FirstOrDefault(g => g.StudentId == groupMember.StudentId && g.ProjectId == groupMember.ProjectId);
				if (exists != null)
				{
					exists.GroupId = groupMember.GroupId;
					_context.Update(exists);
				}
				else
				{
					groupMember.StudentCourseId = _context.StudentCourse.FirstOrDefault(s => s.StudentId == groupMember.StudentId).Id;
					_context.Add(groupMember);
				}
                await _context.SaveChangesAsync();
                return RedirectToAction("Index/" + groupMember.GroupId);
            }
			User currentUser = await _userManager.GetUserAsync(User);
			Group group = _context.Group.Include(c => c.Project).FirstOrDefault(g => g.Id == groupMember.GroupId);
			Course course = _context.Course.FirstOrDefault(c => c.Id == group.Project.CourseId);
			List<StudentCourse> list = _context.StudentCourse.Where(c => c.CourseId == course.Id).ToList();
			List<string> ids = new List<string>();
			foreach (StudentCourse thing in list)
			{
				ids.Add(thing.StudentId);
			}
			ViewData["GroupId"] = new SelectList(_context.Group.Where(g => g.Id == groupMember.GroupId), "Id", "Name");
			ViewData["ProjectId"] = new SelectList(_context.Project.Where(p => p.Id == group.ProjectId), "Id", "Name");
			if (currentUser.Administrator || currentUser.Instructor)
			{
				ViewData["StudentId"] = new SelectList(_context.User.Where(u => ids.Contains(u.Id)), "Id", "FullName");
			}
			else
			{
				ViewData["StudentId"] = new SelectList(_context.User.Where(u => u.Id == currentUser.Id), "Id", "FullName");
			}
			//
			return View(groupMember);
        }

        // GET: GroupMembers/Edit/5
      /*  public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var groupMember = await _context.GroupMember.SingleOrDefaultAsync(m => m.Id == id);
            if (groupMember == null)
            {
                return NotFound();
            }
            ViewData["GroupId"] = new SelectList(_context.Group, "Id", "Name", groupMember.GroupId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", groupMember.ProjectId);
            ViewData["StudentId"] = new SelectList(_context.User, "Id", "Id", groupMember.StudentId);
            ViewData["StudentCourseId"] = new SelectList(_context.StudentCourse, "Id", "StudentId", groupMember.StudentCourseId);
            return View(groupMember);
        }

        // POST: GroupMembers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StudentId,StudentCourseId,GroupId,ProjectId")] GroupMember groupMember)
        {
            if (id != groupMember.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(groupMember);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupMemberExists(groupMember.Id))
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
            ViewData["GroupId"] = new SelectList(_context.Group, "Id", "Name", groupMember.GroupId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", groupMember.ProjectId);
            ViewData["StudentId"] = new SelectList(_context.User, "Id", "Id", groupMember.StudentId);
            ViewData["StudentCourseId"] = new SelectList(_context.StudentCourse, "Id", "StudentId", groupMember.StudentCourseId);
            return View(groupMember);
        }*/

        // GET: GroupMembers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var groupMember = await _context.GroupMember
                .Include(g => g.Group)
                .Include(g => g.Project)
                .Include(g => g.Student)
                .Include(g => g.StudentCourse)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (groupMember == null)
            {
                return NotFound();
            }

            return View(groupMember);
        }

        // POST: GroupMembers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
			User currentUser = await _userManager.GetUserAsync(User);
			if (currentUser.Administrator || currentUser.Instructor)
			{
				var groupMember = await _context.GroupMember.SingleOrDefaultAsync(m => m.Id == id);
				_context.GroupMember.Remove(groupMember);
				await _context.SaveChangesAsync();
			}
            return RedirectToAction(nameof(Index));
        }

        private bool GroupMemberExists(int id)
        {
            return _context.GroupMember.Any(e => e.Id == id);
        }
    }
}
