using Xunit;
using AutoFixture;
using Moq;
using FluentAssertions;
using EF_HRportal_WebAPI.Repository;
using Microsoft.Extensions.Localization;
using EF_HRportal_WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using EF_HRportal_WebAPI.Models.Domain;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using EF_HRportal_WebAPI.Models.DTOs;

namespace EF_HRportal_WebAPI.UnitTests.Controllers
{
    public class AttendanceControllerTests
    {
        private readonly IFixture fixture;
        private readonly Mock<IRepository> serviceMock;
        private readonly Mock<IStringLocalizer<AttendanceController>> localizerMock;
        private readonly AttendanceController sut;

        public AttendanceControllerTests()
        {
            fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));
            serviceMock = fixture.Freeze<Mock<IRepository>>();
            localizerMock = new Mock<IStringLocalizer<AttendanceController>>();
            sut = new AttendanceController(serviceMock.Object,localizerMock.Object);
        }

        //TimeIn Action Method Unit Tests
        [Fact]
        public async Task TimeIn_ShouldReturnOkResponse_WhenEmployeeWithNoNullTimeOutDetailsFound()
        {
            //Arrange
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            AttendanceDetail? oldTimeInDetails = null;
            AttendanceDetail newTimeInDetails = fixture.Create<AttendanceDetail>();
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.GetAttendanceRecordAsync(empId)).ReturnsAsync(oldTimeInDetails);
            serviceMock.Setup(x => x.EmployeeTimeInAsync(empId)).ReturnsAsync(newTimeInDetails);

            localizerMock.Setup(loc => loc["TimeInMsg", newTimeInDetails.Id]).Returns(new LocalizedString("TimeInMsg", $"Last TimeIn record is stored with the primary key as '{newTimeInDetails.Id}'"));

            var timeInSuccessResult = new { lastTimeInId = newTimeInDetails.Id, Message = $"Last TimeIn record is stored with the primary key as '{newTimeInDetails.Id}'" };
            //Act
            var result = await sut.TimeIn(empId).ConfigureAwait(false);
            var okResult = (OkObjectResult)result;
            var response = (dynamic)okResult.Value;

            //Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal(timeInSuccessResult.ToString(), response.ToString());
        }

        [Fact]
        public async Task TimeIn_ShouldReturnBadRequest_WhenEmployeeWithOldTimeInDetialsWithNullTimeOutFound()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            AttendanceDetail oldTimeInDetails = fixture.Create<AttendanceDetail>();
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.GetAttendanceRecordAsync(empId)).ReturnsAsync(oldTimeInDetails);

            localizerMock.Setup(loc => loc["MultipleTimeIn", empId]).Returns(new LocalizedString("MultipleTimeIn", $"TimeIN record for the Employee with ID = {empId} exists which has not been timed out. TimeOut first before Timing In again"));

            var result = await sut.TimeIn(empId).ConfigureAwait(false);
            var badRequestResult = (BadRequestObjectResult)result;

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal($"TimeIN record for the Employee with ID = {empId} exists which has not been timed out. TimeOut first before Timing In again", badRequestResult.Value);
        }

        [Fact]
        public async Task TimeIn_ShouldReturnBadRequest_WhenTimeInFailedDueToSomeError()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            AttendanceDetail? oldAttendanceDetail = null;
            AttendanceDetail? newAttendanceDetail = null;
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.GetAttendanceRecordAsync(empId)).ReturnsAsync(oldAttendanceDetail);
            serviceMock.Setup(x => x.EmployeeTimeInAsync(empId)).ReturnsAsync(newAttendanceDetail);

            localizerMock.Setup(loc => loc["TimeInFailure"]).Returns(new LocalizedString("TimeInFailure", "Unable to TimeIn. Please try again"));

            var result = await sut.TimeIn(empId).ConfigureAwait(false);
            var badRequestResult = (BadRequestObjectResult)result;

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal("Unable to TimeIn. Please try again", badRequestResult.Value);
        }

        [Fact]
        public async Task TimeIn_ShouldReturnBadRequest_WhenEmployeeWithGivenIdDoesNotExist()
        {
            var empId = fixture.Create<int>();
            Employeedetail? employeeDetails = null;
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);

            localizerMock.Setup(loc => loc["EmployeeDoesNotExist",empId]).Returns(new LocalizedString("EmployeeDoesNotExist", $"Employee with given ID = {empId} does not exist!!"));

            var result = await sut.TimeIn(empId).ConfigureAwait(false);
            var badRequestResult = (BadRequestObjectResult)result;

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal($"Employee with given ID = {empId} does not exist!!", badRequestResult.Value);
        }


        //TimeOut Action Method Unit Tests
        [Fact]
        public async Task TimeOut_ShouldReturnOkResponse_WhenEmployeeAttendanceRecordFoundWithTimeOutAsNull()
        {
            var empId = fixture.Create<int>();
            var employee = fixture.Create<Employeedetail>();
            var attendanceRecord = fixture.Create<AttendanceDetail>();
            var updatedAttendanceRecord = fixture.Create<AttendanceDetail>();
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employee);
            serviceMock.Setup(x => x.GetAttendanceRecordAsync(empId)).ReturnsAsync(attendanceRecord);
            serviceMock.Setup(x => x.EmployeeTimeOutAsync(attendanceRecord)).ReturnsAsync(updatedAttendanceRecord);

            var result = await sut.TimeOut(empId).ConfigureAwait(false);
            
            result.Should().NotBeNull();
            Assert.True(result is OkObjectResult);
        }

        [Fact]
        public async Task TimeOut_ShouldReturnBadRequest_WhenEmployeeAttendanceRecordNotFoundWithTimeOutAsNull()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            AttendanceDetail? attendanceDetail = null;
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.GetAttendanceRecordAsync(empId)).ReturnsAsync(attendanceDetail);

            localizerMock.Setup(loc => loc["TimeOutRecordNotFound", empId]).Returns(new LocalizedString("TimeOutRecordNotFound", $"Attendance record to TimeOut for the given Employee with ID = {empId} does not exist..!!"));

            var result = await sut.TimeOut(empId).ConfigureAwait(false);
            var badRequestResult = (BadRequestObjectResult)result;

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal($"Attendance record to TimeOut for the given Employee with ID = {empId} does not exist..!!", badRequestResult.Value);
        }

        [Fact]
        public async Task TimeOut_ShouldReturnBadRequest_WhenEmployeeNotFound()
        {
            var empId = fixture.Create<int>();
            Employeedetail? employeedetail = null;
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeedetail);

            localizerMock.Setup(loc => loc["EmployeeDoesNotExist", empId]).Returns(new LocalizedString("EmployeeDoesNotExist", $"Employee with given ID = {empId} does not exist!!"));

            var result = await sut.TimeOut(empId).ConfigureAwait (false);
            var badRequestResult = (BadRequestObjectResult)result;

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal($"Employee with given ID = {empId} does not exist!!", badRequestResult.Value);
        }

        //GetAttendance Action Method Unit Tests
        [Fact]
        public async Task GetAttendance_ShouldReturnOkResponse_WhenEmployeeWithAttendanceRecordsFound()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            var attendance = fixture.Create<List<AttendanceSummaryDto>>();
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.GetAttendanceOfEmployeeAsync(empId)).ReturnsAsync(attendance);

            var result = await sut.GetAttendance(empId).ConfigureAwait(false);
            var okObjectResult = (OkObjectResult)result;

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            Assert.IsAssignableFrom<List<AttendanceSummaryDto>>(okObjectResult.Value);
            var returnedAttendance = (List<AttendanceSummaryDto>?) okObjectResult.Value;

            Assert.Equal(attendance, returnedAttendance);
        }

        [Fact]
        public async Task GetAttendance_ShouldReturnOkResponse_WhenEmployeeFoundButWithZeroAttendanceRecords()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            List<AttendanceSummaryDto> attendance = new List<AttendanceSummaryDto> { };
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.GetAttendanceOfEmployeeAsync(empId)).ReturnsAsync(attendance);

            localizerMock.Setup(loc => loc["NoAttendanceRecords", empId]).Returns(new LocalizedString("NoAttendanceRecords", $"No attendance records to display for the employee with ID = {empId}"));

            var result = await sut.GetAttendance(empId).ConfigureAwait(false);
            var okObjectResult = (OkObjectResult)result;

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal($"No attendance records to display for the employee with ID = {empId}",okObjectResult.Value);
        }

        [Fact]
        public async Task GetAttendance_ShouldReturnBadRequest_WhenEmployeeWithGivenIdNotFound()
        {
            var empId = fixture.Create<int>();
            Employeedetail? employeedetail = null;
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeedetail);

            localizerMock.Setup(loc => loc["EmployeeDoesNotExist", empId]).Returns(new LocalizedString("EmployeeDoesNotExist", $"Employee with given ID = {empId} does not exist!!"));

            var result = await sut.GetAttendance(empId).ConfigureAwait(false);
            var badRequestResult = (BadRequestObjectResult)result;

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal($"Employee with given ID = {empId} does not exist!!", badRequestResult.Value);
        }
    }
}