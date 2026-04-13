using System.Collections.Concurrent;
using TaskOrchestrator.Application;
using TaskOrchestrator.Domain;

namespace TaskOrchestrator.Infrastructure;

public class InMemoryTaskRepository: ITaskRepository
{

    private ConcurrentDictionary<Guid, OrchestratedTask> _concurrentDictionary = new ();

    public async Task<OrchestratedTask?> GetAsync(Guid id, CancellationToken ct = default)
    {
        _concurrentDictionary.TryGetValue(id, out var task);
        return task;
    }
    
    public async Task<IReadOnlyList<OrchestratedTask>> GetAllAsync(CancellationToken ct = default)
    {

       return _concurrentDictionary.Values.ToList();
    }

    public async Task AddAsync(OrchestratedTask task, CancellationToken ct = default)
    {
        _concurrentDictionary.TryAdd(task.Id, task);
    }

    public async Task UpdateAsync(OrchestratedTask task, CancellationToken ct = default)
    {
        _concurrentDictionary.AddOrUpdate(task.Id, task, (key, existing) => task);

    }
}
