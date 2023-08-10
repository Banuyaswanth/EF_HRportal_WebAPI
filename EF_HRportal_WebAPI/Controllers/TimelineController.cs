using AutoMapper;
using EF_HRportal_WebAPI.Models.DTOs;
using EF_HRportal_WebAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EF_HRportal_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimelineController : ControllerBase
    {
        private readonly IRepository repository;
        private readonly IMapper mapper;

        public TimelineController(IRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [HttpGet("GetTimeline/{EmpId}")]
        public async Task<IActionResult> GetTimeLine([FromRoute] int EmpId)
        {
            var Employee = await repository.GetEmployeeByIdAsync(EmpId);
            if (Employee == null)
            {
                return NotFound("Employee with the given ID does not exist..!!");
            }
            var timeLineDetails = await repository.GetTimeLineAsync(EmpId);
            if (timeLineDetails.Count == 0)
            {
                return NotFound("No timeline actions found for the employee");
            }
            var timeLineDetailsDTO = mapper.Map<List<TimelineDetailsDTO>>(timeLineDetails);
            return Ok(timeLineDetailsDTO);
        }
    }
}
