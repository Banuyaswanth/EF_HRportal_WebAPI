using EF_HRportal_WebAPI.Models.Domain;
using EF_HRportal_WebAPI.Models.DTOs;

namespace EF_HRportal_WebAPI.Repository
{
    public interface IRepository
    {
        Task<Admindetail?> GetHRbyEmailAsync(string email);
        Task<Employeedetail?> GetEmployeeByEmailAsync(string email);
        Task<Employeedetail> AddEmployeeAsync(Employeedetail employeeDetail);
        Task<Timelinedetail> AddTimeLineAsync(Timelinedetail timelineDetail);
        Task<Managerdetail?> GetManagerByIdAsync(int id);
        Task<Admindetail?> GetHRByIdAsync(int id);
        Task<List<Employeedetail>> GetAllEmployeesAsync();
        Task<Employeedetail?> GetEmployeeByIdAsync(int id);
        Task<Employeedetail?> DeleteEmployeeAsync(Employeedetail employeeDetails);
        Task<Admindetail> ChangeHRPasswordAsync(Admindetail adminDetails, ChangePasswordRequestDTO newDetails);
        Task<Employeedetail> UpdateEmployeeDetailsAsync(Employeedetail? employeeDetails, UpdateEmployeeByHRRequestDTO newDetails);
        Task<List<Employeedetail>> GetAllEmployeesUnderManagerAsync(int id);
        Task<Departmentdetail?> GetDepartmentByIdAsync(string DepartmentId);
        Task<Managerdetail> AddManagerAsync(Employeedetail employee);
        Task<List<Employeedetail>> SearchEmployeeByNameAsync(string name);
        Task<Employeedetail> UpdatePersonalDetailsAsync(Employeedetail employeeDetails, UpdatePersonalDetailsDTO newDetails);
        Task<Employeedetail> ChangeEmployeePasswordAsync(Employeedetail employeeDetails, ChangePasswordRequestDTO newCredentials);
        Task<List<Timelinedetail>> GetTimeLineAsync(int EmpId);
        Task<AttendanceDetail> EmployeeTimeInAsync(int EmpId);
        Task<AttendanceDetail> EmployeeTimeOutAsync(AttendanceDetail attendanceRecord);
        Task<AttendanceDetail> SetDurationAsync(AttendanceDetail attendanceRecord);
        Task<AttendanceDetail?> GetAttendanceRecordAsync(int LastTimeInId);
        Task<List<AttendanceSummaryDTO>> GetAttendanceOfEmployeeAsync(int EmployeeId);
    }
}
