using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using AttendanceManagementSystem.Domain.Entities;

namespace AttendanceManagementSystem.Infrastructure.Persistence.EntityConfigurations
{
    public class EmployeeScheduleConfigurations : IEntityTypeConfiguration<EmployeeSchedule>
    {
        public void Configure(EntityTypeBuilder<EmployeeSchedule> builder)
        {
            builder.ToTable("EmployeeSchedules");
            builder.HasKey(s => s.EmployeeScheduleId);

            builder.HasOne(s => s.Employee)
                .WithOne(e => e.EmployeeSchedule)
                .HasForeignKey<EmployeeSchedule>(s => s.EmployeeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(s => s.StartTime)
                .HasColumnType("time")
                .IsRequired();
            builder.Property(s => s.LimitInMinutes)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                   .IsRequired()
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETDATE()");

        }
    }
}
