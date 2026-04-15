namespace TaskOrchestrator.Application;

using TaskOrchestrator.Domain;


public class RestartTaskCommandHandler(ITaskRepository taskRepository, TaskChannels channel)
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly TaskChannels _channel = channel;

    public async Task<Guid>HandleAsync(RestartTaskCommand command, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetAsync(command.Id, ct);

        if (task is null)
        {
            throw new DomainException($"Task {command.Id} not found.");
        }
            
        task.ReStart();
        await _taskRepository.UpdateAsync(task, ct);
        if (task.Type == TaskType.Simulation)
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
