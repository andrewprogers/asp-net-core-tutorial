using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;

namespace ContosoUniversity.Controllers
{
    public class CoursesController : Controller
    {
        private SchoolContext _context;

        public CoursesController(SchoolContext context) 
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var courses = 
                _context.Courses
                .Include(c => c.Department)
                .AsNoTracking();
                
            return View(await courses.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) {
                return NotFound();
            }
            var course = await _context.Courses.FindAsync(id);
            if (course == null) {
                return NotFound();
            }
            return View(course);
        }

        [HttpGet]
        public IActionResult Create() {
            PopulateDepartmentsDropDownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseID, Title, Credits, DepartmentID")] Course course) {
            try
            {
                if (ModelState.IsValid) {
                    _context.Add(course);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to create Course. Try again later.");
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }
            var course = await _context.Courses.FindAsync(id);
            if (course == null) {
                return NotFound();
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            return View(course);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id) {
            if (id == null) {
                return NotFound();
            }
            var courseToUpdate = await _context.Courses.FindAsync(id);
            var updateSucceeded = await TryUpdateModelAsync<Course>(
                courseToUpdate,
                "",
                c => c.DepartmentID,
                c => c.Title,
                c => c.Credits
            );

            if (updateSucceeded) {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to update Course. Try again later.");
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDepartmentsDropDownList(courseToUpdate.DepartmentID);
            return View(courseToUpdate);
        }

        private void PopulateDepartmentsDropDownList(int? selectedDepartment = null)
        {
            var departmentQuery =
                from d in _context.Departments
                orderby d.Name
                select d;
            ViewBag.DepartmentID = new SelectList(departmentQuery, "DepartmentID", "Name", selectedDepartment);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id, bool deleteError = false) {
            if (id == null) {
                return NotFound();
            }
            var course = await _context.Courses.Include(c => c.Department).AsNoTracking().SingleOrDefaultAsync(c => c.CourseID == id);
            if (course == null) {
                return NotFound();
            }
            if (deleteError) {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int? id) {
            if (id == null) {
                return NotFound();
            }
            var course = await _context.Courses.AsNoTracking().SingleOrDefaultAsync(c => c.CourseID == id);
            if (course == null) {
                return NotFound();
            }
            try
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                return RedirectToAction(nameof(Delete), new { id=course.CourseID, deleteError = true });
            }
        }
    }
}