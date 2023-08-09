using System;
using System.Collections.Generic;

namespace EF_HRportal_WebAPI.Models.Domain;

public partial class Managerdetail
{
    public int ManagerId { get; set; }

    public string Department { get; set; } = null!;

    public virtual Departmentdetail DepartmentNavigation { get; set; } = null!;

    public virtual Employeedetail Manager { get; set; } = null!;
}
