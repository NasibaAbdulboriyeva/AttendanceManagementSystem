using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagementSystem.Infrastructure.Persistence.EntityConfigurations
{
    public class AttendanceLogConfigurations : IEntityTypeConfiguration<AttendanceLog>
    {
        public void Configure(EntityTypeBuilder<AttendanceLog> builder)
        {
            builder.ToTable("AttendanceLogs");
            builder.HasKey(al => al.AttendenceLogId);

            builder.Property(al => al.EmployeeId)
                   .IsRequired();

            builder.Property(al => al.RecordedTime)
                   .IsRequired()
                   .HasColumnType("datetime2");

            builder.Property(al => al.Status)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(al => al.RawUsername)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(al => al.CreatedAt)
                   .IsRequired()
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(al => al.Employee)
                   .WithMany(e => e.AttendanceLogs)
                   .HasForeignKey(al => al.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
