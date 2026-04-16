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

        task.Start();

        Assert.Equal(TaskState.Running, task.Status);
    }

    [Fact]
    public void Cannot_start_a_non_pending_task()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 3);
        task.Start();
        task.Succeed();

        Assert.Throws<DomainException>(() => task.Start());
    }

    [Fact]
    public void Retry_from_failed_goes_back_to_pending_until_max_attempts()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 2);

        task.Start();
        task.Fail();

        task.Retry();

        Assert.Equal(TaskState.Pending, task.Status);
        Assert.Equal(1, task.Attempts);

        task.Start();
        task.Fail();

        Assert.Throws<DomainException>(() => task.Retry());
    }

    [Fact]
    public void Restart_must_put_the_attempts_to_zero()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 3);

        task.Start();
        task.Fail();

        while (task.Attempts < 3)
        {
            task.Retry();
            task.Start();
            task.Fail();
        }

        Assert.Equal(TaskState.Failed, task.Status);
        Assert.Equal(3, task.Attempts);

        task.ReStart();

        Assert.Equal(TaskState.Pending, task.Status);
        Assert.Equal(0, task.Attempts);
    }

    [Fact]
    public void Imposstible_to_restart_when_the_attempts_is_less_than_3()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 3);

        task.Start();
        task.Fail();

        while (task.Attempts < 2)
        {
            task.Retry();
            task.Start();
            task.Fail();
        }

        Assert.Equal(TaskState.Failed, task.Status);
        Assert.Equal(2, task.Attempts);
        Assert.Throws<DomainException>(() =>  task.ReStart());
    }

     [Fact]
    public void Imposstible_to_restart_a_task_not_failed()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 3);

        task.Start();

        Assert.Equal(TaskState.Running, task.Status);
    
        Assert.Throws<DomainException>(() =>  task.ReStart());
    }

    [Fact]
    public void Cancel_only_available_for_pending_task()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 2);

        Assert.Equal(TaskState.Pending, task.Status);

        task.Cancel();

        Assert.Equal(TaskState.Cancelled, task.Status);
    }

    [Fact]
    public void Imposible_to_cancel_a_task_in_a_different_state_than_pending()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 2);

        task.Start();

        Assert.Equal(TaskState.Running, task.Status);

        Assert.Throws<DomainException>(() =>  task.Cancel());
    }

    [Fact]
    public void Archive_action_is_only_possible_for_succed_action()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 2);
        task.Start();
        task.Succeed();

        Assert.Equal(TaskState.Succeeded, task.Status);

        task.Archive();
        Assert.Equal(TaskState.Archived, task.Status);
    }
    
    [Fact]
    public void Impossible_to_Archive_if_the_task_has_not_succeed()
    {
        var task = OrchestratedTask.CreateNew(TaskType.Simulation, maxAttempts: 2);
        task.Start();

        Assert.Equal(TaskState.Running, task.Status);

        Assert.Throws<DomainException>(() => task.Archive());
    }

}
