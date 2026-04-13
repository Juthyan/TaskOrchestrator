namespace TaskOrchestrator.Application;

using TaskOrchestrator.Domain;

public record EnqueueTaskCommand(TaskType Type, int MaxAttempts = 3);
