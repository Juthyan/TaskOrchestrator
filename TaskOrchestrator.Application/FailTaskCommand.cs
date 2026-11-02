namespace TaskOrchestrator.Application;

public record FailTaskCommand(Guid Id, string? Reason = null);