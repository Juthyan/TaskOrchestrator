using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskOrchestrator.Domain;

namespace TaskOrchestrator.Application.Tests;

public class EnqueueTaskCommandHandlerTest
{
    private readonly Mock<ITaskRepository> TaskRepo = new Mock<ITaskRepository>();
    private readonly TaskChannels ChannelMock = new TaskChannels();
    private readonly TaskMetrics Metrics = new TaskMetrics(
        new ServiceCollection().AddMetrics().BuildServiceProvider().GetRequiredService<IMeterFactory>());

    [Fact]
    public async Task Enqueue_task_should_return_a_guid_and_save_task()
    {
        TaskRepo.Setup(r => r.AddAsync(It.IsAny<OrchestratedTask>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
        
        var handler = new EnqueueTaskCommandHandler(
            TaskRepo.Object,
            ChannelMock,
            NullLogger<EnqueueTaskCommandHandler>.Instance,
            Metrics,
            new TaskActivitySource());
        var id = await handler.HandleAsync(new EnqueueTaskCommand(TaskType.Simulation), default);

        Assert.NotEqual(Guid.Empty, id);    
        TaskRepo.Verify(r => r.AddAsync(It.IsAny<OrchestratedTask>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
