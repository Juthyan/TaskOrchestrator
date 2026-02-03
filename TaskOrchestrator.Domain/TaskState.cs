namespace TaskOrchestrator.Domain;

public enum TaskState
{
    Pending,
    Running,
    Succeeded,
    Failed,
    Cancelled
}