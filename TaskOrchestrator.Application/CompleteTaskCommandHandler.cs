using TaskOrchestrator.Domain;

namespace TaskOrchestrator.Application;

public class CompleteTaskCommandHandler(ITaskRepository taskRepository)
{
    private readonly ITaskRepository _taskRepository = taskRepository;

    public async Task HandleAsync(CompleteTaskCommand command, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetAsync(command.Id, ct) ?? throw new DomainException($"Task {command.Id} not found.");

        task.Succeed();
        await _taskRepository.UpdateAsync(task, ct);
    }
}