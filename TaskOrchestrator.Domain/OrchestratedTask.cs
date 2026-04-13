namespace TaskOrchestrator.Domain;


public class OrchestratedTask
{
    public Guid Id { get; }

    public TaskState Status { get; private set; } = TaskState.Pending;

    public TaskType Type { get; private set; }

    public int Attempts { get; private set; }

    public int MaxAttempts { get; }

    public DateTime CreatedAtUtc { get; }
    public DateTime? LastUpdatedAtUtc { get; private set; }

    private OrchestratedTask() { }

    public OrchestratedTask(Guid id, TaskType type, int maxAttempts)
    {
        if (maxAttempts <= 0)
        {
            throw new DomainException("MaxAttempts must be greater than zero.");
        }

        Id = id;
        Type = type;
        MaxAttempts = maxAttempts;
        Status = TaskState.Pending;
        Attempts = 0;
        CreatedAtUtc = DateTime.UtcNow;
        LastUpdatedAtUtc = CreatedAtUtc;
    }

    public static OrchestratedTask CreateNew(TaskType type, int maxAttempts = 3)
    {
        return new OrchestratedTask(Guid.NewGuid(), type, maxAttempts);
    }

    public void Running()
    {
        if (Status != TaskState.Pending)
        {
            throw new DomainException("Cannot start a task that is not pending");
        }

        Status = TaskState.Running;
        Touch();
    }


    public void Succeed()
    {
        if (Status != TaskState.Running)
        {
            throw new DomainException("Cannot succeed a task that is not Running.");
        }

        Attempts++;
        Status = TaskState.Succeeded;
        Touch();

    }

    public void Failed()
    {
        if (Status != TaskState.Running)
        {
            throw new DomainException("Cannot fail a task that is not Running.");
        }

        Attempts++;
        Status = TaskState.Failed;
        Touch();
    }

    public void Cancelled()
    {
        if (Status != TaskState.Pending)
        {
            throw new DomainException("Cannot fail a task that is not Running.");
        }

        Status = TaskState.Cancelled;
        Touch();
    }

     public void Archived()
    {
        if (Status != TaskState.Succeeded || Status != TaskState.Failed)
        {
            throw new DomainException("Something happend");
        }

        Status = TaskState.Archived;
        Touch();
    }

    public void Retry()
    {
        if (Status != TaskState.Failed)
        {
            throw new DomainException($"Can only retry a task that is Failed, current status is {Status}.");
        }

        if (Attempts >= MaxAttempts)
        {
            throw new DomainException("Max attempts reached, cannot retry this task.");
        }

        Status = TaskState.Pending;
        Touch();
    }

     private void Touch()
    {
        LastUpdatedAtUtc = DateTime.UtcNow;
    }
}