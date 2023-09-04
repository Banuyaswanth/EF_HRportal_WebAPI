using AutoFixture;
using AutoMapper;
using EF_HRportal_WebAPI.Controllers;
using EF_HRportal_WebAPI.Models.Domain;
using EF_HRportal_WebAPI.Models.DTOs;
using EF_HRportal_WebAPI.Repository;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF_HRportal_WebAPI.UnitTests.Controllers
{
    public class TimelineControllerTests
    {
        private readonly IFixture fixture;
        private readonly Mock<IRepository> serviceMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<IStringLocalizer<TimelineController>> localizerMock;
        private readonly TimelineController sut;

        public TimelineControllerTests()
        {
            fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));
            serviceMock = fixture.Freeze<Mock<IRepository>>();
            mapperMock = fixture.Freeze<Mock<IMapper>>();
            localizerMock = new Mock<IStringLocalizer<TimelineController>>();
            sut = new TimelineController(serviceMock.Object, mapperMock.Object, localizerMock.Object);
        }

        //GetTimeLine Action Method Unit Test cases
        [Fact]
        public async Task GetTimeLine_ShouldReturnOkResponse_WhenEmployeeFoundWithTimelineDetails()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            var timelineDetails = fixture.Create<List<Timelinedetail>>();
            var timelineDetailsDto = fixture.Create<List<TimelineDetailsDto>>();
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.GetTimeLineAsync(empId)).ReturnsAsync(timelineDetails);
            mapperMock.Setup(mapper => mapper.Map<List<TimelineDetailsDto>>(timelineDetails)).Returns(timelineDetailsDto);

            var result = await sut.GetTimeLine(empId).ConfigureAwait(false);

            Assert.IsType<OkObjectResult>(result);
            var okObjectResult = (OkObjectResult)result;
            Assert.Equal(timelineDetailsDto, okObjectResult.Value);
        }

        [Fact]
        public async Task GetTimeLine_ShouldReturnOkResponse_WhenEmployeeFoundWithZeroTimelineRecords()
        {
            var empId = fixture.Create<int>();
            var employeeDetails = fixture.Create<Employeedetail>();
            var timelineDetails = new List<Timelinedetail> { };
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeeDetails);
            serviceMock.Setup(x => x.GetTimeLineAsync(empId)).ReturnsAsync(timelineDetails);

            localizerMock.Setup(loc => loc["NoTimelineActions", empId]).Returns(new LocalizedString("NoTimelineActions", $"No timeline actions found for the employee with ID = {empId}"));

            var result = await sut.GetTimeLine(empId).ConfigureAwait(false);
            var okObjectResult = (OkObjectResult)result;
            Assert.IsType<OkObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal($"No timeline actions found for the employee with ID = {empId}", okObjectResult.Value);
        }

        [Fact]
        public async Task GetTimeLine_ShouldReturnBadRequest_WhenEmployeeWithGivenIdNotFound()
        {
            var empId = fixture.Create<int>();
            Employeedetail? employeedetail = null;
            serviceMock.Setup(x => x.GetEmployeeByIdAsync(empId)).ReturnsAsync(employeedetail);

            localizerMock.Setup(loc => loc["EmployeeDoesNotExist", empId]).Returns(new LocalizedString("EmployeeDoesNotExist", $"Employee with given ID = {empId} does not exist!!"));

            var result = await sut.GetTimeLine(empId).ConfigureAwait(false);
            var badRequestResult = (BadRequestObjectResult)result;

            Assert.IsType<BadRequestObjectResult>(result);
            result.Should().NotBeNull();
            Assert.Equal($"Employee with given ID = {empId} does not exist!!", badRequestResult.Value);
        }
    }
}
