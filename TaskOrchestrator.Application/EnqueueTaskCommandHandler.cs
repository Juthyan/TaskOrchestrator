using System.Data;

namespace TaskOrchestrator.Application;

using TaskOrchestrator.Domain;


public class EnqueueTaskCommandHandler(ITaskRepository taskRepository)
{
    private readonly ITaskRepository _taskRepository = taskRepository;


    public async Task<Guid>  HandleAsync(EnqueueTaskCommand command, CancellationToken ct = default)
    {
        var task = OrchestratedTask.CreateNew(command.Type);
        await _taskRepository.AddAsync(task,ct);
        return task.Id;
    }
}
