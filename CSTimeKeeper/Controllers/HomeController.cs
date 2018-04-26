using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CSTimeKeeper.Models;
using Microsoft.AspNetCore.Authorization;
using CSTimeKeeper.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CSTimeKeeper.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly TimeKeeperDbContext _context;
        private readonly UserManager<User> _userManager;

        public HomeController(TimeKeeperDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Index -- Dashboard
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            User currentUser = await _userManager.GetUserAsync(User);

            if (currentUser.Administrator)
                return RedirectToAction(nameof(AdminDash));
            else if (currentUser.Instructor)
                return RedirectToAction(nameof(InstructorDash));
            else
                return RedirectToAction(nameof(StudentDash));
        }

        public async Task<IActionResult> StudentDash()
        {
            User currentUser = await _userManager.GetUserAsync(User);
            List<GroupMember> activeGroups = _context.GroupMember.Include(m => m.Project).Where(m => m.StudentId == currentUser.Id).ToList();
            foreach (GroupMember member in activeGroups)
            {
                member.Project.LoadFullCascade(_context);
                _context.Entry(member.Project.Course).Reference(c => c.Instructor).Load();
            }

            List<StudentCourse> activeCourses = _context.StudentCourse.Include(c => c.Course).Include(c => c.Course.Instructor).Where(c => c.StudentId == currentUser.Id).ToList();

            StudentDashViewModel model = new StudentDashViewModel
            {
                StatusMessage = StatusMessage,
                ActiveGroups = activeGroups,
                ActiveCourses = activeCourses
            };

            return View(model);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AdminDash()
        {
            AdminDashViewModel model = new AdminDashViewModel
            {
                InstructorRequestCount = _context.User.Where(u => u.Instructor && !u.Verified).ToList().Count,
                StudentRequestCount = _context.StudentCourse.Where(c => !c.Approved).ToList().Count
            };

            return View(model);
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> InstructorDash()
        {
            User currentUser = await _userManager.GetUserAsync(User);

            InstructorDashViewModel model = new InstructorDashViewModel
            {
                StatusMessage = StatusMessage,
                ActiveCourses = _context.Course.Include(c => c.Projects).Include(c => c.Students)
                    .Where(c => c.InstructorId == currentUser.Id && c.Active).ToList(),
                Instructor = currentUser,
                CourseRegistrations = _context.StudentCourse.Where(r => r.Course.InstructorId == currentUser.Id && !r.Approved).ToList().Count
            };

            return View(model);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
