using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Controllers
{
    public class DepartmentsController : Controller
    {
        private SchoolContext _context { get; set; }

        public DepartmentsController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            System.Console.WriteLine("Index");
            List<Department> departments = await _context
                .Departments
                .Include(d => d.Administrator)
                .AsNoTracking()
                .ToListAsync();

            return View(departments);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context
                .Departments
                .Include(d => d.Administrator)
                .AsNoTracking()
                .SingleOrDefaultAsync(d => d.DepartmentID == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PopulateInstructorsDropdownList();
            return View(new Department());
        }

        private void PopulateInstructorsDropdownList(int? selectedInstructor = null)
        {
            var instructors = _context
                .Instructors
                .AsNoTracking()
                .OrderBy(i => i.LastName)
                .Select(i => i);

            ViewBag.InstructorID = new SelectList(instructors, "ID", "FullName", selectedInstructor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name, Budget, StartDate, InstructorID")] Department department)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(department);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception)
                {
                    ModelState.AddModelError("", "Failed to create department, please try again later.");
                }
            }
            PopulateInstructorsDropdownList();
            return View(department);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context
                .Departments
                .Include(d => d.Administrator)
                .AsNoTracking()
                .SingleOrDefaultAsync(d => d.DepartmentID == id);
            if (department == null)
            {
                return NotFound();
            }
            PopulateInstructorsDropdownList(department.InstructorID);
            return View(department);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, uint xmin)
        {
            if (id == null)
            {
                return NotFound();
            }

            var departmentToUpdate = await _context
                .Departments
                .Include(d => d.Administrator)
                .SingleOrDefaultAsync(d => d.DepartmentID == id);

            if (departmentToUpdate == null)
            {
                var deletedDepartment = new Department();
                await TryUpdateModelAsync(deletedDepartment);
                ModelState.AddModelError("", "Unable to Save changes, the department was deleted by another user.");
                PopulateInstructorsDropdownList(deletedDepartment.InstructorID);
                return View(deletedDepartment);
            }

            _context.Entry(departmentToUpdate).Property("xmin").OriginalValue = xmin;

            var result = await TryUpdateModelAsync(
                departmentToUpdate,
                "",
                a => a.Name,
                a => a.Budget,
                a => a.StartDate,
                a => a.InstructorID
            );

            if (result)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Department)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError("", "Unable to save changes, the department was deleted by another user");
                    }
                    else
                    {
                        var databaseValues = (Department)databaseEntry.ToObject();

                        if (databaseValues.Name != clientValues.Name)
                        {
                            ModelState.AddModelError("Name", $"Current Value: {databaseValues.Name}");
                        }
                        if (databaseValues.Budget != clientValues.Budget)
                        {
                            ModelState.AddModelError("Budget", $"Current Value: {databaseValues.Budget}");
                        }
                        if (databaseValues.StartDate != clientValues.StartDate)
                        {
                            ModelState.AddModelError("StartDate", $"Current Value: {databaseValues.StartDate}");
                        }
                        if (databaseValues.InstructorID != clientValues.InstructorID)
                        {
                            var instructor = await _context.Instructors.AsNoTracking().SingleOrDefaultAsync(i => i.ID == databaseValues.InstructorID);
                            ModelState.AddModelError("InstructorID", $"Current Value: {instructor?.FullName}");
                        }
                        ModelState.AddModelError("", "The record you attempted to edit "
                            + "was modified by another user after you got the original value. The "
                            + "edit operation was canceled and the current values in the database "
                            + "have been displayed. If you still want to edit this record, click "
                            + "the Save button again. Otherwise click the Back to List hyperlink.");

                        departmentToUpdate.xmin = databaseValues.xmin;
                        ModelState.Remove("xmin");
                    }
                }
            }
            PopulateInstructorsDropdownList(departmentToUpdate.InstructorID);
            return View(departmentToUpdate);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return NotFound();
            }
            var departmentToDelete = await _context
                .Departments
                .Include(d => d.Administrator)
                .AsNoTracking()
                .SingleOrDefaultAsync(d => d.DepartmentID == id);

            if (departmentToDelete == null)
            {
                if (concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction(nameof(Index));
                }
                return NotFound();
            }

            if (concurrencyError.GetValueOrDefault())
            {
                ViewData["ConcurrencyErrorMessage"] = "The record you attempted to delete "
                    + "was modified by another user after you got the original values. "
                    + "The delete operation was canceled and the current values in the "
                    + "database have been displayed. If you still want to delete this "
                    + "record, click the Delete button again. Otherwise "
                    + "click the Back to List hyperlink.";
            }

            return View(departmentToDelete);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Department department)
        {
            try
            {
                if (await _context.Departments.AnyAsync(d => d.DepartmentID == department.DepartmentID))
                {
                    _context.Remove(department);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                return RedirectToAction(nameof(Delete), new {
                    id = department.DepartmentID,
                    concurrencyError = true
                });
            }
        }
    }
}