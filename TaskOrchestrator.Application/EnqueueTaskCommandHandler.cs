namespace TaskOrchestrator.Application;

using TaskOrchestrator.Domain;


public class EnqueueTaskCommandHandler(ITaskRepository taskRepository, TaskChannels channel)
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly TaskChannels _channel = channel;

    public async Task<Guid>HandleAsync(EnqueueTaskCommand command, CancellationToken ct = default)
    {
        var task = OrchestratedTask.CreateNew(command.Type);
        await _taskRepository.AddAsync(task,ct);
        if (command.Type == TaskType.Simulation)
        {
            await _channel.HighPriority.Writer.WriteAsync(task, ct);
        }
        else
        {
             await _channel.LowPriority.Writer.WriteAsync(task, ct);
        }
       
        return task.Id;
    }
}
