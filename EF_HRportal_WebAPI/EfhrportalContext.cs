using System;
using System.Collections.Generic;
using EF_HRportal_WebAPI.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace EF_HRportal_WebAPI;

public partial class EfhrportalContext : DbContext
{
    public EfhrportalContext()
    {
    }

    public EfhrportalContext(DbContextOptions<EfhrportalContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admindetail> Admindetails { get; set; }

    public virtual DbSet<AttendanceDetail> AttendanceDetails { get; set; }

    public virtual DbSet<Departmentdetail> Departmentdetails { get; set; }

    public virtual DbSet<Employeedetail> Employeedetails { get; set; }

    public virtual DbSet<Managerdetail> Managerdetails { get; set; }

    public virtual DbSet<Timelinedetail> Timelinedetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        optionsBuilder.UseSqlServer("Data Source=DESKTOP-7R5H7B7\\SQLEXPRESS;Initial Catalog=EFHRportal;Integrated Security=True;TrustServerCertificate=True");
        optionsBuilder.UseLazyLoadingProxies();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admindetail>(entity =>
        {
            entity.ToTable("admindetails");

            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Id, "IX_admindetails").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.EmpId).HasColumnName("empId");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("password");

            entity.HasOne(d => d.EmailNavigation).WithMany()
                .HasPrincipalKey(p => p.Email)
                .HasForeignKey(d => d.Email)
                .HasConstraintName("FK_admindetails_employeedetails");

            entity.HasOne(d => d.Emp).WithMany()
                .HasForeignKey(d => d.EmpId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_admindetails_employeedetails1");
        });

        modelBuilder.Entity<AttendanceDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__attendan__3213E83FEF876420");

            entity.ToTable("attendanceDetails");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateOfAttendance)
                .HasColumnType("date")
                .HasColumnName("dateOfAttendance");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.EmpId).HasColumnName("empId");
            entity.Property(e => e.TimeIn)
                .HasColumnType("datetime")
                .HasColumnName("timeIn");
            entity.Property(e => e.TimeOut)
                .HasColumnType("datetime")
                .HasColumnName("timeOut");

            entity.HasOne(d => d.Emp).WithMany(p => p.AttendanceDetails)
                .HasForeignKey(d => d.EmpId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__attendanc__empId__29221CFB");
        });

        modelBuilder.Entity<Departmentdetail>(entity =>
        {
            entity.HasKey(e => e.DepartmentId);

            entity.ToTable("departmentdetails");

            entity.Property(e => e.DepartmentId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("departmentId");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("departmentName");
        });

        modelBuilder.Entity<Employeedetail>(entity =>
        {
            entity.ToTable("employeedetails");

            entity.HasIndex(e => e.Email, "IX_employeedetails").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateOfJoining).HasColumnType("datetime");
            entity.Property(e => e.Department)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("department");
            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.ManagerId).HasColumnName("managerId");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Salary).HasColumnName("salary");

            entity.HasOne(d => d.DepartmentNavigation).WithMany(p => p.Employeedetails)
                .HasForeignKey(d => d.Department)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_employeedetails_departmentdetails");

            entity.HasOne(d => d.Manager).WithMany(p => p.InverseManager)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_employeedetails_employeedetails");
        });

        modelBuilder.Entity<Managerdetail>(entity =>
        {
            entity.HasKey(e => e.ManagerId);

            entity.ToTable("managerdetails");

            entity.Property(e => e.ManagerId)
                .ValueGeneratedNever()
                .HasColumnName("managerId");
            entity.Property(e => e.Department)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("department");

            entity.HasOne(d => d.DepartmentNavigation).WithMany(p => p.Managerdetails)
                .HasForeignKey(d => d.Department)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_managerdetails_departmentdetails");

            entity.HasOne(d => d.Manager).WithOne(p => p.Managerdetail)
                .HasForeignKey<Managerdetail>(d => d.ManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_managerdetails_employeedetails");
        });

        modelBuilder.Entity<Timelinedetail>(entity =>
        {
            entity.ToTable("timelinedetails");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("action");
            entity.Property(e => e.DateOfAction)
                .HasColumnType("datetime")
                .HasColumnName("dateOfAction");
            entity.Property(e => e.EmpId).HasColumnName("empId");

            entity.HasOne(d => d.Emp).WithMany(p => p.Timelinedetails)
                .HasForeignKey(d => d.EmpId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_timelinedetails_employeedetails");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
