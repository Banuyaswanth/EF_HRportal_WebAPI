using AutoMapper;
using EF_HRportal_WebAPI.Models.Domain;
using EF_HRportal_WebAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Runtime.InteropServices;

namespace EF_HRportal_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IRepository repository;
        private readonly IMapper mapper;

        public AttendanceController(IRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [HttpPost("TimeIn/{EmpId}")]
        public async Task<IActionResult> TimeIn([FromRoute] int EmpId)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee == null)
            {
                return NotFound("Employee with the given does not exist..!!");
            }
            var lastTimeInDetails = await repository.EmployeeTimeInAsync(EmpId);
            if (lastTimeInDetails == null)
            {
                return BadRequest("Unable to TimeIn. Please try again");
            }
            return Ok(new { lastTimeInId = lastTimeInDetails.Id, Message = "Last TimeIn record is stored with the primary key as '" + lastTimeInDetails.Id + "'" });
        }

        [HttpPut("TimeOut/{EmpId}/{LastTimeInID}")]
        public async Task<IActionResult> TimeOut([FromRoute] int EmpId, [FromRoute] int LastTimeInID)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee == null)
            {
                return NotFound("Employee with the given ID does not exist");
            }
            var attendanceRecord = await repository.GetAttendanceRecordAsync(LastTimeInID);
            if (attendanceRecord == null)
            {
                return NotFound("Attendance record with the given LastTimeInID does not exist..!!");
            }
            if (attendanceRecord.EmpId != EmpId)
            {
                return BadRequest("Attendance record does not belong to the given Employee ID");
            }
            var updatedAttendanceRecord = await repository.EmployeeTimeOutAsync(attendanceRecord);
            return Ok(updatedAttendanceRecord);
        }

        [HttpGet("GetAttendance/{EmpId}")]
        public async Task<IActionResult> GetAttendance([FromRoute] int EmpId)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee == null)
            {
                return NotFound("Employee with the given ID does not exist!!");
            }
            var attendance = await repository.GetAttendanceOfEmployeeAsync(EmpId);
            if (attendance.Count == 0)
            {
                return Ok("No attendance records to display for the employee");
            }
            return Ok(attendance);
        }
    }
}
