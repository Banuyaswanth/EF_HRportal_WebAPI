using System;
using System.Collections.Generic;

namespace EF_HRportal_WebAPI.Models.Domain;

public partial class Timelinedetail
{
    public int EmpId { get; set; }

    public string Action { get; set; } = null!;

    public DateTime DateOfAction { get; set; }

    public int Id { get; set; }

    public virtual Employeedetail Emp { get; set; } = null!;
}
