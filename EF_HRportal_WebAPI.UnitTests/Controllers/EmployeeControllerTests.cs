using AutoFixture;
using AutoMapper;
using EF_HRportal_WebAPI.Controllers;
using EF_HRportal_WebAPI.Models.Domain;
using EF_HRportal_WebAPI.Models.DTOs;
using EF_HRportal_WebAPI.Repository;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_HRportal_WebAPI.UnitTests.Controllers
{
    public class EmployeeControllerTests
    {
        private readonly IFixture fixture;
        private readonly Mock<IRepository> serviceMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<IStringLocalizer<EmployeeController>> localizerMock;
        private readonly EmployeeController sut;

        public EmployeeControllerTests()
        {
            fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));
            serviceMock = fixture.Freeze<Mock<IRepository>>();
            mapperMock = fixture.Freeze<Mock<IMapper>>();
            localizerMock = new Mock<IStringLocalizer<EmployeeController>>();
            sut = new EmployeeController(serviceMock.Object, mapperMock.Object, localizerMock.Object);
        }

        //Login Action Method Unit Test cases
        [Fact]
        public async Task Login_ShouldReturnOkObject_WhenCorrectLoginCredentialsAreProvided()
        {
            var employeeDetails = fixture.Create<Employeedetail>();
            var loginDetails = new LoginRequestDto
            {
                Email = employeeDetails.Email,
                Password = employeeDetails.Password,
            };
            serviceMock.Setup(x => x.GetEmployeeByEmailAsync(loginDetails.Email.ToLower())).ReturnsAsync(employeeDetails);

            localizerMock.Setup(loc => loc["LoginSuccessful"]).Returns(new LocalizedString("LoginSuccessful", "Login Successful"));

            var result = await sut.Login(loginDetails).ConfigureAwait(false);
            var okObjectResult = (OkObjectResult)result;

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal("Login Successful", okObjectResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenInvalidPasswordIsProvided()
        {
            var employeeDetails = fixture.Create<Employeedetail>();
            var loginDetails = new LoginRequestDto { Email = employeeDetails.Email, Password = "xyz" };
            serviceMock.Setup(x => x.GetEmployeeByEmailAsync(loginDetails.Email.ToLower())).ReturnsAsync(employeeDetails);
            localizerMock.Setup(loc => loc["InvalidPassword"]).Returns(new LocalizedString("InvalidPassword", "Incorrect Password Entered..!!"));

            var result = await sut.Login(loginDetails).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal("Incorrect Password Entered..!!", badRequestResult.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenInvalidEmailIsProvided()
        {
            Employeedetail? employeeDetails = null;
            var loginDetails = new LoginRequestDto { Email = "xyz", Password = "SomeRandomPassword" };
            serviceMock.Setup(x => x.GetEmployeeByEmailAsync(loginDetails.Email.ToLower())).ReturnsAsync(employeeDetails);
            localizerMock.Setup(loc => loc["InvalidEmail"]).Returns(new LocalizedString("InvalidEmail", "Invalid Email Entered..!!"));

            var result = await sut.Login(loginDetails).ConfigureAwait(false);
            var badRequestResult = (BadRequestObjectResult)result;

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal("Invalid Email Entered..!!", badRequestResult.Value);
        }

        //SearchEmployee Action Method Unit Test cases
        [Fact]
        public async Task SearchEmployee_ShouldReturnOkResponse_WhenEmployeesFoundWithNameIncludingGivenInput()
        {
            var Name = "name";
            var employeeDetailsList = fixture.Create<List<Employeedetail>>();
            var employeeDetailsDtoList = fixture.Create<List<EmployeeDetailsDto>>();
            serviceMock.Setup(x => x.SearchEmployeeByNameAsync(Name ?? "")).ReturnsAsync(employeeDetailsList);
            mapperMock.Setup(mapper => mapper.Map<List<EmployeeDetailsDto>>(employeeDetailsList)).Returns(employeeDetailsDtoList);

            var result = await sut.SearchEmployee(Name).ConfigureAwait(false);
            var okObjectResult = (OkObjectResult)result;

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal(employeeDetailsDtoList,okObjectResult.Value);
        }

        [Fact]
        public async Task SearchEmployee_ShouldReturnNotFoundResponse_WhenEmployeesNotFoundWithNameIncludingGivenInput()
        {
            var Name = "Name";
            List<Employeedetail> employeedetailsList = new() { };
            serviceMock.Setup(x => x.SearchEmployeeByNameAsync(Name)).ReturnsAsync(employeedetailsList);
            localizerMock.Setup(loc => loc["EmployeeNameSearchFail", Name ?? ""]).Returns(new LocalizedString("EmployeeNameSearchFail", $"Could not find any Employee with the given name '{Name}'"));

            var result = await sut.SearchEmployee(Name).ConfigureAwait(false);
            var notFoundResult = (NotFoundObjectResult)result;

            Assert.IsType<NotFoundObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal($"Could not find any Employee with the given name '{Name}'", notFoundResult.Value);
        }

        //GetPersonalDetails Action Method Unit Test cases
        [Fact]
        public async Task GetPersonalDetails_ShouldReturnOkResponse_WhenEmployeeFoundWithGivenId()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            var personalDetails = fixture.Create<PersonalDetailsDto>();
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            mapperMock.Setup(mapper => mapper.Map<PersonalDetailsDto>(employeeDetails)).Returns(personalDetails);

            var result = await sut.GetPersonalDetails(empId).ConfigureAwait(false);
            var okObjectResult = (OkObjectResult)result;

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal(personalDetails, okObjectResult.Value);
        }

        [Fact]
        public async Task GetPersonalDetails_ShouldReturnBadRequest_WhenEmployeeNotFoundWithGivenId()
        {
            var empId = fixture.Create<int>();
            Employeedetail? employeedetail = null;
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeedetail);
            localizerMock.Setup(loc => loc["EmployeeDoesNotExist", empId]).Returns(new LocalizedString("EmployeeDoesNotExist", $"Employee with given ID = {empId} does not exist!!"));

            var result = await sut.GetPersonalDetails(empId).ConfigureAwait(false);
            var badRequestResult = (BadRequestObjectResult)result;
            
            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal($"Employee with given ID = {empId} does not exist!!", badRequestResult.Value);
        }

        //UpdatePersonalDetails Action Method Unit Test cases
        [Fact]
        public async Task UpdatePersonalDetails_ShouldReturnOkResponse_WhenEmployeeFoundWithGivenId()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            var newDetails = fixture.Create<UpdatePersonalDetailsDto>();
            var updatedEmployeeDetails = fixture.Create<Employeedetail>();
            var updatedEmployeeDetailsDto = fixture.Create<PersonalDetailsDto>();
            var timelineAction = fixture.Create<Timelinedetail>();
            var updatedPersonalDetailsResult = new { Message = $"Successfully updated the personal details of employee with ID = {empId}", UpdatedDetails = updatedEmployeeDetailsDto };

            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.UpdatePersonalDetailsAsync(employeeDetails, newDetails)).ReturnsAsync(updatedEmployeeDetails);
            serviceMock.Setup(x => x.AddTimeLineAsync(timelineAction)).ReturnsAsync(timelineAction);

            mapperMock.Setup(mapper => mapper.Map<PersonalDetailsDto>(updatedEmployeeDetails)).Returns(updatedEmployeeDetailsDto);

            localizerMock.Setup(loc => loc["PersonalDetailsUpdationSuccess", empId]).Returns(new LocalizedString("PersonalDetailsUpdationSuccess", $"Successfully updated the personal details of employee with ID = {empId}"));

            var result = await sut.UpdatePersonalDetails(empId,newDetails).ConfigureAwait(false);

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            var okObjectResult = (OkObjectResult)result;
            var response = (dynamic)okObjectResult.Value;
            Assert.Equal(updatedPersonalDetailsResult.ToString(), response.ToString());
        }

        [Fact]
        public async Task UpdatePersonalDetails_ShouldReturnBadRequest_WhenEmployeeWithGivenIdNotFound()
        {
            var empId = fixture.Create<int>();
            Employeedetail? employeeDetails = null;
            var newDetails = fixture.Create<UpdatePersonalDetailsDto>();
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            localizerMock.Setup(loc => loc["EmployeeDoesNotExist",empId]).Returns(new LocalizedString("EmployeeDoesNotExist", $"Employee with given ID = {empId} does not exist!!"));

            var result = await sut.UpdatePersonalDetails(empId,newDetails).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal($"Employee with given ID = {empId} does not exist!!",badRequestResult.Value);
        }

        //ChangeLoginPassword Action Method Unit Test cases
        [Fact]
        public async Task ChangeLoginPassword_ShouldReturnOkResponse_WhenEmployeeFoundWithGivenIdAndValidNewCredentialsAreProvided()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            employeeDetails.Email = employeeDetails.Email.ToLower();
            var newCredentials = new ChangePasswordRequestDto { Email = employeeDetails.Email, NewPassword = "xyasdfas", OldPassword = employeeDetails.Password };
            var updatedEmployeeDetials = fixture.Create<Employeedetail>();
            var timelineDetail = fixture.Create<Timelinedetail>();
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.ChangeEmployeePasswordAsync(employeeDetails,newCredentials)).ReturnsAsync(updatedEmployeeDetials);
            serviceMock.Setup(x => x.AddTimeLineAsync(timelineDetail)).ReturnsAsync(timelineDetail);

            localizerMock.Setup(loc => loc["EmployeeLoginPasswordChangeSuccess", empId]).Returns(new LocalizedString("EmployeeLoginPasswordChangeSuccess", $"Successfully changed the password of Employee login for Employee with ID = {empId}"));

            var result = await sut.ChangeLoginPassword(empId,newCredentials).ConfigureAwait(false);

            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            var okObjectResult = (OkObjectResult)result;
            Assert.Equal($"Successfully changed the password of Employee login for Employee with ID = {empId}", okObjectResult.Value);
        }

        [Fact]
        public async Task ChangeLoginPassword_ShouldReturnBadRequest_WhenEmployeeWithGivenIdNotFound()
        {
            var empId = fixture.Create<int>();
            Employeedetail? employeedetails = null;
            var newCredentials = fixture.Create<ChangePasswordRequestDto>();
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeedetails);

            localizerMock.Setup(loc => loc["EmployeeDoesNotExist", empId]).Returns(new LocalizedString("EmployeeDoesNotExist", $"Employee with given ID = {empId} does not exist!!"));

            var result = await sut.ChangeLoginPassword(empId, newCredentials).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal($"Employee with given ID = {empId} does not exist!!",badRequestResult.Value);
        }

        [Fact]
        public async Task ChangeLoginPassword_ShouldReturnBadRequest_WhenInvalidEmailIsProvided()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            var newCredentials = new ChangePasswordRequestDto { Email = "abc" };

            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);

            localizerMock.Setup(loc => loc["InvalidEmail"]).Returns(new LocalizedString("InvalidEmail", "Invalid Email Entered..!!"));

            var result = await sut.ChangeLoginPassword(empId, newCredentials).ConfigureAwait(false);
            
            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal("Invalid Email Entered..!!", badRequestResult.Value);
        }

        [Fact]
        public async Task ChangeLoginPassword_ShouldReturnBadRequest_WhenInvalidOldPasswordIsProvided()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            employeeDetails.Email = employeeDetails.Email.ToLower();
            var newCredentials = new ChangePasswordRequestDto { Email = employeeDetails.Email, OldPassword = "abdasdfasdfa" };

            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);

            localizerMock.Setup(loc => loc["IncorrectOldPassword"]).Returns(new LocalizedString("IncorrectOldPassword", "Incorrect Old Password. Provide the correct old password!!"));

            var result = await sut.ChangeLoginPassword(empId, newCredentials).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal("Incorrect Old Password. Provide the correct old password!!", badRequestResult.Value);
        }

        [Fact]
        public async Task ChangeLoginPassword_ShouldReturnBadRequest_WhenInvalidNewPasswordIsProvided()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            employeeDetails.Email = employeeDetails.Email.ToLower();
            var newCredentials = new ChangePasswordRequestDto { Email = employeeDetails.Email, OldPassword = employeeDetails.Password, NewPassword = employeeDetails.Password };

            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);

            localizerMock.Setup(loc => loc["DifferentNewAndOldPasswords"]).Returns(new LocalizedString("DifferentNewAndOldPasswords", "New Password cannot be the same as Old Password. Try giving a different New Password."));

            var result = await sut.ChangeLoginPassword(empId, newCredentials).ConfigureAwait(false);

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal("New Password cannot be the same as Old Password. Try giving a different New Password.", badRequestResult.Value);
        }
    }
}
