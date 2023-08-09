using AutoMapper;
using EF_HRportal_WebAPI.Models.Domain;
using EF_HRportal_WebAPI.Models.DTOs;

namespace EF_HRportal_WebAPI.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles() 
        {
            CreateMap<CreateEmployeeRequestDTO, Employeedetail>().ReverseMap();
            CreateMap<Employeedetail,EmployeeDetailsDTO>().
                ForMember(x=> x.ManagerName, opt => opt.MapFrom(src => src.Manager.Name)).
                ForMember(x=> x.ManagerEmail, opt => opt.MapFrom(src => src.Manager.Email)).ReverseMap();
            CreateMap<Managerdetail, ManagerDetailsDTO>().ReverseMap();
            CreateMap<Employeedetail, PersonalDetailsDTO>().
                ForMember(x => x.ManagerName, opt => opt.MapFrom(src => src.Manager.Name)).
                ForMember(x => x.ManagerEmail, opt => opt.MapFrom(src => src.Manager.Email)).ReverseMap();
            CreateMap<Timelinedetail, TimelineDetailsDTO>().ReverseMap();
        }
    }
}
