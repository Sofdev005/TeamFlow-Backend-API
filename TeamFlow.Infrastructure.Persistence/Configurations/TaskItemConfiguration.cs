using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamFlow.Domain.Entities;

namespace TeamFlow.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(t => t.Order)
            .IsRequired();

        // Index for rapid column rendering and board queries
        builder.HasIndex(t => new { t.ProjectId, t.BoardColumnId, t.Order });
    }
}