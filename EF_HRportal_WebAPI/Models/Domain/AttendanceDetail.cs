using System;
using System.Collections.Generic;

namespace EF_HRportal_WebAPI.Models.Domain;

public partial class AttendanceDetail
{
    public int Id { get; set; }

    public int EmpId { get; set; }

    public DateTime DateOfAttendance { get; set; }

    public DateTime? TimeIn { get; set; }

    public DateTime? TimeOut { get; set; }

    public int? Duration { get; set; }

    public virtual Employeedetail Emp { get; set; } = null!;
}
