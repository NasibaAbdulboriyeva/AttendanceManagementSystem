using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagementSystem.Infrastructure.Persistence.EntityConfigurations
{
    public class CurrentAttendanceLogConfiguration : IEntityTypeConfiguration<CurrentAttendanceLog>
    {
        public void Configure(EntityTypeBuilder<CurrentAttendanceLog> builder)
        {
            builder.ToTable("CurrentAttendanceLogs");
            builder.HasKey(c => c.CurrentAttendanceLogId);

            builder.Property(c => c.EmployeeId)
                    .IsRequired();

            builder.Property(c => c.LateArrivalMinutes)
                    .IsRequired();

            builder.Property(c => c.RemainingLateMinutes)
                    .IsRequired();

            builder.Property(c => c.Description)
                    .HasMaxLength(500)
                    .IsRequired(false); 

            builder.Property(c => c.IsJustified)//ogohlatirilganmi 
                    .IsRequired();

            builder.Property(c => c.CalculatedAt)
                    .IsRequired()
                    .HasColumnType("datetime2");

            builder.Property(c => c.FirstEntryTime)
                    .HasColumnType("time")
                    .IsRequired();

            builder.Property(c => c.LastLeavingTime)
                    .HasColumnType("time")
                    .IsRequired();
            builder.Property(c => c.WorkedHours)
                    .IsRequired();

            builder.Property(c => c.EntryDay)
                    .HasColumnType("date")
                    .IsRequired();

            builder.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETDATE()");

            builder.HasOne(cal => cal.Employee)
                    .WithMany(e => e.CurrentAttendanceLogs)
                    .HasForeignKey(cal => cal.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(cal => new { cal.EmployeeId, cal.EntryDay })
                     .IsUnique();
        }
    }
}