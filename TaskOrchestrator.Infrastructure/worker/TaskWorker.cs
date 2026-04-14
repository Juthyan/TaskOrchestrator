namespace TaskOrchestrator.Infrastructure;

using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TaskOrchestrator.Application;
using TaskOrchestrator.Domain;

public class TaskWorker : BackgroundService
{
    private readonly Channel<OrchestratedTask> _channel;
    private readonly ITaskRepository _repository;

    public TaskWorker(Channel<OrchestratedTask> channel, ITaskRepository repository)
    {
        _channel = channel;
        _repository = repository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var task in _channel.Reader.ReadAllAsync(stoppingToken))
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
            }
           
        }
    }
}