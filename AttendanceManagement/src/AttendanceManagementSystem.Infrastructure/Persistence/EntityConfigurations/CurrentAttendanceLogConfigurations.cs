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
              
                builder.Property(c => c.EntryDay)
                       .HasColumnType("date")
                       .IsRequired();

                builder.Property(c => c.FirstEntryTime)
                       .HasColumnType("time") 
                       .IsRequired();

            
                builder.Property(c => c.LateArrivalMinutes)
                       .IsRequired();

              
                builder.Property(c => c.RemainingLateMinutes)
                       .IsRequired();

               
                builder.Property(c => c.IsJustified)
                       .IsRequired();

              
                builder.Property(c => c.CalculatedAt)
                       .IsRequired();

            builder.Property(e => e.CreatedAt)
                   .IsRequired()
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.Description)
                  .HasMaxLength(500) 
                  .IsRequired(false);

            builder.HasOne(al => al.Employee)
                   .WithMany(e => e.CurrentAttendanceLogs)
                   .HasForeignKey(al => al.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 1 employee uchun 1 kunda faqat bitta log yozilishi kerak bo'ganiga unique index qoyamiza 

            builder.HasIndex(cal => new { cal.EmployeeId, cal.EntryDay })
                    .IsUnique();
        }
    }
    }
