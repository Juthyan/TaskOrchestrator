namespace TaskOrchestrator.Infrastructure;

using Microsoft.Extensions.Hosting;
using TaskOrchestrator.Application;
using TaskOrchestrator.Domain;

public class TaskWorker : BackgroundService
{
    private readonly TaskChannels _channels;
    private readonly ITaskRepository _repository;

    public TaskWorker(TaskChannels channels, ITaskRepository repository)
    {
        _channels = channels;
        _repository = repository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       var workers = Enumerable.Range(0, 4)
        .Select(_ => RunWorkerAsync(stoppingToken));
    
        await Task.WhenAll(workers);
    }
    private async Task RunWorkerAsync(CancellationToken stoppingToken)
    {
         while (!stoppingToken.IsCancellationRequested)
        {
            if (_channels.HighPriority.Reader.TryRead(out var task))
            {
                await ProcessAsync(task, stoppingToken);
            }
            else if (_channels.LowPriority.Reader.TryRead(out task))
            {
                await ProcessAsync(task, stoppingToken);
            }
            else
            {
                await Task.WhenAny(
                    _channels.HighPriority.Reader.WaitToReadAsync(stoppingToken).AsTask(),
                    _channels.LowPriority.Reader.WaitToReadAsync(stoppingToken).AsTask()
                );
            }
        }
    }

    private async Task ProcessAsync(OrchestratedTask task, CancellationToken stoppingToken)
    {
        try
        {
            task.Start();
            await _repository.UpdateAsync(task, stoppingToken);
            await Task.Delay(1000, stoppingToken);
            task.Succeed();
            await _repository.UpdateAsync(task, stoppingToken);
        }
        catch (Exception)
        {
            task.Fail();
            await _repository.UpdateAsync(task, stoppingToken);

            if (task.Attempts < task.MaxAttempts)
            {
                task.Retry();
                await _repository.UpdateAsync(task, stoppingToken);

                if (task.Type == TaskType.Simulation)
                    await _channels.HighPriority.Writer.WriteAsync(task, stoppingToken);
                else
                    await _channels.LowPriority.Writer.WriteAsync(task, stoppingToken);
            }
        }
    }
}