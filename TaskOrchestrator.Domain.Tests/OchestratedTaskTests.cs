using TaskOrchestrator.Domain;

namespace TaskOrchestrator.Domain.Tests;

public class OrchestratedTaskTests
{
    [Fact]
    public void New_task_starts_in_pending_with_zero_attempts()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 3);

        Assert.Equal(TaskState.Pending, task.Status);
        Assert.Equal(0, task.Attempts);
        Assert.Equal(TaskType.Simulation, task.Type);
    }

    [Fact]
    public void Can_start_a_pending_task()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Monitoring, maxAttempts: 3);

        task.Running();

        Assert.Equal(TaskState.Running, task.Status);
    }

    [Fact]
    public void Cannot_start_a_non_pending_task()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 3);
        task.Running();
        task.Succeed();

        Assert.Throws<DomainException>(() => task.Running());
    }

    [Fact]
    public void Retry_from_failed_goes_back_to_pending_until_max_attempts()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 2);

        task.Running();
        task.Failed();

        task.Retry();

        Assert.Equal(TaskState.Pending, task.Status);
        Assert.Equal(1, task.Attempts);

        task.Running();
        task.Failed();

        Assert.Throws<DomainException>(() => task.Retry());
    }
}
