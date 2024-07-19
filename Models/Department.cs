using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Department
    {
            public int Id { get; set; }
            public string Name { get; set; }
            [NotMapped]
            public IFormFile? LogoFile { get; set; } // For file upload

            public string? Logo { get; set; }
            public int? ParentDepartmentId { get; set; }
            public Department? ParentDepartment { get; set; }
            public ICollection<Department>? SubDepartments { get; set; }
        }
    }

