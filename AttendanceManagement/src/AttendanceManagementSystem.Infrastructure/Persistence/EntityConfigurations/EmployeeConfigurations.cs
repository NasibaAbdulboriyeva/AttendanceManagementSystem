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

            builder.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(150);

            builder.HasIndex(e => e.UserName)
                   .IsUnique();

            builder.Property(e => e.CardId)
                    .IsRequired(false);
            builder.Property(e => e.FingerprintId)
                   .IsRequired(false);
            builder.Property(e => e.CardNumber)
                    .IsRequired(false);

            builder.HasIndex(e => e.CardNumber)
                   .IsUnique(false);
            builder.HasIndex(e => e.FingerprintNumber)
                  .IsUnique(false);

            builder.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
            builder.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETDATE()");

            builder.Property(e => e.ModifiedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETDATE()");

            builder.HasMany(e => e.AttendanceLogs)
                    .WithOne(al => al.Employee)
                    .HasForeignKey(al => al.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.CurrentAttendanceLogs)
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