using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EF_HRportal_WebAPI.Models.Domain;

public partial class Admindetail
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int EmpId { get; set; }

    public virtual Employeedetail EmailNavigation { get; set; } = null!;

    public virtual Employeedetail Emp { get; set; } = null!;
}
