using AutoMapper;
using EF_HRportal_WebAPI.CustomActionFilters;
using EF_HRportal_WebAPI.Models.Domain;
using EF_HRportal_WebAPI.Models.DTOs;
using EF_HRportal_WebAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Identity.Client;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace EF_HRportal_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HRController : ControllerBase
    {
        private readonly IRepository repository;
        private readonly IMapper mapper;
        private readonly IStringLocalizer<HRController> localizer;

        public HRController(IRepository repository, IMapper mapper, IStringLocalizer<HRController> localizer)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.localizer = localizer;
        }


        /*This API authenticates if the user is an Admin/HR
        This API will be consumed if the user selects Admin role in the login form*/
        [HttpPost("login")]
        [ValidateModel]
        public async Task<IActionResult> Login(LoginRequestDto loginDetails)
        {
            var adminDomain = await repository.GetHRbyEmailAsync(loginDetails.Email.ToLower());
            if(adminDomain != null)
            {
                if (adminDomain.Password != loginDetails.Password)
                {
                    return BadRequest(localizer["InvalidPassword"].Value);
                }
                return Ok(localizer["LoginSuccessful"].Value);
            }
            return BadRequest(localizer["InvalidEmail"].Value);
        }

        /*This API changes the login password of the Admin/HR Login portal
        This will store the HR login password changed action in the timeline details table*/
        [HttpPut("ChangeHRPassword/{HRid}")]
        [ValidateModel]
        public async Task<IActionResult> ChangeHRPassword([FromRoute] int HRid, [FromBody] ChangePasswordRequestDto newHRcredentialsDTO)
        {
            var HR = await repository.GetHRByIdAsync(HRid);
            if(HR != null)
            {
                if (HR.Email != newHRcredentialsDTO.Email.ToLower())
                {
                    return BadRequest(localizer["InvalidEmail"].Value);
                }
                if (HR.Password != newHRcredentialsDTO.OldPassword)
                {
                    return BadRequest(localizer["IncorrectOldPassword"].Value);
                }
                if (HR.Password == newHRcredentialsDTO.NewPassword)
                {
                    return BadRequest(localizer["DifferentNewAndOldPasswords"].Value);
                }
                await repository.ChangeHRPasswordAsync(HR, newHRcredentialsDTO);
                var timeLineAction = new Timelinedetail
                {
                    EmpId = HR.EmpId,
                    Action = "Changed the password of HR Login portal",
                    DateOfAction = DateTime.Now
                };
                await repository.AddTimeLineAsync(timeLineAction);
                return Ok(localizer["HRLoginPasswordChangeSuccess", HR.EmpId].Value);
            }
            return BadRequest(localizer["HRDoNotExist",HRid].Value);
        }

        /*This API will create a new Employee and returns EmployeeDetailsDTO of the newly created employee
        This will store the new employee created action in the timeline details table*/
        [HttpPost("CreateEmployee/{HRid}")]
        [ValidateModel]
        public async Task<IActionResult> CreateEmployee([FromRoute] int HRid, [FromBody] CreateEmployeeRequestDto createEmployeeDTO)
        {
            var HR = await repository.GetHRByIdAsync(HRid);
            if(HR != null)
            {
                var emailUser = await repository.GetEmployeeByEmailAsync(createEmployeeDTO.Email);
                if (emailUser != null)
                {
                    return StatusCode(StatusCodes.Status409Conflict, new { Message = localizer["EmailAlreadyInUse"].Value });
                }
                var Department = await repository.GetDepartmentByIdAsync(createEmployeeDTO.Department);
                if (Department == null)
                {
                    return NotFound(localizer["DepartmentDoesNotExist", createEmployeeDTO.Department].Value);
                }
                var manager = await repository.GetManagerByIdAsync(createEmployeeDTO.ManagerId);
                if (manager == null)
                {
                    return NotFound(localizer["ManagerDoesNotExist", createEmployeeDTO.ManagerId].Value);
                }
                var employeeDetailsDomain = mapper.Map<Employeedetail>(createEmployeeDTO);
                employeeDetailsDomain.Email = createEmployeeDTO.Email.ToLower();
                var newEmployeeDomain = await repository.AddEmployeeAsync(employeeDetailsDomain);
                var timeLineAction = new Timelinedetail
                {
                    EmpId = HRid,
                    Action = "Created an Employee with ID '" + newEmployeeDomain.Id + "'",
                    DateOfAction = DateTime.Now
                };
                await repository.AddTimeLineAsync(timeLineAction);
                var newEmployeeDTO = mapper.Map<EmployeeDetailsDto>(newEmployeeDomain);
                return CreatedAtAction(nameof(CreateEmployee), newEmployeeDTO);
            }return BadRequest(localizer["HRDoNotExist",HRid].Value);           
        }

        //This API will return a list of EmployeeDetailDTO's of all the employees present in the database
        [HttpGet("GetAllEmployees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employeesListDomain = await repository.GetAllEmployeesAsync();
            if (employeesListDomain.Count == 0)
            {
                return Ok(localizer["NoEmployees"].Value);
            }
            var employeesListDTO = mapper.Map<List<EmployeeDetailsDto>>(employeesListDomain);
            return Ok(employeesListDTO);
        }

        /*This API will return a EmployeeDetailsDTO
        It requires an Employee ID to be passed in the route*/
        [HttpGet("GetEmployee/{EmpId}")]
        public async Task<IActionResult> GetEmployee([FromRoute] int EmpId)
        {
            var employeeDomain = await repository.GetEmployeeByIdAsync(EmpId);
            if (employeeDomain != null) 
            {
                var employeeDTO = mapper.Map<EmployeeDetailsDto>(employeeDomain);
                return Ok(employeeDTO);
            }
            return NotFound(localizer["EmployeeDoesNotExist", EmpId].Value);
        }

        /*This API will delete the Employee from the database.
        By triggering this API all the data of the deleted Employee will be completely erased.
        Employee's attendance,timeline will also be deleted
        This API requires the ID of the HR/Admin who is performing the delete operation and the ID of the Employee who needs to be deleted
        This will store the employee deleted action in the timeline details table*/
        [HttpDelete("DeleteEmployee/{HRid}/{EmpId}")]
        public async Task<IActionResult> DeleteEmployee([FromRoute] int HRid, [FromRoute] int EmpId)
        {
            var HR = await repository.GetHRByIdAsync(HRid);
            if (HR != null)
            {
                var employeeDomain = await repository.GetEmployeeByIdAsync(EmpId);
                if (employeeDomain != null)
                {
                    var deletedEmployeeDomain = await repository.DeleteEmployeeAsync(employeeDomain);
                    var deletedEmployeeDTO = mapper.Map<EmployeeDetailsDto>(deletedEmployeeDomain);
                    var timeLineAction = new Timelinedetail
                    {
                        EmpId = HRid,
                        Action = "Deleted an Employee with ID '" + deletedEmployeeDomain.Id + "'",
                        DateOfAction = DateTime.Now
                    };
                    await repository.AddTimeLineAsync(timeLineAction);
                    return Ok(new { Message = localizer["EmployeeDeletionSuccess"].Value, deletedEmployeeDetails = deletedEmployeeDTO });
                }
                return BadRequest(localizer["EmployeeDoesNotExist", EmpId].Value);
            }
            return BadRequest(localizer["HRDoNotExist",HRid].Value);
        }

        /*This API is used to update the details of the employee
        It requires the ID of HR/Admin who is updating the details, ID of the employee whose details are being updated
        This API also requires a body of UpdateEmployeeByHRRequestDTO
        This will store the updated employee details action in the timeline details table*/
        [HttpPut("UpdateEmployee/{HRid}/{EmpId}")]
        [ValidateModel]
        public async Task<IActionResult> UpdateEmployee([FromRoute] int HRid, [FromRoute] int EmpId, [FromBody] UpdateEmployeeByHRRequestDto newDetails)
        {
            var HR = await repository.GetHRByIdAsync(HRid);
            if (HR != null )
            {
                var EmployeeDomain = await repository.GetEmployeeByIdAsync(EmpId);
                if (EmployeeDomain != null)
                {
                    var Department = await repository.GetDepartmentByIdAsync(newDetails.Department);
                    if (Department != null)
                    {
                        var Manager = await repository.GetManagerByIdAsync(newDetails.ManagerId);
                        if (Manager != null)
                        {
                            var updatedEmployeeDomain = await repository.UpdateEmployeeDetailsAsync(EmployeeDomain, newDetails);

                            var timeLineAction = new Timelinedetail
                            {
                                EmpId = HRid,
                                Action = "Updated the Details of the Employee with id '" + EmpId + "'",
                                DateOfAction = DateTime.Now
                            };
                            await repository.AddTimeLineAsync(timeLineAction);
                            var updatedEmployeeDTO = mapper.Map<EmployeeDetailsDto>(updatedEmployeeDomain);
                            return Ok(new { msg = localizer["EmployeeDetailsUpdationSuccess", updatedEmployeeDTO.Id].Value, updatedEmployeeDTO });
                        }
                        return NotFound(localizer["ManagerDoesNotExist", newDetails.ManagerId].Value);
                    }
                    return NotFound(localizer["DepartmentDoesNotExist", newDetails.Department].Value);
                }
                return BadRequest(localizer["EmployeeDoesNotExist", EmpId].Value);
            }
            return BadRequest(localizer["HRDoNotExist", HRid].Value);
        }

        /*This API will return the list of EmployeeDetailsDTO's of the employees reporting to the 
        provided ManagerId in the route*/
        [HttpGet("/api/GetEmployeesReportingToManager/{ManagerId}")]
        public async Task<IActionResult> GetEmployeesReportingToManager([FromRoute] int ManagerId)
        {
            var Manager = await repository.GetManagerByIdAsync(ManagerId);
            if (Manager != null)
            {
                var EmployeesListDomain = await repository.GetAllEmployeesUnderManagerAsync(ManagerId);
                if (EmployeesListDomain.Count == 0)
                {
                    return Ok(localizer["NoEmployees"].Value);
                }
                var EmployeeListDTO = mapper.Map<List<EmployeeDetailsDto>>(EmployeesListDomain);
                return Ok(EmployeeListDTO);
            }
            return BadRequest(localizer["ManagerDoesNotExist",ManagerId].Value);
        }

        /*This API will Add an employee as a manager
        This API requires the ID of the HR/Admin who is adding the Manager and also the employee ID of the
        employee who is being added as manager
        This will store the manager added action in the timeline details table*/
        [HttpPost("AddManager/{HRid}/{EmpId}")]
        public async Task<IActionResult> AddManager([FromRoute] int HRid, [FromRoute] int EmpId)
        {
            var HR = await repository.GetHRByIdAsync(HRid);
            if (HR != null)
            {
                var Employee = await repository.GetEmployeeByIdAsync(EmpId);
                if (Employee != null)
                {
                    var newManager = await repository.AddManagerAsync(Employee);
                    var newManagerDTO = mapper.Map<ManagerDetailsDto>(newManager);
                    var timeLineAction = new Timelinedetail
                    {
                        EmpId = HRid,
                        Action = "Added employee with Id '" + newManager.ManagerId + "' as a Manager",
                        DateOfAction = DateTime.Now,
                    };
                    await repository.AddTimeLineAsync(timeLineAction);
                    return StatusCode(StatusCodes.Status201Created, newManagerDTO);
                }
                return BadRequest(localizer["EmployeeDoesNotExist", EmpId].Value);
            }
            return BadRequest(localizer["HRDoNotExist",HRid].Value);
        }
    }
}
