using System.Diagnostics;

namespace TaskOrchestrator.Application;

public class TaskActivitySource
{
    private static readonly ActivitySource Source = new("TaskOrchestrator");

    public Activity? StartEnqueueTask(string taskId, string taskType)
    {
        var activity = Source.StartActivity("EnqueueTask");
        activity?.SetTag("task.id", taskId);
        activity?.SetTag("task.type", taskType);
        return activity;
    }

    public Activity? StartProcessTask(string taskId, string taskType)
    {
        var activity = Source.StartActivity("ProcessTask");
        activity?.SetTag("task.id", taskId);
        activity?.SetTag("task.type", taskType);
        return activity;
    }
}