using AutoMapper;
using EF_HRportal_WebAPI.Models.DTOs;
using EF_HRportal_WebAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace EF_HRportal_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimelineController : ControllerBase
    {
        private readonly IRepository repository;
        private readonly IMapper mapper;
        private readonly IStringLocalizer<TimelineController> localizer;

        public TimelineController(IRepository repository, IMapper mapper, IStringLocalizer<TimelineController> localizer)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.localizer = localizer;
        }

        //This API will return the List of TimelineDetailsDTO's of the employee whose ID is provided in the Route
        [HttpGet("GetTimeline/{EmpId}")]
        public async Task<IActionResult> GetTimeLine([FromRoute] int EmpId)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee != null)
            {
                var timeLineDetails = await repository.GetTimeLineAsync(EmpId);
                if (timeLineDetails.Count == 0)
                {
                    return Ok(localizer["NoTimelineActions", EmpId].Value);
                }
                var timeLineDetailsDTO = mapper.Map<List<TimelineDetailsDto>>(timeLineDetails);
                return Ok(timeLineDetailsDTO);
            }
            return Ok(localizer["EmployeeDoesNotExist",EmpId].Value);
        }
    }
}
