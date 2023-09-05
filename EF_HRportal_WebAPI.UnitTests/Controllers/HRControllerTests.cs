using AutoFixture;
using AutoMapper;
using EF_HRportal_WebAPI.Controllers;
using EF_HRportal_WebAPI.Models.Domain;
using EF_HRportal_WebAPI.Models.DTOs;
using EF_HRportal_WebAPI.Repository;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EF_HRportal_WebAPI.UnitTests.Controllers
{
    public class HRControllerTests
    {
        private readonly IFixture fixture;
        private readonly Mock<IRepository> serviceMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<IStringLocalizer<HRController>> localizerMock;
        private readonly HRController sut;

        public HRControllerTests()
        {
            fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));
            serviceMock = fixture.Freeze<Mock<IRepository>>();
            mapperMock = fixture.Freeze<Mock<IMapper>>();
            localizerMock = new Mock<IStringLocalizer<HRController>>();
            sut = new HRController(serviceMock.Object, mapperMock.Object, localizerMock.Object);
        }

        //Login Action Method Unit Test cases
        [Fact]
        public async Task Login_ShouldReturnOkResponse_WhenValidLoginDetailsAreProvided()
        {
            var loginDetails = fixture.Create<LoginRequestDto>();
            var hrDetails = fixture.Create<Admindetail>();
            loginDetails.Password = hrDetails.Password;
            serviceMock.Setup(x => x.GetHRbyEmailAsync(loginDetails.Email.ToLower())).ReturnsAsync(hrDetails);
            localizerMock.Setup(loc => loc["LoginSuccessful"]).Returns(new LocalizedString("LoginSuccessful", "Login Successful"));

            var result = await sut.Login(loginDetails).ConfigureAwait(false);

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            var okObjectResult = (OkObjectResult)result;
            Assert.Equal("Login Successful", okObjectResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenInvalidEmailIsProvided()
        {
            var loginDetails = fixture.Create<LoginRequestDto>();
            Admindetail? hrDetails = null;
            serviceMock.Setup(x => x.GetHRbyEmailAsync(loginDetails.Email.ToLower())).ReturnsAsync(hrDetails);
            localizerMock.Setup(loc => loc["InvalidEmail"]).Returns(new LocalizedString("InvalidEmail", "Invalid Email Entered..!!"));

            var result = await sut.Login(loginDetails).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal("Invalid Email Entered..!!", badRequestResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenInvalidPasswordIsProvided()
        {
            var loginDetails = fixture.Create<LoginRequestDto>();
            var hrDetails = fixture.Create<Admindetail>();
            
            serviceMock.Setup(x => x.GetHRbyEmailAsync(loginDetails.Email.ToLower())).ReturnsAsync(hrDetails);
            localizerMock.Setup(loc => loc["InvalidPassword"]).Returns(new LocalizedString("InvalidPassword", "Incorrect Password Entered..!!"));

            var result = await sut.Login(loginDetails).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal("Incorrect Password Entered..!!", badRequestResult.Value);
        }

        //ChangeHRPassword Action Method Unit Test cases
        [Fact]
        public async Task ChangeHRPassword_ShouldReturnOkResponse_WhenHrFoundWithGivenIdAndValidNewCredentialsAreProvided()
        {
            var hrId = fixture.Create<int>();
            var hrDetails = fixture.Create<Admindetail>();
            hrDetails.Email = hrDetails.Email.ToLower();
            var newCredentials = new ChangePasswordRequestDto { Email = hrDetails.Email, OldPassword = hrDetails.Password, NewPassword = "asdfafasdf"};
            var timelineDetails = fixture.Create<Timelinedetail>();
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.ChangeHRPasswordAsync(hrDetails,newCredentials)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.AddTimeLineAsync(timelineDetails)).ReturnsAsync(timelineDetails);
            localizerMock.Setup(loc => loc["HRLoginPasswordChangeSuccess", hrDetails.EmpId]).Returns(new LocalizedString("HRLoginPasswordChangeSuccess", $"Successfully changed the HR Login password of portal for HR with ID = {hrId}"));

            var result = await sut.ChangeHRPassword(hrId, newCredentials).ConfigureAwait(false);

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            var okObjectResult = (OkObjectResult)result;
            Assert.Equal($"Successfully changed the HR Login password of portal for HR with ID = {hrId}", okObjectResult.Value);
        }

        [Fact]
        public async Task ChangeHRPassword_ShouldReturnBadRequest_WhenHrNotFoundWithGivenId()
        {
            var hrId = fixture.Create<int>();
            Admindetail? hrDetails = null;
            var newCredentials = fixture.Create<ChangePasswordRequestDto>();
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            localizerMock.Setup(loc => loc["HRDoNotExist", hrId]).Returns(new LocalizedString("HRDoNotExist", $"HR with the given ID = {hrId} does not exist!!"));

            var result = await sut.ChangeHRPassword(hrId, newCredentials).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal($"HR with the given ID = {hrId} does not exist!!", badRequestResult.Value);
        }

        [Fact]
        public async Task ChangeHRPassword_ShouldReturnOkResponse_WhenHrFoundWithGivenIdAndInvalidEmailIsProvided()
        {
            var hrId = fixture.Create<int>();
            var hrDetails = fixture.Create<Admindetail>();
            var newCredentials = new ChangePasswordRequestDto { Email = "xuasdfasdf" };
            
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            localizerMock.Setup(loc => loc["InvalidEmail"]).Returns(new LocalizedString("InvalidEmail", "Invalid Email Entered..!!"));

            var result = await sut.ChangeHRPassword(hrId, newCredentials).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal("Invalid Email Entered..!!", badRequestResult.Value);
        }

        [Fact]
        public async Task ChangeHRPassword_ShouldReturnOkResponse_WhenHrFoundWithGivenIdAndInvalidOldPasswordIsProvided()
        {
            var hrId = fixture.Create<int>();
            var hrDetails = fixture.Create<Admindetail>();
            hrDetails.Email = hrDetails.Email.ToLower();
            var newCredentials = new ChangePasswordRequestDto { Email = hrDetails.Email, OldPassword = "dfhaksghfk" };

            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            localizerMock.Setup(loc => loc["IncorrectOldPassword"]).Returns(new LocalizedString("IncorrectOldPassword", "Incorrect Old Password. Provide the correct old password!!"));

            var result = await sut.ChangeHRPassword(hrId, newCredentials).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal("Incorrect Old Password. Provide the correct old password!!", badRequestResult.Value);
        }

        [Fact]
        public async Task ChangeHRPassword_ShouldReturnOkResponse_WhenHrFoundWithGivenIdAndInvalidNewPasswordIsProvided()
        {
            var hrId = fixture.Create<int>();
            var hrDetails = fixture.Create<Admindetail>();
            hrDetails.Email = hrDetails.Email.ToLower();
            var newCredentials = new ChangePasswordRequestDto { Email = hrDetails.Email, OldPassword = hrDetails.Password, NewPassword = hrDetails.Password };

            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            localizerMock.Setup(loc => loc["DifferentNewAndOldPasswords"]).Returns(new LocalizedString("DifferentNewAndOldPasswords", "New Password cannot be the same as Old Password. Try giving a different New Password."));

            var result = await sut.ChangeHRPassword(hrId, newCredentials).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal("New Password cannot be the same as Old Password. Try giving a different New Password.", badRequestResult.Value);
        }

        //CreateEmployee Action Method Unit Tests
        [Fact]
        public async Task CreateEmployee_ShouldReturnCreatedAtActionResult_WhenHrFoundWithGivenIdAndValidNewEmployeeDetailsAreProvided()
        {
            var hrId = fixture.Create<int>();
            var hrDetails = fixture.Create<Admindetail>();
            var createEmployeeDto = fixture.Create<CreateEmployeeRequestDto>();
            Employeedetail? emailUser = null;
            var departmentDetails = fixture.Create<Departmentdetail>();
            var managerDetails = fixture.Create<Managerdetail>();
            var employeeDetailsDomain = fixture.Create<Employeedetail>();
            var timelineDetail = fixture.Create<Timelinedetail>();
            var employeeDetailDto = fixture.Create<EmployeeDetailsDto>();
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.GetEmployeeByEmailAsync(createEmployeeDto.Email)).ReturnsAsync(emailUser);
            serviceMock.Setup(x => x.GetDepartmentByIdAsync(createEmployeeDto.Department)).ReturnsAsync(departmentDetails);
            serviceMock.Setup(x => x.GetManagerByIdAsync(createEmployeeDto.ManagerId)).ReturnsAsync(managerDetails);
            serviceMock.Setup(x => x.AddEmployeeAsync(employeeDetailsDomain)).ReturnsAsync(employeeDetailsDomain);
            serviceMock.Setup(x => x.AddTimeLineAsync(timelineDetail)).ReturnsAsync(timelineDetail);

            mapperMock.Setup(mapper => mapper.Map<Employeedetail>(createEmployeeDto)).Returns(employeeDetailsDomain);
            mapperMock.Setup(mapper => mapper.Map<EmployeeDetailsDto>(employeeDetailsDomain)).Returns(employeeDetailDto);

            var result = await sut.CreateEmployee(hrId,createEmployeeDto).ConfigureAwait(false);

            Assert.IsType<CreatedAtActionResult>(result);
            result.Should().NotBeNull();
            var createdAtActionResult = (CreatedAtActionResult)result;
            Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.Equal(employeeDetailDto.ToString(), (createdAtActionResult.Value ?? "").ToString());
        }

        [Fact]
        public async Task CreateEmployee_ShouldReturnBadRequest_WhenHrNotFoundWithGivenId()
        {
            var hrId = fixture.Create<int>();
            Admindetail? hrDetails = null;
            var createEmployeeDto = fixture.Create<CreateEmployeeRequestDto>();
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);

            localizerMock.Setup(loc => loc["HRDoNotExist", hrId]).Returns(new LocalizedString("HRDoNotExist", $"HR with the given ID = {hrId} does not exist!!"));

            var result = await sut.CreateEmployee(hrId, createEmployeeDto).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal($"HR with the given ID = {hrId} does not exist!!", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateEmployee_ShouldReturn409Conflict_WhenHrFoundWithGivenIdAndEmailIsAlreadyInUse()
        {
            var hrId = fixture.Create<int>();
            var hrDetails = fixture.Create<Admindetail>();
            var createEmployeeDto = fixture.Create<CreateEmployeeRequestDto>();
            var emailUser = fixture.Create<Employeedetail>();
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.GetEmployeeByEmailAsync(createEmployeeDto.Email)).ReturnsAsync(emailUser);
            localizerMock.Setup(loc => loc["EmailAlreadyInUse"]).Returns(new LocalizedString("EmailAlreadyInUse", "Email is already in use..!! Try using a different Email"));

            var result = await sut.CreateEmployee(hrId,createEmployeeDto).ConfigureAwait(false);

            Assert.IsType<ObjectResult>(result);
            result.Should().NotBeNull();
            var objectResult = (ObjectResult)result;
            Assert.Equal(StatusCodes.Status409Conflict, objectResult.StatusCode);
            Assert.Equal(new { Message = "Email is already in use..!! Try using a different Email" }.ToString(), (objectResult.Value ?? "").ToString());
        }

        [Fact]
        public async Task CreateEmployee_ShouldReturnNotFound_WhenHrFoundWithGivenIdAndDepartmentIsNotFound()
        {
            var hrId = fixture.Create<int>();
            var hrDetails = fixture.Create<Admindetail>();
            var createEmployeeDto = fixture.Create<CreateEmployeeRequestDto>();
            Employeedetail? emailUser = null;
            Departmentdetail? department = null;
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.GetEmployeeByEmailAsync(createEmployeeDto.Email)).ReturnsAsync(emailUser);
            serviceMock.Setup(x => x.GetDepartmentByIdAsync(createEmployeeDto.Department)).ReturnsAsync(department);
            localizerMock.Setup(loc => loc["DepartmentDoesNotExist", createEmployeeDto.Department]).Returns(new LocalizedString("DepartmentDoesNotExist", $"The given Department ID {createEmployeeDto.Department} does not exist in the organisation!! Provide a valid Department ID"));

            var result = await sut.CreateEmployee(hrId, createEmployeeDto).ConfigureAwait(false);

            Assert.IsType<NotFoundObjectResult>(result);
            result.Should().NotBeNull();
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.Equal($"The given Department ID {createEmployeeDto.Department} does not exist in the organisation!! Provide a valid Department ID", notFoundResult.Value);
        }

        [Fact]
        public async Task CreateEmployee_ShouldReturnNotFound_WhenHrFoundWithGivenIdAndManagerIsNotFound()
        {
            var hrId = fixture.Create<int>();
            var hrDetails = fixture.Create<Admindetail>();
            var createEmployeeDto = fixture.Create<CreateEmployeeRequestDto>();
            Employeedetail? emailUser = null;
            var department = fixture.Create<Departmentdetail>();
            Managerdetail? manager = null;
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.GetEmployeeByEmailAsync(createEmployeeDto.Email)).ReturnsAsync(emailUser);
            serviceMock.Setup(x => x.GetDepartmentByIdAsync(createEmployeeDto.Department)).ReturnsAsync(department);
            serviceMock.Setup(x => x.GetManagerByIdAsync(createEmployeeDto.ManagerId)).ReturnsAsync(manager);
            localizerMock.Setup(loc => loc["ManagerDoesNotExist", createEmployeeDto.ManagerId]).Returns(new LocalizedString("ManagerDoesNotExist", $"Manager with given ID = {createEmployeeDto.ManagerId} does not exist!! Give a valid Manager ID"));

            var result = await sut.CreateEmployee(hrId, createEmployeeDto).ConfigureAwait(false);

            Assert.IsType<NotFoundObjectResult>(result);
            result.Should().NotBeNull();
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.Equal($"Manager with given ID = {createEmployeeDto.ManagerId} does not exist!! Give a valid Manager ID", notFoundResult.Value);
        }

        //GetAllEmployees Action Method Unit Test cases
        [Fact]
        public async Task GetAllEmployees_ShouldReturnOkResponseWithListOfEmployees_WhenEmployeesFound()
        {
            var employeesDomainList = fixture.Create<List<Employeedetail>>();
            var employeesDtoList = fixture.Create<List<EmployeeDetailsDto>>();
            serviceMock.Setup(x => x.GetAllEmployeesAsync()).ReturnsAsync(employeesDomainList);
            mapperMock.Setup(mapper => mapper.Map<List<EmployeeDetailsDto>>(employeesDomainList)).Returns(employeesDtoList);

            var result = await sut.GetAllEmployees().ConfigureAwait(false);

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            var okObjectResult = (OkObjectResult)result;
            Assert.Equal(employeesDtoList, okObjectResult.Value);
        }

        [Fact]
        public async Task GetAllEmployees_ShouldReturnOkResponse_WhenZeroEmployeeRecordsAreFound()
        {
            var employeesDomainList = new List<Employeedetail> { };
            serviceMock.Setup(x => x.GetAllEmployeesAsync()).ReturnsAsync(employeesDomainList);
            localizerMock.Setup(loc => loc["NoEmployees"]).Returns(new LocalizedString("NoEmployees", "There are no employees at present to display"));

            var result = await sut.GetAllEmployees().ConfigureAwait(false);

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            var okObjectResult = (OkObjectResult)result;
            Assert.Equal("There are no employees at present to display", okObjectResult.Value);
        }

        //GetEmployee Action Method Unit Test cases
        [Fact]
        public async Task GetEmployee_ShouldReturnOkResponse_WhenEmployeeFoundWithGivenId()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            var employeeDto = fixture.Create<EmployeeDetailsDto>();
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            mapperMock.Setup(mapper => mapper.Map<EmployeeDetailsDto>(employeeDetails)).Returns(employeeDto);

            var result = await sut.GetEmployee(empId).ConfigureAwait(false);

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            var okObjectResult = (OkObjectResult)result;
            Assert.Equal(employeeDto.ToString(), (okObjectResult.Value ?? "").ToString());
        }

        [Fact]
        public async Task GetEmployee_ShouldReturnNotFound_WhenEmployeeWithGivenIdNotFound()
        {
            var empId = fixture.Create<int>();
            Employeedetail? employeeDetails = null;
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            localizerMock.Setup(loc => loc["EmployeeDoesNotExist", empId]).Returns(new LocalizedString("EmployeeDoesNotExist", $"Employee with given ID = {empId} does not exist!!"));

            var result = await sut.GetEmployee(empId).ConfigureAwait(false);

            Assert.IsType<NotFoundObjectResult>(result);
            result.Should().NotBeNull();
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.Equal($"Employee with given ID = {empId} does not exist!!", notFoundResult.Value);
        }

        //DeleteEmployee Action Method Unit Test cases
        [Fact]
        public async Task DeleteEmployee_ShouldReturnOkResponse_WhenHrAndEmployeeFoundWithGivenIds()
        {
            var hrId = fixture.Create<int>();
            var empId = fixture.Create<int>();
            var hrDetails = fixture.Create<Admindetail>();
            var employeeDomain = fixture.Create<Employeedetail>();
            var employeeDto = fixture.Create<EmployeeDetailsDto>();
            var timelineDetails = fixture.Create<Timelinedetail>();
            var objectInResult = new { Message = "Employee with following details has been deleted successfully", deletedEmployeeDetails = employeeDto };
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDomain);
            serviceMock.Setup(x => x.DeleteEmployeeAsync(employeeDomain)).ReturnsAsync(employeeDomain);
            serviceMock.Setup(x => x.AddTimeLineAsync(timelineDetails)).ReturnsAsync(timelineDetails);

            mapperMock.Setup(mapper => mapper.Map<EmployeeDetailsDto>(employeeDomain)).Returns(employeeDto);
            localizerMock.Setup(loc => loc["EmployeeDeletionSuccess"]).Returns(new LocalizedString("EmployeeDeletionSuccess", "Employee with following details has been deleted successfully"));

            var result = await sut.DeleteEmployee(hrId,empId).ConfigureAwait(false);

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            var okObjectResult = (OkObjectResult)result;
            Assert.Equal(objectInResult.ToString(), (okObjectResult.Value ?? "").ToString());
        }

        [Fact]
        public async Task DeleteEmployee_ShouldReturnBadRequest_WhenHrFoundAndEmployeeNotFoundWithGivenIds()
        {
            var hrId = fixture.Create<int>();
            var empId = fixture.Create<int>();
            var hrDetails = fixture.Create<Admindetail>();
            Employeedetail? employeeDomain = null;
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDomain);

            localizerMock.Setup(loc => loc["EmployeeDoesNotExist",empId]).Returns(new LocalizedString("EmployeeDoesNotExist", $"Employee with given ID = {empId} does not exist!!"));

            var result = await sut.DeleteEmployee(hrId, empId).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal($"Employee with given ID = {empId} does not exist!!", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteEmployee_ShouldReturnBadRequest_WhenHrFoundNotFoundWithGivenId()
        {
            var hrId = fixture.Create<int>();
            var empId = fixture.Create<int>();
            Admindetail? hrDetails = null;
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);

            localizerMock.Setup(loc => loc["HRDoNotExist", hrId]).Returns(new LocalizedString("HRDoNotExist", $"HR with the given ID = {hrId} does not exist!!"));

            var result = await sut.DeleteEmployee(hrId, empId).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal($"HR with the given ID = {hrId} does not exist!!", badRequestResult.Value);
        }

        //UpdateEmployee Action Method Unit Test cases
        [Fact]
        public async Task UpdateEmployee_ShouldReturnOkResponse_WhenHrAndEmployeeFoundWithGivenIdsAndValidDetailsAreProvided()
        {
            var hrId = fixture.Create<int>();
            var empId = fixture.Create<int>();
            var newDetails = fixture.Create<UpdateEmployeeByHRRequestDto>();
            var hrDetails = fixture.Create<Admindetail>();
            var employeeDetails = fixture.Create<Employeedetail>();
            var departmentDetails = fixture.Create<Departmentdetail>();
            var managerDetails = fixture.Create<Managerdetail>();
            var timelineDetails = fixture.Create<Timelinedetail>();
            var employeeDetailsDto = fixture.Create<EmployeeDetailsDto>();
            var resultObject = new { msg = $"Updated the details of employee with id {employeeDetailsDto.Id} successfully", updatedEmployeeDTO = employeeDetailsDto };
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.GetDepartmentByIdAsync(newDetails.Department)).ReturnsAsync(departmentDetails);
            serviceMock.Setup(x => x.GetManagerByIdAsync(newDetails.ManagerId)).ReturnsAsync(managerDetails);
            serviceMock.Setup(x => x.UpdateEmployeeDetailsAsync(employeeDetails, newDetails)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.AddTimeLineAsync(timelineDetails)).ReturnsAsync(timelineDetails);

            mapperMock.Setup(mapper => mapper.Map<EmployeeDetailsDto>(employeeDetails)).Returns(employeeDetailsDto);
            localizerMock.Setup(loc => loc["EmployeeDetailsUpdationSuccess", employeeDetailsDto.Id]).Returns(new LocalizedString("EmployeeDetailsUpdationSuccess", $"Updated the details of employee with id {employeeDetailsDto.Id} successfully"));

            var result = await sut.UpdateEmployee(hrId, empId, newDetails).ConfigureAwait(false);

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            var okObjectResult = (OkObjectResult)result;
            Assert.Equal(resultObject.ToString(), (okObjectResult.Value ?? "").ToString());
        }

        [Fact]
        public async Task UpdateEmployee_ShouldReturnNotFound_WhenManagerNotFound()
        {
            var hrId = fixture.Create<int>();
            var empId = fixture.Create<int>();
            var newDetails = fixture.Create<UpdateEmployeeByHRRequestDto>();
            var hrDetails = fixture.Create<Admindetail>();
            var employeeDetails = fixture.Create<Employeedetail>();
            var departmentDetails = fixture.Create<Departmentdetail>();
            Managerdetail? managerDetails = null;
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.GetDepartmentByIdAsync(newDetails.Department)).ReturnsAsync(departmentDetails);
            serviceMock.Setup(x => x.GetManagerByIdAsync(newDetails.ManagerId)).ReturnsAsync(managerDetails);

            localizerMock.Setup(loc => loc["ManagerDoesNotExist", newDetails.ManagerId]).Returns(new LocalizedString("ManagerDoesNotExist", $"Manager with given ID = {newDetails.ManagerId} does not exist!! Give a valid Manager ID"));

            var result = await sut.UpdateEmployee(hrId, empId, newDetails).ConfigureAwait(false);

            Assert.IsType<NotFoundObjectResult>(result);
            result.Should().NotBeNull();
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.Equal($"Manager with given ID = {newDetails.ManagerId} does not exist!! Give a valid Manager ID", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateEmployee_ShouldReturnNotFound_WhenDepartmentNotFound()
        {
            var hrId = fixture.Create<int>();
            var empId = fixture.Create<int>();
            var newDetails = fixture.Create<UpdateEmployeeByHRRequestDto>();
            var hrDetails = fixture.Create<Admindetail>();
            var employeeDetails = fixture.Create<Employeedetail>();
            Departmentdetail? departmentDetails = null;
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.GetDepartmentByIdAsync(newDetails.Department)).ReturnsAsync(departmentDetails);

            localizerMock.Setup(loc => loc["DepartmentDoesNotExist", newDetails.Department]).Returns(new LocalizedString("DepartmentDoesNotExist", $"The given Department ID {newDetails.Department} does not exist in the organisation!! Provide a valid Department ID"));

            var result = await sut.UpdateEmployee(hrId, empId, newDetails).ConfigureAwait(false);

            Assert.IsType<NotFoundObjectResult>(result);
            result.Should().NotBeNull();
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.Equal($"The given Department ID {newDetails.Department} does not exist in the organisation!! Provide a valid Department ID", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateEmployee_ShouldReturnNotFound_WhenEmployeeWithGivenIdNotFound()
        {
            var hrId = fixture.Create<int>();
            var empId = fixture.Create<int>();
            var newDetails = fixture.Create<UpdateEmployeeByHRRequestDto>();
            var hrDetails = fixture.Create<Admindetail>();
            Employeedetail? employeeDetails = null;
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);

            localizerMock.Setup(loc => loc["EmployeeDoesNotExist", empId]).Returns(new LocalizedString("EmployeeDoesNotExist", $"Employee with given ID = {empId} does not exist!!"));

            var result = await sut.UpdateEmployee(hrId, empId, newDetails).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal($"Employee with given ID = {empId} does not exist!!", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateEmployee_ShouldReturnNotFound_WhenHrWithGivenIdNotFound()
        {
            var hrId = fixture.Create<int>();
            var empId = fixture.Create<int>();
            var newDetails = fixture.Create<UpdateEmployeeByHRRequestDto>();
            Admindetail? hrDetails = null;
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);

            localizerMock.Setup(loc => loc["HRDoNotExist", hrId]).Returns(new LocalizedString("HRDoNotExist", $"HR with the given ID = {hrId} does not exist!!"));

            var result = await sut.UpdateEmployee(hrId, empId, newDetails).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal($"HR with the given ID = {hrId} does not exist!!", badRequestResult.Value);
        }

        //GetEmployeesReportingToManager Action Method Unit Test cases
        [Fact]
        public async Task GetEmployeesReportingToManager_ShouldReturnOkResponse_WhenManagerFoundWithGivenId()
        {
            var managerId = fixture.Create<int>();
            var manager = fixture.Create<Managerdetail>();
            var employeesDomainList = fixture.Create<List<Employeedetail>>();
            var employeeDtoList = fixture.Create<List<EmployeeDetailsDto>>();
            serviceMock.Setup(x => x.GetManagerByIdAsync(managerId)).ReturnsAsync(manager);
            serviceMock.Setup(x => x.GetAllEmployeesUnderManagerAsync(managerId)).ReturnsAsync(employeesDomainList);

            mapperMock.Setup(mapper => mapper.Map<List<EmployeeDetailsDto>>(employeesDomainList)).Returns(employeeDtoList);

            var result = await sut.GetEmployeesReportingToManager(managerId).ConfigureAwait(false);

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            var okObjectResult = (OkObjectResult)result;
            Assert.Equal(employeeDtoList,okObjectResult.Value);
        }

        [Fact]
        public async Task GetEmployeesReportingToManager_ShouldReturnOkResponse_WhenManagerFoundWithGivenIdAndZeroEmployeesAreFoundReportingToManager()
        {
            var managerId = fixture.Create<int>();
            var manager = fixture.Create<Managerdetail>();
            var employeesDomainList = new List<Employeedetail> { };
            serviceMock.Setup(x => x.GetManagerByIdAsync(managerId)).ReturnsAsync(manager);
            serviceMock.Setup(x => x.GetAllEmployeesUnderManagerAsync(managerId)).ReturnsAsync(employeesDomainList);
            localizerMock.Setup(loc => loc["NoEmployees"]).Returns(new LocalizedString("NoEmployees", "There are no employees at present to display"));

            var result = await sut.GetEmployeesReportingToManager(managerId).ConfigureAwait(false);

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            var okObjectResult = (OkObjectResult)result;
            Assert.Equal("There are no employees at present to display", okObjectResult.Value);
        }

        [Fact]
        public async Task GetEmployeesReportingToManager_ShouldReturnBadRequest_WhenManagerNotFoundWithGivenId()
        {
            var managerId = fixture.Create<int>();
            Managerdetail? manager = null;
            serviceMock.Setup(x => x.GetManagerByIdAsync(managerId)).ReturnsAsync(manager);
            localizerMock.Setup(loc => loc["ManagerDoesNotExist",managerId]).Returns(new LocalizedString("ManagerDoesNotExist", $"Manager with given ID = {managerId} does not exist!! Give a valid Manager ID"));

            var result = await sut.GetEmployeesReportingToManager(managerId).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal($"Manager with given ID = {managerId} does not exist!! Give a valid Manager ID", badRequestResult.Value);
        }

        //AddManager Action Method Unit Test cases
        [Fact]
        public async Task AddManager_ShouldReturnOkResponse_WhenHrAndEmployeeFoundWithGivenIds()
        {
            var hrId = fixture.Create<int>();
            var empId = fixture.Create<int>();
            var hrDetails = fixture.Create<Admindetail>();
            var employeeDetails = fixture.Create<Employeedetail>();
            var managerDetails = fixture.Create<Managerdetail>();
            var mangerDetailsDto = fixture.Create<ManagerDetailsDto>();
            var timelineDetails = fixture.Create<Timelinedetail>();
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.AddManagerAsync(employeeDetails)).ReturnsAsync(managerDetails);
            serviceMock.Setup(x => x.AddTimeLineAsync(timelineDetails)).ReturnsAsync(timelineDetails);
            mapperMock.Setup(mapper => mapper.Map<ManagerDetailsDto>(managerDetails)).Returns(mangerDetailsDto);

            var result = await sut.AddManager(hrId, empId).ConfigureAwait(false);

            Assert.IsType<ObjectResult>(result);
            result.Should().NotBeNull();
            var objectResult = (ObjectResult)result;
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.Equal(mangerDetailsDto.ToString(), (objectResult.Value ?? "").ToString());
        }

        [Fact]
        public async Task AddManager_ShouldReturnOkResponse_WhenHrFoundAndEmployeeNotFoundWithGivenIds()
        {
            var hrId = fixture.Create<int>();
            var empId = fixture.Create<int>();
            var hrDetails = fixture.Create<Admindetail>();
            Employeedetail? employeeDetails = null;
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            localizerMock.Setup(loc => loc["EmployeeDoesNotExist", empId]).Returns(new LocalizedString("EmployeeDoesNotExist", $"Employee with given ID = {empId} does not exist!!"));

            var result = await sut.AddManager(hrId, empId).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal($"Employee with given ID = {empId} does not exist!!", badRequestResult.Value);
        }

        [Fact]
        public async Task AddManager_ShouldReturnOkResponse_WhenHrNotFoundWithGivenId()
        {
            var hrId = fixture.Create<int>();
            var empId = fixture.Create<int>();
            Admindetail? hrDetails = null;
            serviceMock.Setup(x => x.GetHRByIdAsync(hrId)).ReturnsAsync(hrDetails);
            localizerMock.Setup(loc => loc["HRDoNotExist", hrId]).Returns(new LocalizedString("HRDoNotExist", $"HR with the given ID = {hrId} does not exist!!"));

            var result = await sut.AddManager(hrId, empId).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal($"HR with the given ID = {hrId} does not exist!!", badRequestResult.Value);
        }
    }
}
