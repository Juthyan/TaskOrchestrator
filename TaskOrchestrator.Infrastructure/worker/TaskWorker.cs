namespace TaskOrchestrator.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskOrchestrator.Application;
using TaskOrchestrator.Domain;

public class TaskWorker : BackgroundService
{
    private readonly TaskChannels _channels;
    private readonly IServiceScopeFactory _scopeFactory;

    public TaskWorker(TaskChannels channels, IServiceScopeFactory scopeFactory)
    {
        _channels = channels;
        _scopeFactory = scopeFactory;
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
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

        try
        {
            task.Start();
            await repository.UpdateAsync(task, stoppingToken);
            await Task.Delay(1000, stoppingToken);
            task.Succeed();
            await repository.UpdateAsync(task, stoppingToken);
        }
        catch (Exception)
        {
            task.Fail();
            await repository.UpdateAsync(task, stoppingToken);

            if (task.Attempts < task.MaxAttempts)
            {
                task.Retry();
                await repository.UpdateAsync(task, stoppingToken);

                if (task.Type == TaskType.Simulation)
                    await _channels.HighPriority.Writer.WriteAsync(task, stoppingToken);
                else
                    await _channels.LowPriority.Writer.WriteAsync(task, stoppingToken);
            }
        }
    }
}