using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagementSystem.Infrastructure.Persistence.EntityConfigurations
{
    public class DoorActivityLogConfigurations : IEntityTypeConfiguration<DoorActivityLog>
    {
        public void Configure(EntityTypeBuilder<DoorActivityLog> builder)
        {
            builder.ToTable("DoorActivityLogs");
            builder.HasKey(e => e.DoorActivityLogId);

            builder.Property(e => e.RecordedTime)
                   .IsRequired()
                   .HasColumnType("datetime2");
            builder.Property(e => e.Type)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(e => e.DeviceName)
                   .IsRequired()
                   .HasMaxLength(100);
        }
    }
}
