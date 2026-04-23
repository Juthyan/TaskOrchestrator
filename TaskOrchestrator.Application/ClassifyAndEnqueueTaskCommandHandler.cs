using Microsoft.Extensions.Logging;
using TaskOrchestrator.Domain;

namespace TaskOrchestrator.Application;

public class ClassifyAndEnqueueTaskCommandHandler(ITaskClassifier taskClassifier, ITaskRepository taskRepository, TaskChannels channel, ILogger<ClassifyAndEnqueueTaskCommandHandler> logger, TaskMetrics metrics, TaskActivitySource taskActivitySource)
{
    private readonly ITaskClassifier _taskClassifier = taskClassifier;
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly TaskChannels _channel = channel;
    private readonly ILogger<ClassifyAndEnqueueTaskCommandHandler> _logger = logger;
    private readonly TaskMetrics _metrics = metrics;
    private readonly TaskActivitySource _taskActivitySource = taskActivitySource;


    public async Task<Guid> HandleAsync(ClassifyAndEnqueueTaskCommand command, CancellationToken ct = default)
        {
            var classifiedType =  await _taskClassifier.ClassifyAsync(command.Description, ct);
            var taskType =  Enum.Parse<TaskType>(classifiedType, ignoreCase: true);

            var task = OrchestratedTask.CreateNew(taskType);
            using var activity = _taskActivitySource.StartEnqueueTask(task.Id.ToString(), task.Type.ToString());

            await _taskRepository.AddAsync(task, ct);
            _logger.LogInformation("Task {TaskId} of type {TaskType} created", task.Id, task.Type);
            _metrics.TaskEnqueued(task.Type.ToString());

            if (task.Type == TaskType.Simulation)
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
