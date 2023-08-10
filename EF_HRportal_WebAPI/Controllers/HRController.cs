using AutoMapper;
using EF_HRportal_WebAPI.CustomActionFilters;
using EF_HRportal_WebAPI.Models.Domain;
using EF_HRportal_WebAPI.Models.DTOs;
using EF_HRportal_WebAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace EF_HRportal_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HRController : ControllerBase
    {
        private readonly IRepository repository;
        private readonly IMapper mapper;

        public HRController(IRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [HttpPost("login")]
        [ValidateModel]
        public async Task<IActionResult> Login(LoginRequestDTO loginDetails)
        {
            var adminDomain = await repository.GetHRbyEmailAsync(loginDetails.Email);
            if (adminDomain == null)
            {
                return NotFound("Invalid Email!!");
            }
            if (adminDomain.Password != loginDetails.Password)
            {
                return BadRequest("Invalid Password!!");
            }
            return Ok("Login Successful");
        }

        [HttpPut("ChangeHRPassword/{HRid}")]
        [ValidateModel]
        public async Task<IActionResult> ChangeHRPassword([FromRoute] int HRid, [FromBody] ChangePasswordRequestDTO newHRcredentialsDTO)
        {
            var HR = await repository.GetHRByIdAsync(HRid);
            if (HR == null)
            {
                return NotFound("HR with the given ID does not exist!! Cannot change the password");
            }
            if (HR.Email != newHRcredentialsDTO.Email)
            {
                return BadRequest("Incorrect Email ID entered for the provided HR ID");
            }
            if (HR.Password != newHRcredentialsDTO.OldPassword)
            {
                return BadRequest("Provide the correct old password!!");
            }
            if (HR.Password == newHRcredentialsDTO.NewPassword)
            {
                return BadRequest("New password cannot be the same as Old password.");
            }
            var updatedAdminDetails = await repository.ChangeHRPasswordAsync(HR, newHRcredentialsDTO);
            var timeLineAction = new Timelinedetail
            {
                EmpId = HR.EmpId,
                Action = "Changed the password of HR Login portal",
                DateOfAction = DateTime.Now
            };
            await repository.AddTimeLineAsync(timeLineAction);
            return Ok("Successfully changed the password of the HR login Portal");
        }

        [HttpPost("CreateEmployee/{HRid}")]
        [ValidateModel]
        public async Task<IActionResult> CreateEmployee([FromRoute] int HRid, [FromBody] CreateEmployeeRequestDTO createEmployeeDTO)
        {
            var HR = await repository.GetHRByIdAsync(HRid);
            if (HR == null)
            {
                return NotFound("HR with given ID does not exist!!");
            }
            var emailUser = await repository.GetEmployeeByEmailAsync(createEmployeeDTO.Email);
            if (emailUser != null)
            {
                return BadRequest("Email has already been taken!! Try using a different Email");
            }
            var Department = await repository.GetDepartmentByIdAsync(createEmployeeDTO.Department);
            if (Department == null)
            {
                return NotFound("The given Department does not exist in the organisation!! Provide a valid Department ID");
            }
            var manager = await repository.GetManagerByIdAsync(createEmployeeDTO.ManagerId);
            if (manager == null)
            {
                return NotFound("Manager with given ID does not exist!! Give a valid Manager ID");
            }
            var employeeDetailsDomain = mapper.Map<Employeedetail>(createEmployeeDTO);
            var newEmployeeDomain = await repository.AddEmployeeAsync(employeeDetailsDomain);
            var timeLineAction = new Timelinedetail
            {
                EmpId = HRid,
                Action = "Created an Employee with ID '" + newEmployeeDomain.Id + "'",
                DateOfAction = DateTime.Now
            };
            await repository.AddTimeLineAsync(timeLineAction);
            var newEmployeeDTO = mapper.Map<EmployeeDetailsDTO>(newEmployeeDomain);
            return CreatedAtAction(nameof(CreateEmployee), newEmployeeDTO);
        }

        [HttpGet("GetAllEmployees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employeesListDomain = await repository.GetAllEmployeesAsync();
            if (employeesListDomain.Count == 0)
            {
                return Ok("There are no employees at present to display");
            }
            var employeesListDTO = mapper.Map<List<EmployeeDetailsDTO>>(employeesListDomain);
            return Ok(employeesListDTO);
        }

        [HttpGet("GetEmployee/{EmpId}")]
        public async Task<IActionResult> GetEmployee([FromRoute] int EmpId)
        {
            var employeeDomain = await repository.GetEmployeeByIdAsync(EmpId);
            if (employeeDomain == null)
            {
                return NotFound("Employee with given ID does not exist!!");
            }
            var employeeDTO = mapper.Map<EmployeeDetailsDTO>(employeeDomain);
            return Ok(employeeDTO);
        }

        [HttpDelete("DeleteEmployee/{HRid}/{EmpId}")]
        public async Task<IActionResult> DeleteEmployee([FromRoute] int HRid, [FromRoute] int EmpId)
        {
            var HR = await repository.GetHRByIdAsync(HRid);
            if (HR == null)
            {
                return NotFound("HR with the given ID does not exist!!");
            }
            var employeeDomain = await repository.GetEmployeeByIdAsync(EmpId);
            if (employeeDomain == null)
            {
                return NotFound("Employee with the given ID does not exist!! Provide the ID of an existing employee to delete from the database.");
            }
            var deletedEmployeeDomain = await repository.DeleteEmployeeAsync(employeeDomain);
            var deletedEmployeeDTO = mapper.Map<EmployeeDetailsDTO>(deletedEmployeeDomain);
            var timeLineAction = new Timelinedetail
            {
                EmpId = HRid,
                Action = "Deleted an Employee with ID '" + deletedEmployeeDomain.Id + "'",
                DateOfAction = DateTime.Now
            };
            await repository.AddTimeLineAsync(timeLineAction);
            return Ok(new { message = "Employee with following details has been deleted successfully", deletedEmployeeDetails = deletedEmployeeDTO });
        }

        [HttpPut("UpdateEmployee/{HRid}/{EmpId}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateEmployee([FromRoute] int HRid, [FromRoute] int EmpId, [FromBody] UpdateEmployeeByHRRequestDTO newDetails)
        {
            var HR = await repository.GetHRByIdAsync(HRid);
            if (HR == null)
            {
                return NotFound("HR with the given ID does not exist!!");
            }
            var EmployeeDomain = await repository.GetEmployeeByIdAsync(EmpId);
            if (EmployeeDomain == null)
            {
                return NotFound("Employee with the Given ID does not exist!! Cannot update the details");
            }
            var Department = await repository.GetDepartmentByIdAsync(newDetails.Department);
            if (Department == null)
            {
                return NotFound("The given Department does not exist in the organisation!! Provide a valid Department ID");
            }
            var Manager = await repository.GetManagerByIdAsync(newDetails.ManagerId);
            if (Manager == null)
            {
                return NotFound("Manager with given new ManagerId does not exist.. Provide a valid ManagerId");
            }
            var updatedEmployeeDomain = await repository.UpdateEmployeeDetailsAsync(EmployeeDomain, newDetails);

            var timeLineAction = new Timelinedetail
            {
                EmpId = HRid,
                Action = "Updated the Details of the Employee with id '" + EmpId + "'",
                DateOfAction = DateTime.Now
            };
            await repository.AddTimeLineAsync(timeLineAction);
            var updatedEmployeeDTO = mapper.Map<EmployeeDetailsDTO>(updatedEmployeeDomain);
            return Ok(new { msg = "Updated the details successfully", updatedEmployeeDTO, });
        }

        [HttpGet("/api/GetEmployeesReportingToManager/{ManagerId}")]
        public async Task<IActionResult> GetEmployeesReportingToManager([FromRoute] int ManagerId)
        {
            var Manager = await repository.GetManagerByIdAsync(ManagerId);
            if (Manager == null)
            {
                return NotFound("Manager with the given ID does not exist. Provide a valid ManagerId");
            }
            var EmployeesListDomain = await repository.GetAllEmployeesUnderManagerAsync(ManagerId);
            if (EmployeesListDomain.Count == 0)
            {
                return Ok("No employees are reporting to the given ManagerId");
            }
            var EmployeeListDTO = mapper.Map<List<EmployeeDetailsDTO>>(EmployeesListDomain);
            return Ok(EmployeeListDTO);
        }

        [HttpPost("AddManager/{HRid}/{EmpId}")]
        public async Task<IActionResult> AddManager([FromRoute] int HRid, [FromRoute] int EmpId)
        {
            var HR = await repository.GetHRByIdAsync(HRid);
            if (HR == null)
            {
                return NotFound("HR with given ID does not exist!!");
            }
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee == null)
            {
                return NotFound("Employee with given ID does not exist..!! Provide a valid employee ID to add as a manager");
            }
            var newManager = await repository.AddManagerAsync(Employee);
            var newManagerDTO = mapper.Map<ManagerDetailsDTO>(newManager);
            var timeLineAction = new Timelinedetail
            {
                EmpId = HRid,
                Action = "Added employee with Id '" + newManager.ManagerId + "' as a Manager",
                DateOfAction = DateTime.Now,
            };
            await repository.AddTimeLineAsync(timeLineAction);
            return Ok(newManagerDTO);
        }
    }
}
