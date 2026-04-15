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
            throw new DomainException("MaxAttempts must be greater than zero.");

        Id = id;
        Type = type;
        MaxAttempts = maxAttempts;
        Status = TaskState.Pending;
        Attempts = 0;
        CreatedAtUtc = DateTime.UtcNow;
        LastUpdatedAtUtc = CreatedAtUtc;
    }

    public static OrchestratedTask CreateNew(TaskType type, int maxAttempts = 3)
        => new OrchestratedTask(Guid.NewGuid(), type, maxAttempts);

    public void Start()
    {
        if (Status != TaskState.Pending)
            throw new DomainException("Cannot start a task that is not Pending.");

        Status = TaskState.Running;
        Touch();
    }

    public void Succeed()
    {
        if (Status != TaskState.Running)
            throw new DomainException("Cannot succeed a task that is not Running.");

        Attempts++;
        Status = TaskState.Succeeded;
        Touch();
    }

    public void Fail()
    {
        if (Status != TaskState.Running)
            throw new DomainException("Cannot fail a task that is not Running.");

        Attempts++;
        Status = TaskState.Failed;
        Touch();
    }

    public void Cancel()
    {
        if (Status != TaskState.Pending)
            throw new DomainException("Cannot cancel a task that is not Pending.");

        Status = TaskState.Cancelled;
        Touch();
    }

    public void Archive()
    {
        if (Status != TaskState.Succeeded && Status != TaskState.Failed)
            throw new DomainException("Cannot archive a task that is not Succeeded or Failed.");

        Status = TaskState.Archived;
        Touch();
    }

    public void Retry()
    {
        if (Status != TaskState.Failed)
            throw new DomainException($"Can only retry a Failed task, current status is {Status}.");

        if (Attempts >= MaxAttempts)
            throw new DomainException("Max attempts reached, cannot retry this task.");

        Status = TaskState.Pending;
        Touch();
    }


    public void ReStart()
    {
        if (Status != TaskState.Failed)
        throw new DomainException($"Can only restart a Failed task, current status is {Status}.");

        if (Attempts < MaxAttempts)
            throw new DomainException("Automatic retry not exhausted, cannot restart this task.");

        Status = TaskState.Pending;
        Attempts = 0;
        Touch();
    }

    private void Touch()
    {
        LastUpdatedAtUtc = DateTime.UtcNow;
    }
}
