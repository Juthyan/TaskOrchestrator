using Moq;
using TaskOrchestrator.Domain;

namespace TaskOrchestrator.Application.Tests;

public class EnqueueTaskCommandHandlerTest
{
    private readonly Mock<ITaskRepository> TaskRepo = new Mock<ITaskRepository>();
    private readonly TaskChannels ChannelMock = new TaskChannels();

    [Fact]
    public async Task Enqueue_task_should_return_a_guid_and_save_task()
    {
        TaskRepo.Setup(r => r.AddAsync(It.IsAny<OrchestratedTask>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);
        
        var handler = new EnqueueTaskCommandHandler(TaskRepo.Object, ChannelMock);
        var id = await handler.HandleAsync(new EnqueueTaskCommand(TaskType.Simulation), default);

        Assert.NotEqual(Guid.Empty, id);    
        TaskRepo.Verify(r => r.AddAsync(It.IsAny<OrchestratedTask>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
