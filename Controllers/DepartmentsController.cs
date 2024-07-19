using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DepartmentsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments.ToListAsync();
            return View(departments);
        }

        #region Create Department
        // GET: Departments/Create
        public IActionResult Create()
        {
            ViewData["ParentDepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,LogoFile,ParentDepartmentId")] Department department)
        {
            if (ModelState.IsValid)
            {
                if (department.LogoFile != null)
                {

                    // Get the path to the wwwroot folder
                    string wwwRootPath = _webHostEnvironment.WebRootPath;

                    // Combine the path to get the full path to the images folder
                    string uploadsFolder = Path.Combine(wwwRootPath, "images");

                    // Ensure the directory exists
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Get the file name and combine it with the uploads folder path
                    string fileName = Path.GetFileName(department.LogoFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await department.LogoFile.CopyToAsync(stream);
                    }

                    // Set the file path to the department's Logo property
                    department.Logo = "/images/" + fileName;
                }

                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ParentDepartmentId = new SelectList(_context.Departments, "Id", "Name", department.ParentDepartmentId);
            return View(department);
        }
        //Get all parents for department
        #endregion
       
        #region View Department Hierarchy on Detail button press
        private List<Department> GetParentDepartments(Department department)
        {
            var parents = new List<Department>();
            var currentDepartment = department;

            while (currentDepartment.ParentDepartmentId != null)
            {
                currentDepartment = _context.Departments
                    .Include(d => d.ParentDepartment)
                    .FirstOrDefault(d => d.Id == currentDepartment.ParentDepartmentId);
                if (currentDepartment != null)
                {
                    parents.Add(currentDepartment);
                }
            }

            return parents;
        }
        /*
         * Implement functionality to select a department/sub-department and display:
            A list of all sub-departments within the selected department/sub-department.
            A list of all parent departments up to the top-level for the selected department/sub-department.

         */
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .Include(d => d.SubDepartments)
                .Include(d => d.ParentDepartment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            var parentDepartments = GetParentDepartments(department);
            ViewBag.ParentDepartments = parentDepartments.Select(d => new { d.Id, d.Name }).ToList();

            return View(department);
        }
        #endregion
       
        #region Edit Department
        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            ViewData["ParentDepartmentId"] = new SelectList(_context.Departments, "Id", "Name", department.ParentDepartmentId);
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Logo,ParentDepartmentId")] Department department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id))
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
            ViewData["ParentDepartmentId"] = new SelectList(_context.Departments, "Id", "Name", department.ParentDepartmentId);
            return View(department);
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
        #endregion
        #region Delete Department
        // GET: Departments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}