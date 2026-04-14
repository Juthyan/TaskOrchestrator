namespace TaskOrchestrator.Application;

using System.Threading.Channels;
using TaskOrchestrator.Domain;


public class EnqueueTaskCommandHandler(ITaskRepository taskRepository, Channel<OrchestratedTask> channel)
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly Channel<OrchestratedTask> _channel = channel;

    public async Task<Guid>  HandleAsync(EnqueueTaskCommand command, CancellationToken ct = default)
    {
        var task = OrchestratedTask.CreateNew(command.Type);
        await _taskRepository.AddAsync(task,ct);
        await _channel.Writer.WriteAsync(task, ct);
        return task.Id;
    }
}
