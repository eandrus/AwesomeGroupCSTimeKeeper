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
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly TimeKeeperDbContext _context;
        private readonly UserManager<User> _userManager;

        [TempData]
        public string StatusMessage { get; set; }

        public CoursesController(TimeKeeperDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            User currentUser = await _userManager.GetUserAsync(User);
            List<Course> allCourses = await _context.Course.Include(c => c.Students).Include(c => c.Instructor).ToListAsync();

            List<Course> myCourses;
            List<Course> myPastCourses;
            List<Course> availableCourses;
            List<Course> pastCourses;
            List<Course> pendingRegistrations;

            if (currentUser.Instructor)
            {
                myCourses = allCourses.Where(c => c.InstructorId == currentUser.Id && c.Active).ToList();
                myPastCourses = allCourses.Where(c => c.InstructorId == currentUser.Id && !c.Active).ToList();
                availableCourses = allCourses.Where(c => c.InstructorId != currentUser.Id && c.Active).ToList();
                pastCourses = allCourses.Where(c => c.InstructorId != currentUser.Id && !c.Active).ToList();
                pendingRegistrations = new List<Course>();
            }
            else if (currentUser.Administrator)
            {
                myCourses = new List<Course>();
                myPastCourses = new List<Course>();
                availableCourses = allCourses.Where(c => c.Active).ToList();
                pastCourses = allCourses.Where(c => !c.Active).ToList();
                pendingRegistrations = new List<Course>();
            }
            else //Student
            {
                List<StudentCourse> myRegistrations = _context.StudentCourse.Where(c => c.StudentId == currentUser.Id).Include(r => r.Course).ToList();
                myCourses = new List<Course>();
                myPastCourses = new List<Course>();
                pendingRegistrations = new List<Course>();

                foreach (StudentCourse registration in myRegistrations)
                {
                    if (registration.Approved)
                    {
                        if (registration.Course.Active)
                            myCourses.Add(registration.Course);
                        else
                            myPastCourses.Add(registration.Course);
                    }
                    else
                    {
                        pendingRegistrations.Add(registration.Course);
                    }
                }

                availableCourses = allCourses.Where(c => c.Active && !myCourses.Contains(c) && !pendingRegistrations.Contains(c)).ToList();
                pastCourses = new List<Course>();
            }


            CoursesViewModel model = new CoursesViewModel
            {
                CurrentUser = currentUser,
                MyCourses = myCourses,
                MyPastCourses = myPastCourses,
                AvailableCourses = availableCourses,
                PastCourses = pastCourses,
                PendingRegistrations = pendingRegistrations,
                StatusMessage = StatusMessage
            };

            return View(model);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Register(int id)
        {
            User currentUser = await _userManager.GetUserAsync(User);
            Course course = await _context.Course.FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound();
            else if (!course.Active)
                StatusMessage = "Error: Cannot register for an Inactive Course";
            else if (_context.StudentCourse.Any(c => c.StudentId == currentUser.Id && c.CourseId == id))
                StatusMessage = "Error: You have already registered for this course";
            else
            {
                StudentCourse registration = new StudentCourse
                {
                    CourseId = id,
                    StudentId = currentUser.Id
                };

                _context.StudentCourse.Add(registration);

                try
                {
                    await _context.SaveChangesAsync();
                    StatusMessage = $"Successfully registered for Course: { course.Name }";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: { ex.Message }";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> CourseRequests()
        {
            List<StudentCourse> requests = await _context.StudentCourse.Where(c => !c.Approved).Include(c => c.Course).Include(c => c.Student).ToListAsync();
            User currentUser = await _userManager.GetUserAsync(User);

            if (currentUser.Instructor)
            {
                requests = requests.Where(c => c.Course.InstructorId == currentUser.Id).ToList();
                foreach (StudentCourse request in requests)
                    request.Course.Instructor = currentUser;
            }
            else
            {
                List<User> instructors = _context.User.Where(u => u.Instructor).ToList();
                foreach (StudentCourse request in requests)
                    request.Course.Instructor = instructors.FirstOrDefault(i => i.Id == request.Course.InstructorId);
            }

            requests.Sort();

            CourseRequestsViewModel model = new CourseRequestsViewModel
            {
                Requests = requests,
                StatusMessage = StatusMessage
            };

            return View(model);
        }

        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            StudentCourse request = await _context.StudentCourse.Include(r => r.Course).Include(r => r.Student).FirstOrDefaultAsync(r => r.Id == id);
            User currentUser = await _userManager.GetUserAsync(User);

            if (request == null)
                return NotFound();

            //only allow admin or instructor to approve
            if (!currentUser.Administrator && request.Course.InstructorId != currentUser.Id)
            {
                StatusMessage = "Error: You are not authorized to approve this course request";
                return RedirectToAction(nameof(CourseRequests));
            }

            try
            {
                request.Approved = true;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: { ex.Message }";
                return RedirectToAction(nameof(CourseRequests));
            }

            StatusMessage = $"Successfully approved course request for student { request.Student.FullName }";
            return RedirectToAction(nameof(CourseRequests));
        }

        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> DenyRequest(int id)
        {
            StudentCourse request = await _context.StudentCourse.Include(r => r.Course).Include(r => r.Student).FirstOrDefaultAsync(r => r.Id == id);
            User currentUser = await _userManager.GetUserAsync(User);

            if (request == null)
                return NotFound();

            //only allow admin or instructor to approve
            if (!currentUser.Administrator && request.Course.InstructorId != currentUser.Id)
            {
                StatusMessage = "Error: You are not authorized to deny this course request";
                return RedirectToAction(nameof(CourseRequests));
            }

            try
            {
                _context.Remove(request);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: { ex.Message }";
                return RedirectToAction(nameof(CourseRequests));
            }

            StatusMessage = $"Successfully denied course request for student { request.Student.FullName }";
            return RedirectToAction(nameof(CourseRequests));
        }

        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> ApproveAllRequests()
        {
            User currentUser = await _userManager.GetUserAsync(User);
            List<StudentCourse> requests;

            if (currentUser.Administrator)
                requests = await _context.StudentCourse
                    .Include(r => r.Course).Include(r => r.Student)
                    .Where(r => !r.Approved).ToListAsync();
            else
                requests = await _context.StudentCourse
                    .Include(r => r.Course).Include(r => r.Student)
                    .Where(r => r.Course.InstructorId == currentUser.Id && !r.Approved).ToListAsync();

            try
            {
                foreach (StudentCourse request in requests)
                {
                    request.Approved = true;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: { ex.Message }";
                return RedirectToAction(nameof(CourseRequests));
            }

            StatusMessage = $"Successfully approved course requests for { requests.Count } students";
            return RedirectToAction(nameof(CourseRequests));
        }

        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.Instructor)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> Create()
        {
            User currentUser = await _userManager.GetUserAsync(User);
            ViewData["InstructorId"] = 
                new SelectList(currentUser.Administrator ? _context.User.Where(u => u.Instructor && u.Verified) : _context.User.Where(u => u.Id == currentUser.Id)
                , "Id", "FullName");
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> Create([Bind("Id,InstructorId,Name")] Course course)
        {
            if (ModelState.IsValid)
            {
                course.Name = course.Name.Trim();

                //Validate unique course name
                if (_context.Course.Any(c => c.InstructorId == course.InstructorId && c.Name.ToLower().Equals(course.Name.ToLower())))
                {
                    StatusMessage = "Error: A course with that name exists. Could not create Course";
                    return RedirectToAction(nameof(Index));
                }

                User user = await _userManager.GetUserAsync(User);
                course.CreatedBy = user.UserName;
                _context.Add(course);
                await _context.SaveChangesAsync();
                StatusMessage = $"Added Course { course.Name }";
                return RedirectToAction(nameof(Index));
            }
            ViewData["InstructorId"] = new SelectList(_context.User, "Id", "Id", course.InstructorId);
            return View(course);
        }

        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course.SingleOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            User currentUser = await _userManager.GetUserAsync(User);
            ViewData["InstructorId"] =
                new SelectList(currentUser.Administrator ? _context.User.Where(u => u.Instructor && u.Verified) : _context.User.Where(u => u.Id == currentUser.Id)
                , "Id", "FullName", course.InstructorId);
            return View(course);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InstructorId,Name,Active")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = $"Saved changes to Course { course.Name }";
                return RedirectToAction(nameof(Index));
            }
            User currentUser = await _userManager.GetUserAsync(User);
            ViewData["InstructorId"] =
                new SelectList(currentUser.Administrator ? _context.User.Where(u => u.Instructor && u.Verified) : _context.User.Where(u => u.Id == currentUser.Id)
                , "Id", "FullName", course.InstructorId);
            return View(course);
        }

        [Authorize(Roles = "Administrator, Instructor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.Instructor)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [Authorize(Roles = "Administrator, Instructor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Course.SingleOrDefaultAsync(m => m.Id == id);
            _context.Course.Remove(course);
            await _context.SaveChangesAsync();
            StatusMessage = "Successfully deleted course";
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.Id == id);
        }
    }
}
