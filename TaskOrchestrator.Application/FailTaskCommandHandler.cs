using Microsoft.Extensions.Logging;
using TaskOrchestrator.Domain;

namespace TaskOrchestrator.Application;

public class FailTaskCommandHandler(ITaskRepository taskRepository, ILogger<FailTaskCommandHandler> logger)
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly ILogger<FailTaskCommandHandler> _logger = logger;


    public async Task HandleAsync(FailTaskCommand command, CancellationToken ct = default)
    {

        var task = await _taskRepository.GetAsync(command.Id, ct) ?? throw new DomainException($"Task {command.Id} not found.");
        task.Fail();

        if (task.Attempts < task.MaxAttempts)
        {
            task.Retry(); // republish
        }

        await _taskRepository.UpdateAsync(task, ct);
        _logger.LogWarning("Task {TaskId} failed: {Reason}", command.Id, command.Reason ?? "no reason provided");    }
}