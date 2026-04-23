namespace TaskOrchestrator.Application;


public record ClassifyAndEnqueueTaskCommand(string Description, int MaxAttempts = 3);
