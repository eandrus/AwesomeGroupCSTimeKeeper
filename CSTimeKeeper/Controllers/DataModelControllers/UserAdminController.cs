using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CSTimeKeeper.Data;
using CSTimeKeeper.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace CSTimeKeeper
{
    [Authorize(Roles = "Administrator")]
    public class UserAdminController : Controller
    {
        private readonly TimeKeeperDbContext _context;
        private readonly UserManager<User> _userManager;

        [TempData]
        public string StatusMessage { get; set; }

        public UserAdminController(TimeKeeperDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UserAdmin
        public async Task<IActionResult> Index()
        {
            List<User> userList = await _context.User.ToListAsync();
            userList.Sort();
            return View(userList);
        }

        public async Task<IActionResult> InstructorRequests()
        {
            List<User> requestList = await _context.User.Where(u => u.Instructor && !u.Verified).ToListAsync();
            requestList.Sort();

            InstructorRequestsViewModel model = new InstructorRequestsViewModel
            {
                Requests = requestList,
                StatusMessage = StatusMessage
            };

            return View(model);
        }

        public async Task<IActionResult> ApproveRequest(string id)
        {
            User user = _context.User.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            user.Verified = true;
            await _context.SaveChangesAsync();
            StatusMessage = $"Approved Instructor Request for { user.FullName }";
            return RedirectToAction(nameof(InstructorRequests));
        }

        public async Task<IActionResult> DenyRequest(string id)
        {
            User user = _context.User.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            user.Instructor = false;
            //switch roles
            await _userManager.AddToRoleAsync(user, "Student");
            await _userManager.RemoveFromRoleAsync(user, "Instructor");

            await _context.SaveChangesAsync();
            StatusMessage = $"Denied Instructor Request for { user.FullName }. This user is now a Student";
            return RedirectToAction(nameof(InstructorRequests));
        }

        public async Task<IActionResult> ApproveAllRequests()
        {
            List<User> allRequests = _context.User.Where(u => u.Instructor && !u.Verified).ToList();
            if (allRequests == null || allRequests.Count == 0)
                return NotFound();
            
            foreach (User user in allRequests)
                user.Verified = true;

            await _context.SaveChangesAsync();
            StatusMessage = $"Approved Instructor Request for { allRequests.Count } Users";
            return RedirectToAction(nameof(InstructorRequests));
        }

        // GET: UserAdmin/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .SingleOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: UserAdmin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserAdmin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Administrator,Instructor,Verified,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: UserAdmin/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.SingleOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: UserAdmin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("FirstName,LastName,Administrator,Instructor,Verified,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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
            return View(user);
        }

        // GET: UserAdmin/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .SingleOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: UserAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.User.SingleOrDefaultAsync(m => m.Id == id);
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}
