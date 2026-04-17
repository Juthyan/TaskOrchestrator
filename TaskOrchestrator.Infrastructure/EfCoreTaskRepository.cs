namespace TaskOrchestrator.Infrastructure;


using Microsoft.EntityFrameworkCore;
using TaskOrchestrator.Application;
using TaskOrchestrator.Domain;

public class EfCoreTaskRepository(TaskOrchestratorDbContext context) : ITaskRepository
{
    private readonly TaskOrchestratorDbContext _context = context;
    public async Task AddAsync(OrchestratedTask task, CancellationToken ct = default)
    {
        await _context.Tasks.AddAsync(task, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<OrchestratedTask>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Tasks.ToListAsync(ct);
    }

    public async Task<OrchestratedTask?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Tasks.FindAsync(new object[] { id }, ct);
    }

    public async Task UpdateAsync(OrchestratedTask task, CancellationToken ct = default)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync(ct);
    }
}