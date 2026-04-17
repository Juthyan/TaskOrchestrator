namespace TaskOrchestrator.Application;

using TaskOrchestrator.Domain;
using Microsoft.Extensions.Logging;

public class EnqueueTaskCommandHandler(ITaskRepository taskRepository, TaskChannels channel, ILogger<EnqueueTaskCommandHandler> logger, TaskMetrics metrics, TaskActivitySource taskActivitySource)
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly TaskChannels _channel = channel;
    private readonly ILogger<EnqueueTaskCommandHandler> _logger = logger;
    private readonly TaskMetrics _metrics = metrics;
    private readonly TaskActivitySource _taskActivitySource = taskActivitySource;

    public async Task<Guid> HandleAsync(EnqueueTaskCommand command, CancellationToken ct = default)
    {
        var task = OrchestratedTask.CreateNew(command.Type);
        using var activity = _taskActivitySource.StartEnqueueTask(task.Id.ToString(), task.Type.ToString());

        await _taskRepository.AddAsync(task, ct);
        _logger.LogInformation("Task {TaskId} of type {TaskType} created", task.Id, task.Type);
        _metrics.TaskEnqueued(task.Type.ToString());

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
