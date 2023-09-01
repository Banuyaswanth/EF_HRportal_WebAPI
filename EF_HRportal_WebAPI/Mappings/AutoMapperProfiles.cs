using AutoMapper;
using EF_HRportal_WebAPI.Models.Domain;
using EF_HRportal_WebAPI.Models.DTOs;

namespace EF_HRportal_WebAPI.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles() 
        {
            CreateMap<CreateEmployeeRequestDto, Employeedetail>().ReverseMap();
            CreateMap<Employeedetail,EmployeeDetailsDto>().
                ForMember(x=> x.ManagerName, opt => opt.MapFrom(src => src.Manager.Name)).
                ForMember(x=> x.ManagerEmail, opt => opt.MapFrom(src => src.Manager.Email)).ReverseMap();
            CreateMap<Managerdetail, ManagerDetailsDto>().ReverseMap();
            CreateMap<Employeedetail, PersonalDetailsDto>().
                ForMember(x => x.ManagerName, opt => opt.MapFrom(src => src.Manager.Name)).
                ForMember(x => x.ManagerEmail, opt => opt.MapFrom(src => src.Manager.Email)).ReverseMap();
            CreateMap<Timelinedetail, TimelineDetailsDto>().ReverseMap();
        }
    }
}
