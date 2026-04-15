using TaskOrchestrator.Domain;

namespace TaskOrchestrator.Application;

public class CancelTaskCommandHandler(ITaskRepository taskRepository)
{
    private readonly ITaskRepository _taskRepository = taskRepository;

    public async Task<Guid> HandleAsync(CancelTaskCommand command, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetAsync(command.Id, ct);

        if (task is null)
        {
            throw new DomainException($"Task {command.Id} not found.");
        }

        task.Cancel();
        await _taskRepository.UpdateAsync(task, ct);
        return task.Id;
    }
}