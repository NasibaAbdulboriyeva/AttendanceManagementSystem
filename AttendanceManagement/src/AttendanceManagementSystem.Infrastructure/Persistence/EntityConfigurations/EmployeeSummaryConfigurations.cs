using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendanceManagementSystem.Infrastructure.Persistence.EntityConfigurations
{
    public class EmployeeSummaryConfigurations : IEntityTypeConfiguration<EmployeeSummary>
    {
        public void Configure(EntityTypeBuilder<EmployeeSummary> builder)
        {
            builder.ToTable("EmployeeSummaries");
            builder.HasKey(e => e.EmployeeSummaryId);
            builder.Property(e => e.EmployeeId)
                   .IsRequired();

            builder.Property(e => e.Month)
                   .IsRequired()
                   .HasColumnName("SummaryMonth")
                   .HasConversion<string>();

            builder.Property(e => e.LateArrivalMinutes)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(e => e.CalculatedAt)
                   .IsRequired()
                   .HasColumnType("datetime2");

            builder.Property(e => e.CreatedAt)
                   .IsRequired()
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETDATE()");


            builder.HasOne(e => e.Employee)
                   .WithMany()
                   .HasForeignKey(e => e.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
