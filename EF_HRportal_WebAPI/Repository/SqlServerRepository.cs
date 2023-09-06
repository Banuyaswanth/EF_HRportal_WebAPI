using EF_HRportal_WebAPI.Models.Domain;
using EF_HRportal_WebAPI.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EF_HRportal_WebAPI.Repository
{
    public class SqlServerRepository: IRepository
    {
        private readonly EfhrportalContext dbContext;
        public SqlServerRepository(EfhrportalContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Employeedetail> AddEmployeeAsync(Employeedetail employeeDetail)
        {
            await dbContext.Employeedetails.AddAsync(employeeDetail);
            await dbContext.SaveChangesAsync();
            return employeeDetail;
        }

        public async Task<Managerdetail> AddManagerAsync(Employeedetail employee)
        {
            var newManager = new Managerdetail
            {
                ManagerId = employee.Id,
                Department = employee.Department ?? "Exec"
            };
            await dbContext.Managerdetails.AddAsync(newManager);
            await dbContext.SaveChangesAsync();
            return newManager;
        }

        public async Task<Timelinedetail> AddTimeLineAsync(Timelinedetail timelineDetail)
        {
            await dbContext.Timelinedetails.AddAsync(timelineDetail);
            await dbContext.SaveChangesAsync();
            return timelineDetail;
        }

        public async Task<Employeedetail> ChangeEmployeePasswordAsync(Employeedetail employeeDetails, ChangePasswordRequestDto newCredentials)
        {
            employeeDetails.Password = newCredentials.NewPassword;
            dbContext.Entry(employeeDetails).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return employeeDetails;
        }

        public async Task<Admindetail> ChangeHRPasswordAsync(Admindetail adminDetails, ChangePasswordRequestDto newDetails)
        {
            adminDetails.Password = newDetails.NewPassword;
            dbContext.Entry(adminDetails).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return adminDetails;
        }

        public async Task<Employeedetail> DeleteEmployeeAsync(Employeedetail employeeDetails)
        {
            dbContext.Employeedetails.Remove(employeeDetails);
            await dbContext.SaveChangesAsync();
            return employeeDetails;
        }

        public async Task<AttendanceDetail?> EmployeeTimeInAsync(int EmpId)
        {
            var newTimeIn = new AttendanceDetail
            {
                EmpId = EmpId,
                DateOfAttendance = DateTime.Now.Date,
                TimeIn = DateTime.Now,
            };
            await dbContext.AttendanceDetails.AddAsync(newTimeIn);
            await dbContext.SaveChangesAsync();
            return newTimeIn;
        }

        public async Task<AttendanceDetail> EmployeeTimeOutAsync(AttendanceDetail attendanceRecord)
        {
            attendanceRecord.TimeOut = DateTime.Now;
            await dbContext.SaveChangesAsync();
            var updatedAttendanceDetail = await SetDurationAsync(attendanceRecord);
            return updatedAttendanceDetail;
        }

        public async Task<List<Employeedetail>> GetAllEmployeesAsync()
        {
            return await dbContext.Employeedetails.ToListAsync();
        }

        public async Task<List<Employeedetail>> GetAllEmployeesUnderManagerAsync(int id)
        {
            var employeesList = from employee in dbContext.Employeedetails
                                where employee.ManagerId == id
                                select employee;
            return await employeesList.ToListAsync();
        }

        public async Task<List<AttendanceSummaryDto>> GetAttendanceOfEmployeeAsync(int EmployeeId)
        {
            var attendanceList = from record in dbContext.AttendanceDetails
                                 where record.EmpId == EmployeeId && record.TimeOut != null
                                 group record by record.DateOfAttendance into a
                                 orderby a.Key descending
                                 select new
                                 {
                                     Date = a.Key,
                                     TotalDuration = a.Sum(x => x.Duration)
                                 };
            var AttendanceList = await attendanceList.ToListAsync();
            var FinalAttendanceList = new List<AttendanceSummaryDto>();
            foreach (var attendance in AttendanceList)
            {
                string duration = attendance.TotalDuration / 60 + "hrs : " + attendance.TotalDuration % 60 + "min";
                var formattedAttendance = new AttendanceSummaryDto
                {
                    Date = attendance.Date.ToShortDateString(),
                    TotalDuration = duration
                };
                FinalAttendanceList.Add(formattedAttendance);
            }
            return FinalAttendanceList;
        }

        public async Task<AttendanceDetail?> GetAttendanceRecordAsync(int EmpId)
        {
            return await dbContext.AttendanceDetails.FirstOrDefaultAsync(x => x.EmpId == EmpId && x.TimeOut == null);
        }

        public async Task<Departmentdetail?> GetDepartmentByIdAsync(string DepartmentId)
        {
            return await dbContext.Departmentdetails.FindAsync(DepartmentId);
        }

        public async Task<Employeedetail?> GetEmployeeByEmailAsync(string email)
        {
            return await dbContext.Employeedetails.Include(x=>x.Manager).FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<Employeedetail?> GetEmployeeByIdAsync(int id)
        {
            return await dbContext.Employeedetails.Include(x=>x.Manager).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Admindetail?> GetHRbyEmailAsync(string email)
        {
            return await dbContext.Admindetails.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<Admindetail?> GetHRByIdAsync(int id)
        {
            return await dbContext.Admindetails.FirstOrDefaultAsync(x => x.EmpId == id);
        }

        public async Task<Managerdetail?> GetManagerByIdAsync(int id)
        {
            return await dbContext.Managerdetails.FirstOrDefaultAsync(x => x.ManagerId == id);
        }

        public async Task<List<Timelinedetail>> GetTimeLineAsync(int EmpId)
        {
            var timelineDetails = from action in dbContext.Timelinedetails
                                  where action.EmpId == EmpId
                                  orderby action.DateOfAction descending
                                  select action;
            return await timelineDetails.ToListAsync();
        }

        public async Task<List<Employeedetail>> SearchEmployeeByNameAsync(string name)
        {
            var employeesList = from employee in dbContext.Employeedetails
                                where employee.Name.Contains(name)
                                select employee;
            return await employeesList.ToListAsync();
        }

        public async Task<AttendanceDetail> SetDurationAsync(AttendanceDetail attendanceRecord)
        {
            TimeSpan? duration = attendanceRecord.TimeOut - attendanceRecord.TimeIn;
            attendanceRecord.Duration = (int)duration.Value.TotalMinutes;
            dbContext.Entry(attendanceRecord).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return attendanceRecord;
        }

        public async Task<Employeedetail> UpdateEmployeeDetailsAsync(Employeedetail employeeDetails, UpdateEmployeeByHRRequestDto newDetails)
        {
            employeeDetails.Name = newDetails.Name;
            employeeDetails.Phone = newDetails.Phone;
            employeeDetails.Salary = newDetails.Salary;
            employeeDetails.Department = newDetails.Department;
            employeeDetails.ManagerId = newDetails.ManagerId;
            dbContext.Entry(employeeDetails).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return employeeDetails;
        }

        public async Task<Employeedetail> UpdatePersonalDetailsAsync( Employeedetail employeeDetails, UpdatePersonalDetailsDto newDetails)
        {
            employeeDetails.Name = newDetails.Name;
            employeeDetails.Phone = newDetails.Phone;
            dbContext.Entry(employeeDetails).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return employeeDetails;

        }
    }
}
