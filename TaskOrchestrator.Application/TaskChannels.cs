using System.Threading.Channels;
using TaskOrchestrator.Domain;

namespace TaskOrchestrator.Application;

public class TaskChannels
{
    public Channel<OrchestratedTask> HighPriority { get; }
    public Channel<OrchestratedTask> LowPriority { get; }

    public TaskChannels()
    {
        HighPriority = Channel.CreateBounded<OrchestratedTask>(100);
        LowPriority = Channel.CreateBounded<OrchestratedTask>(100);
    }
}