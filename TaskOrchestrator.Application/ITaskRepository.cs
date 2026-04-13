using TaskOrchestrator.Domain;

namespace TaskOrchestrator.Application;

public interface ITaskRepository
{
    Task<OrchestratedTask?> GetAsync(Guid id, CancellationToken ct = default);
    
    Task AddAsync(OrchestratedTask task, CancellationToken ct = default);
    
    Task UpdateAsync(OrchestratedTask task, CancellationToken ct = default);
    
    Task<IReadOnlyList<OrchestratedTask>> GetAllAsync(CancellationToken ct = default);
}