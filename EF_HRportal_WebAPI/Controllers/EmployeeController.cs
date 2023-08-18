using AutoMapper;
using EF_HRportal_WebAPI.CustomActionFilters;
using EF_HRportal_WebAPI.Models.Domain;
using EF_HRportal_WebAPI.Models.DTOs;
using EF_HRportal_WebAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Identity.Client;

namespace EF_HRportal_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IRepository repository;
        private readonly IMapper mapper;
        private readonly IStringLocalizer<EmployeeController> localizer;

        public EmployeeController(IRepository Repository, IMapper Mapper, IStringLocalizer<EmployeeController> localizer)
        {
            repository = Repository;
            mapper = Mapper;
            this.localizer = localizer;
        }

        //This API authenticates if the user is an Employee
        //This API will be consumed if the user does not select Admin role in the login form
        [HttpPost("login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginDetails)
        {
            var Employee = await repository.GetEmployeeByEmailAsync(loginDetails.Email.ToLower());
            if (Employee == null)
            {
                return BadRequest(localizer["InvalidEmail"].Value);
            }
            if (Employee.Password != loginDetails.Password)
            {
                return BadRequest(localizer["InvalidPassword"].Value);
            }
            return Ok(localizer["LoginSuccessful"].Value);
        }

        //This API will search for an employee based on the name provided in the Query parameter
        //If no name is provided this will return a list of EmployeeDetailsDTO's of all the employees in the database
        [HttpGet("/api/SearchEmployee")]
        public async Task<IActionResult> SearchEmployee([FromQuery] string? Name)
        {
            var employeesDomainList = await repository.SearchEmployeeByNameAsync(Name ?? "");
            if (employeesDomainList.Count == 0)
            {
                return Ok(localizer["EmployeeNameSearchFail",Name].Value);
            }
            var employeesDTOList = mapper.Map<List<EmployeeDetailsDTO>>(employeesDomainList);
            return Ok(employeesDTOList);
        }

        //This API will return the PersonalDetailsDTO of the employee
        //This requires the Employee ID to be provided in the route
        [HttpGet("/api/GetPersonalDetails/{EmpId}")]
        public async Task<IActionResult> GetPersonalDetails([FromRoute] int EmpId)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee == null)
            {
                return Ok(localizer["EmployeeDoesNotExist", EmpId].Value);
            }
            var PersonalDetails = mapper.Map<PersonalDetailsDTO>(Employee);
            return Ok(PersonalDetails);
        }

        //This API will update the personal details(only name or phone) of the employee
        //This API will take the UpdatePersonalDetailsDTO as body and EmpId in the route who is updating his personal details
        //This will store the update action in the timeline details table
        [HttpPut("UpdatePersonalDetails/{EmpId}")]
        [ValidateModel]
        public async Task<IActionResult> UpdatePersonalDetails([FromRoute] int EmpId, [FromBody] UpdatePersonalDetailsDTO newDetails)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee == null)
            {
                return Ok(localizer["EmployeeDoesNotExist",EmpId].Value);
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
            return Ok(new {Message = localizer["PersonalDetailsUpdationSuccess",EmpId].Value, UpdatedDetails = updatedEmployeeDTO});
        }

        //This API will change the login password of the Employee login
        //This will accept ChangePasswordRequestDTO in the body and EmpId in the route
        //This will store the Employee login password changed action in the timeline details table
        [HttpPut("ChangePassword/{EmpId}")]
        public async Task<IActionResult> ChangeLoginPassword([FromRoute] int EmpId, [FromBody] ChangePasswordRequestDTO newCredentials)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee == null)
            {
                return Ok(localizer["EmployeeDoesNotExist",EmpId].Value);
            }
            if (Employee.Email != newCredentials.Email.ToLower())
            {
                return Ok(localizer["InvalidEmail"].Value);
            }
            if (Employee.Password != newCredentials.OldPassword)
            {
                return Ok(localizer["IncorrectOldPassword"].Value);
            }
            if (Employee.Password == newCredentials.NewPassword)
            {
                return Ok(localizer["DifferentNewAndOldPasswords"].Value);
            }
            var updatedEmployeeDetails = await repository.ChangeEmployeePasswordAsync(Employee, newCredentials);
            var timeLineAction = new Timelinedetail
            {
                EmpId = EmpId,
                Action = "Changed the Password",
                DateOfAction = DateTime.Now
            };
            await repository.AddTimeLineAsync(timeLineAction);
            return Ok(localizer["EmployeeLoginPasswordChangeSuccess",EmpId].Value);
        }

        //[HttpGet("GetSalary/{EmpId}")]
        //public async Task<IActionResult> GetSalary([FromRoute] int EmpId)
        //{

        //}
    }
}
