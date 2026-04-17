namespace TaskOrchestrator.Infrastructure;

using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskOrchestrator.Application;
using TaskOrchestrator.Domain;

public class TaskWorker : BackgroundService
{
    private readonly TaskChannels _channels;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaskWorker> _logger;
    private readonly TaskMetrics _metrics;


    public TaskWorker(TaskChannels channels, IServiceScopeFactory scopeFactory, ILogger<TaskWorker> logger, TaskMetrics metrics)
    {
        _channels = channels;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _metrics = metrics;
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
        var stopwatch = Stopwatch.StartNew();

        try
        {
            task.Start();
            await repository.UpdateAsync(task, stoppingToken);
            _logger.LogInformation("Task {TaskId} of type {TaskType} ran", task.Id, task.Type);

            await Task.Delay(1000, stoppingToken);
            task.Succeed();
            await repository.UpdateAsync(task, stoppingToken);
            _logger.LogInformation("Task {TaskId} of type {TaskType} succeeded", task.Id, task.Type);
            _metrics.TaskCompleted("Succeeded");
        }
        catch (Exception)
        {
            task.Fail();
            await repository.UpdateAsync(task, stoppingToken);
            _logger.LogWarning("Task {TaskId} failed, retrying attempt {Attempts}/{MaxAttempts}", task.Id, task.Attempts, task.MaxAttempts);
            _metrics.TaskCompleted("Failed");

            if (task.Attempts < task.MaxAttempts)
            {
                task.Retry();
                await repository.UpdateAsync(task, stoppingToken);
                _logger.LogWarning("Task {TaskId} of type {TaskType} retryed, attempt {Attempts}/{MaxAttempts}", task.Id, task.Type, task.Attempts, task.MaxAttempts);


                var jitter = Random.Shared.NextDouble() * 1000;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, task.Attempts)) + TimeSpan.FromMilliseconds(jitter);
                await Task.Delay(delay, stoppingToken);

                if (task.Type == TaskType.Simulation)
                    await _channels.HighPriority.Writer.WriteAsync(task, stoppingToken);
                else
                    await _channels.LowPriority.Writer.WriteAsync(task, stoppingToken);
            }
            else
            {
                _logger.LogError("Task {TaskId} exhausted all {MaxAttempts} attempts", task.Id, task.MaxAttempts);
            }
        }
        stopwatch.Stop();
        _metrics.RecordDuration(stopwatch.Elapsed.TotalMilliseconds);
    }
}