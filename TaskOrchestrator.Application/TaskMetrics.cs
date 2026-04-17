using System.Diagnostics.Metrics;

namespace TaskOrchestrator.Application;

public class TaskMetrics
{
    private readonly Counter<int> _tasksEnqueued;
    private readonly Counter<int> _tasksCompleted;
    private readonly Histogram<double> _processingDuration;

    public TaskMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("TaskOrchestrator");
        _tasksEnqueued = meter.CreateCounter<int>("tasks_enqueued_total");
        _tasksCompleted = meter.CreateCounter<int>("tasks_completed_total");
        _processingDuration = meter.CreateHistogram<double>("task_processing_duration_ms");
    }

    public void TaskEnqueued(string taskType) =>
        _tasksEnqueued.Add(1, new KeyValuePair<string, object?>("type", taskType));

    public void TaskCompleted(string status) =>
        _tasksCompleted.Add(1, new KeyValuePair<string, object?>("status", status));

    public void RecordDuration(double milliseconds) =>
        _processingDuration.Record(milliseconds);
}