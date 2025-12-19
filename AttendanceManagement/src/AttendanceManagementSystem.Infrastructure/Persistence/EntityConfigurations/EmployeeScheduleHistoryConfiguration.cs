using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace AttendanceManagementSystem.Infrastructure.Persistence.EntityConfigurations
{
    public class EmployeeScheduleHistoryConfiguration : IEntityTypeConfiguration<EmployeeScheduleHistory>
    {
        public void Configure(EntityTypeBuilder<EmployeeScheduleHistory> builder)
        {
            builder.ToTable("EmployeeScheduleHistories");
            builder.HasKey(s => s.EmployeeScheduleHistoryId);

            builder.HasOne(sh => sh.Employee)
                .WithMany(e => e.EmployeeScheduleHistories)
                .HasForeignKey(sh => sh.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

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

            builder.Property(s => s.ValidTo)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()");

            builder.Property(s => s.ValidFrom)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("GETDATE()"); 
        }
    }
}

