﻿using AutoMapper;
using EF_HRportal_WebAPI.Models.Domain;
using EF_HRportal_WebAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Identity.Client;
using System.Runtime.InteropServices;

namespace EF_HRportal_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IRepository repository;
        private readonly IStringLocalizer<AttendanceController> localizer;

        public AttendanceController(IRepository repository, IStringLocalizer<AttendanceController> localizer)
        {
            this.repository = repository;
            this.localizer = localizer;
        }

        /*This API will create a new attendance record for the Employee whose ID is provided in the route
        This API will add the Time In for the created attendance record
        It will return the ID of the record generated which should be stored in the Cookies of the browser using frontend*/
        [HttpPost("TimeIn/{EmpId}")]
        public async Task<IActionResult> TimeIn([FromRoute] int EmpId)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee != null)
            {
                var oldTimeInDetails = await repository.GetAttendanceRecordAsync(EmpId);
                if (oldTimeInDetails != null)
                {
                    return BadRequest(localizer["MultipleTimeIn", EmpId].Value);
                }
                var newTimeInDetails = await repository.EmployeeTimeInAsync(EmpId);
                if (newTimeInDetails != null)
                {
                    return Ok(new { lastTimeInId = newTimeInDetails.Id, Message = localizer["TimeInMsg", newTimeInDetails.Id].Value });
                }
                return BadRequest(localizer["TimeInFailure"].Value);
            }
            return BadRequest(localizer["EmployeeDoesNotExist",EmpId].Value);
        }

        /*This API will accept the Employee ID and the LastTimeInID that is returned by the TimeIN API
        This API will update the TimeOut record and calculate the duration for the attendance record*/
        [HttpPut("TimeOut/{EmpId}")]
        public async Task<IActionResult> TimeOut([FromRoute] int EmpId)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee != null)
            {
                var attendanceRecord = await repository.GetAttendanceRecordAsync(EmpId);
                if (attendanceRecord != null)
                {
                    var updatedAttendanceRecord = await repository.EmployeeTimeOutAsync(attendanceRecord);
                    return Ok(updatedAttendanceRecord);
                }
                return BadRequest(localizer["TimeOutRecordNotFound", EmpId].Value);
            }
            return BadRequest(localizer["EmployeeDoesNotExist",EmpId].Value);
        }

        //This API will return the summary of the attendance of the Employee whose ID is provided in the Route
        [HttpGet("GetAttendance/{EmpId}")]
        public async Task<IActionResult> GetAttendance([FromRoute] int EmpId)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee != null)
            {
                var attendance = await repository.GetAttendanceOfEmployeeAsync(EmpId);
                if (attendance.Count == 0)
                {
                    return Ok(localizer["NoAttendanceRecords", EmpId].Value);
                }
                return Ok(attendance);
            }
            return BadRequest(localizer["EmployeeDoesNotExist",EmpId].Value);
        }
    }
}
