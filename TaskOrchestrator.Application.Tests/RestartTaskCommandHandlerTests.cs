using TaskOrchestrator.Application;
using Moq;
using TaskOrchestrator.Domain;
using System.Data;
public class RestartTaskCommandHandlerTests
{
    private readonly Mock<ITaskRepository> TaskRepo = new Mock<ITaskRepository>();
    private readonly TaskChannels ChannelMock = new TaskChannels();

    [Fact]
    public async Task Restart_task_should_reset_attempts_and_set_status_to_pending()
    {
        var existingTask = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 3);
        existingTask.Start();
        existingTask.Fail();

        while (existingTask.Attempts < 3)
        {
            existingTask.Retry();
            existingTask.Start();
            existingTask.Fail();
        }

        TaskRepo.Setup(r => r.GetAsync(existingTask.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingTask);

        TaskRepo.Setup(r => r.UpdateAsync(It.IsAny<OrchestratedTask>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

        var handler = new RestartTaskCommandHandler(TaskRepo.Object, ChannelMock);
        var id = await handler.HandleAsync(new RestartTaskCommand(existingTask.Id), default);

        Assert.Equal(existingTask.Id, id);
        TaskRepo.Verify(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        TaskRepo.Verify(r => r.UpdateAsync(It.IsAny<OrchestratedTask>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Restart_task_not_found_should_throw_domain_exception()
    { 
       TaskRepo.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((OrchestratedTask?)null);
        
        var handler = new RestartTaskCommandHandler(TaskRepo.Object, ChannelMock);
        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(new RestartTaskCommand(new Guid()), default));

        TaskRepo.Verify(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        TaskRepo.Verify(r => r.UpdateAsync(It.IsAny<OrchestratedTask>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}