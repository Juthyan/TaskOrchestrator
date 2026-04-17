namespace TaskOrchestrator.Application;

using TaskOrchestrator.Domain;
using Microsoft.Extensions.Logging;

public class EnqueueTaskCommandHandler(ITaskRepository taskRepository, TaskChannels channel, ILogger<EnqueueTaskCommandHandler> logger)
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly TaskChannels _channel = channel;

    private readonly ILogger<EnqueueTaskCommandHandler> _logger = logger;

    public async Task<Guid> HandleAsync(EnqueueTaskCommand command, CancellationToken ct = default)
    {
        var task = OrchestratedTask.CreateNew(command.Type);
        await _taskRepository.AddAsync(task, ct);
        _logger.LogInformation("Task {TaskId} of type {TaskType} created", task.Id, task.Type);

        if (command.Type == TaskType.Simulation)
        {
            await _channel.HighPriority.Writer.WriteAsync(task, ct);
            _logger.LogInformation("Task {TaskId} enqueued to {Priority} channel", task.Id, "HighPriority");

        }
        else
        {
            await _channel.LowPriority.Writer.WriteAsync(task, ct);
            _logger.LogInformation("Task {TaskId} enqueued to {Priority} channel", task.Id, "LowPriority");

        }

        return task.Id;
    }
}
