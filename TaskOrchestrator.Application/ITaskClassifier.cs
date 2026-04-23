namespace TaskOrchestrator.Application;

public interface ITaskClassifier
{
    Task<string>  ClassifyAsync(string description, CancellationToken ct = default);

}