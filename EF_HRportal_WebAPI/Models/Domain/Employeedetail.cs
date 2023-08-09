using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EF_HRportal_WebAPI.Models.Domain;

public partial class Employeedetail
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public int? Salary { get; set; }

    public string? Department { get; set; }

    public DateTime DateOfJoining { get; set; }

    public int ManagerId { get; set; }

    [JsonIgnore]
    public virtual ICollection<AttendanceDetail> AttendanceDetails { get; set; } = new List<AttendanceDetail>();

    [JsonIgnore]
    public virtual Departmentdetail? DepartmentNavigation { get; set; }

    [JsonIgnore]
    public virtual ICollection<Employeedetail> InverseManager { get; set; } = new List<Employeedetail>();
    
    [JsonIgnore]
    public virtual Employeedetail Manager { get; set; } = null!;

    [JsonIgnore]
    public virtual Managerdetail? Managerdetail { get; set; }

    [JsonIgnore]
    public virtual ICollection<Timelinedetail> Timelinedetails { get; set; } = new List<Timelinedetail>();
}
