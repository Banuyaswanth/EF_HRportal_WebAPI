using System;
using System.Collections.Generic;

namespace EF_HRportal_WebAPI.Models.Domain;

public partial class Departmentdetail
{
    public string DepartmentId { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;

    public virtual ICollection<Employeedetail> Employeedetails { get; set; } = new List<Employeedetail>();

    public virtual ICollection<Managerdetail> Managerdetails { get; set; } = new List<Managerdetail>();
}
