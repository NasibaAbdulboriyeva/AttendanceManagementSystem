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

            builder.HasIndex(s => s.EmployeeId)
                   .IsUnique();

            builder.Property(s => s.LimitInMinutes)
                .IsRequired();

            builder.Property(s => s.StartTime)
                .HasColumnType("time")
                .IsRequired();

            builder.Property(s => s.EndTime)
                .HasColumnType("time")
                .IsRequired();

            builder.Property(s => s.EmployementType)
                .IsRequired()
                .HasConversion<string>(); 

            builder.Property(s => s.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(s => s.ModifiedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()"); // Default qiymat berildi
        }
    }
}