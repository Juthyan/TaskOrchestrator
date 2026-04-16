
using Moq;
using TaskOrchestrator.Application;
using TaskOrchestrator.Domain;

public class CancelTaskCommandHandlerTests
{
    private readonly Mock<ITaskRepository> TaskRepo = new Mock<ITaskRepository>();

    [Fact]
    public async Task Cancel_task_should_set_status_to_cancelled()
    {
        var existingTask = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 3);

        TaskRepo.Setup(r => r.GetAsync(existingTask.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(existingTask);

        var handler = new CancelTaskCommandHandler(TaskRepo.Object);
        var id = await handler.HandleAsync(new CancelTaskCommand(existingTask.Id), default);

        Assert.Equal(existingTask.Id, id);

        TaskRepo.Verify(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        TaskRepo.Verify(r => r.UpdateAsync(It.IsAny<OrchestratedTask>(), It.IsAny<CancellationToken>()), Times.Once);

    }

    [Fact]
     public async Task Restart_task_not_found_should_throw_domain_exception()
    {
        
       TaskRepo.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((OrchestratedTask?)null);
        
        var handler = new CancelTaskCommandHandler(TaskRepo.Object);
        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(new CancelTaskCommand(new Guid()), default));

        TaskRepo.Verify(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        TaskRepo.Verify(r => r.UpdateAsync(It.IsAny<OrchestratedTask>(), It.IsAny<CancellationToken>()), Times.Never);
    }




}