namespace TaskOrchestrator.Infrastructure;


using Microsoft.EntityFrameworkCore;
using TaskOrchestrator.Domain;

public class TaskOrchestratorDbContext(DbContextOptions<TaskOrchestratorDbContext> options) : DbContext(options)
{
    public DbSet<OrchestratedTask> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);

        modelBuilder.Entity<OrchestratedTask>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Status)
                .HasConversion<string>();

            entity.Property(t => t.Type)
                .HasConversion<string>();
        });
    }
}
