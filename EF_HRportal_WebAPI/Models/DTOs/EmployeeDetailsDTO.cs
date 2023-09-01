using EF_HRportal_WebAPI.Models.Domain;

namespace EF_HRportal_WebAPI.Models.DTOs
{
    public class EmployeeDetailsDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Phone { get; set; }

        public string? Department { get; set; }

        public DateTime DateOfJoining { get; set; }

        public int ManagerId { get; set; }

        public string ManagerName { get; set; } = null!;
        public string ManagerEmail { get; set; } = null!;
    }
}
