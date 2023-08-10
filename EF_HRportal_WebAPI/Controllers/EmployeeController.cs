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
    public class EmployeeController : ControllerBase
    {
        private readonly IRepository repository;
        private readonly IMapper mapper;

        public EmployeeController(IRepository Repository, IMapper Mapper)
        {
            repository = Repository;
            mapper = Mapper;
        }

        [HttpPost("login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginDetails)
        {
            var Employee = await repository.GetEmployeeByEmailAsync(loginDetails.Email);
            if (Employee == null)
            {
                return NotFound("Invalid Username!!");
            }
            if (Employee.Password != loginDetails.Password)
            {
                return BadRequest("Invalid Password!!");
            }
            return Ok("Login Successful");
        }

        [HttpGet("/api/SearchEmployee/{name}")]
        public async Task<IActionResult> SearchEmployee([FromRoute] string name)
        {
            var employeesDomainList = await repository.SearchEmployeeByNameAsync(name);
            if (employeesDomainList.Count == 0)
            {
                return NotFound("No employee can be found with the given name");
            }
            var employeesDTOList = mapper.Map<List<EmployeeDetailsDTO>>(employeesDomainList);
            return Ok(employeesDTOList);
        }

        [HttpGet("/api/GetPersonalDetails/{EmpId}")]
        public async Task<IActionResult> GetPersonalDetails([FromRoute] int EmpId)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee == null)
            {
                return NotFound("Cannot fetch the details of an employee who does not exist..!! Provide a valid Employee ID to fetch the Personal Details");
            }
            var PersonalDetails = mapper.Map<PersonalDetailsDTO>(Employee);
            return Ok(PersonalDetails);
        }

        [HttpPut("UpdatePersonalDetails/{EmpId}")]
        [ValidateModel]
        public async Task<IActionResult> UpdatePersonalDetails([FromRoute] int EmpId, [FromBody] UpdatePersonalDetailsDTO newDetails)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee == null)
            {
                return NotFound("Cannot update the details of a non existent Employee..!");
            }
            var updatedEmployeeDomain = await repository.UpdatePersonalDetailsAsync(Employee, newDetails);
            var updatedEmployeeDTO = mapper.Map<PersonalDetailsDTO>(updatedEmployeeDomain);
            var timeLineAction = new Timelinedetail
            {
                EmpId = EmpId,
                Action = "Updated personal details",
                DateOfAction = DateTime.Now
            };
            await repository.AddTimeLineAsync(timeLineAction);
            return Ok(updatedEmployeeDTO);
        }

        [HttpPut("ChangePassword/{EmpId}")]
        public async Task<IActionResult> ChangeLoginPassword([FromRoute] int EmpId, [FromBody] ChangePasswordRequestDTO newCredentials)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee == null)
            {
                return NotFound("Employee with the given ID does not exist..!! Cannot change the password");
            }
            if (Employee.Email != newCredentials.Email)
            {
                return BadRequest("Incorrect Email entered!!");
            }
            if (Employee.Password != newCredentials.OldPassword)
            {
                return BadRequest("Provide the correct old password!!");
            }
            if (Employee.Password == newCredentials.NewPassword)
            {
                return BadRequest("New password cannot be the same as the old password");
            }
            var updatedEmployeeDetails = await repository.ChangeEmployeePasswordAsync(Employee, newCredentials);
            var timeLineAction = new Timelinedetail
            {
                EmpId = EmpId,
                Action = "Changed the Password",
                DateOfAction = DateTime.Now
            };
            await repository.AddTimeLineAsync(timeLineAction);
            return Ok("Successfully changed the password of Employee login");
        }
    }
}
