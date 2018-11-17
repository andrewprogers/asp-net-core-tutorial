using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Models.SchoolViewModels;

namespace ContosoUniversity.Controllers
{
    public class InstructorsController : Controller
    {
        private SchoolContext _context;

        public InstructorsController(SchoolContext context) 
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? id, int? courseID) {
            var viewModel = new InstructorIndexData();
            viewModel.Instructors = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                    .ThenInclude(i => i.Enrollments)
                    .ThenInclude(i => i.Student)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                    .ThenInclude(i => i.Department)
                .AsNoTracking()
                .OrderBy(i => i.LastName)
                .ToListAsync();

            if (id != null) {
                ViewData["InstructorID"] = id.Value;
                Instructor instructor = viewModel.Instructors.Where(i => i.ID == id.Value).Single();
                viewModel.Courses = instructor.CourseAssignments.Select(ca => ca.Course);
            }
            if (courseID != null) {
                ViewData["CourseID"] = courseID.Value;
                Course course = viewModel.Courses.Where(c => c.CourseID == courseID.Value).Single();
                viewModel.Enrollments = course.Enrollments;
            }
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) {
                return NotFound();
            }
            var instructor = await _context
                .Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments).ThenInclude(c => c.Course)
                .AsNoTracking()
                .SingleOrDefaultAsync(i => i.ID == id);
            if (instructor == null) {
                return NotFound();
            }
            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = _context.Courses;
            var instructorCourses = new HashSet<int>(instructor.CourseAssignments.Select(c => c.CourseID));
            var viewModel = new List<AssignedCourseData>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewData["Courses"] = viewModel;
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Edit")]
        public async Task<IActionResult> EditPost(int? id) 
        {
            if (id == null)
            {
                return NotFound();
            }
            var instructorToupdate = await _context
                .Instructors
                .Include(i => i.OfficeAssignment)
                .SingleOrDefaultAsync(i => i.ID == id);

            var updateSuccess = await TryUpdateModelAsync(
                instructorToupdate,
                "",
                i => i.FirstMidName,
                i => i.LastName,
                i => i.HireDate,
                i => i.OfficeAssignment
            );

            if (updateSuccess) 
            {
                if (String.IsNullOrWhiteSpace(instructorToupdate.OfficeAssignment?.Location)) {
                    instructorToupdate.OfficeAssignment = null;
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists, " +
                    "see your system administrator.");
                }
                return RedirectToAction(nameof(Index));
            }
            return View(instructorToupdate);
        }
    }
}