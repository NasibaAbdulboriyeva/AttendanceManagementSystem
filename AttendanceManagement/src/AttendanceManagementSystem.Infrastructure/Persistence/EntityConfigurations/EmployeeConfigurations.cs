using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagementSystem.Infrastructure.Persistence.EntityConfigurations
{
    public class EmployeeConfigurations : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");
            builder.HasKey(e => e.EmployeeId);

            builder.Property(e => e.FullName)
                   .IsRequired()
                   .HasMaxLength(250);

            builder.Property(e => e.Code)
                   .HasMaxLength(50);

            builder.Property(e => e.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);

            builder.Property(e => e.CreatedAt)
                   .IsRequired()
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETDATE()");

            builder.HasMany(e => e.AttendanceLogs)
                   .WithOne(al => al.Employee)
                   .HasForeignKey(al => al.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.EmployeeSummaries)
                   .WithOne(es => es.Employee)
                   .HasForeignKey(es => es.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.EmployeeSchedule)
                   .WithOne(es => es.Employee)
                   .HasForeignKey<EmployeeSchedule>(es => es.EmployeeId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}